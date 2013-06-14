using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GpsTrackerServer.Data;
using GpsTrackerServer.Tracker;

namespace GpsTrackerServer.Server
{
    public class TrackerServer : IDisposable
    {
        /// <summary>
        /// Логгер
        /// </summary>
        protected readonly log4net.ILog Logger = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Флаг disposed
        /// </summary>
        protected bool IsDisposed;

        /// <summary>
        /// TCP прослушиватель
        /// </summary>
        protected TcpListener TcpListener;

        /// <summary>
        /// Основной поток сервера
        /// </summary>
        protected Thread ListenThread;

        /// <summary>
        /// Событие остановки работы сервера
        /// </summary>
        protected ManualResetEvent StopEvent;

        /*
        /// <summary>
        /// Служебный таймер для периодического отслеживания трекеров
        /// </summary>
        private readonly Timer _timer;
        */

        /// <summary>
        /// Список работающих трекеров
        /// </summary>
        public List<GpsTracker> TrackerList { get; private set; }
        
        /// <summary>
        /// Ctor
        /// </summary>
        public TrackerServer()
        {
            log4net.Config.XmlConfigurator.Configure();

            Logger.Info("Инициализация сервера...");

            // Инициализация события остановки
            StopEvent = new ManualResetEvent(true);

            // Создание списка трекеров
            TrackerList = new List<GpsTracker>();

            //_timer = new Timer(OnTimer);

        }

        /// <summary>
        /// Запуск сервера
        /// </summary>
        public void StartServer()
        {
            Logger.Info("Запуск сервера...");
            try
            {
                if (!StopEvent.WaitOne(0))
                    throw new Exception("Сервер уже запущен");

                // Сброс события остановки
                StopEvent.Reset();

                // очистка списка трекеров
                TrackerList.Clear();

                TcpListener =
                    new TcpListener(new IPEndPoint(IPAddress.Parse(ConfigurationManager.AppSettings["ServerAddress"]),
                                                   Convert.ToInt32(ConfigurationManager.AppSettings["ServerPort"])));
                // запуск основного потока
                ListenThread = new Thread(ListenForClients);
                ListenThread.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка запуска сервера", ex);
            }
            
        }

        /// <summary>
        /// Остановка сервера
        /// </summary>
        public void StopServer()
        {
            Logger.Info("Остановка сервера...");
            // Сигналим об остановке сервера
            StopEvent.Set();

            // Закрываем порт
            TcpListener.Stop();

            // Ждем максимум 10 секунд пока основной поток остановиться
            var timeElapsed = 0;
            while (ListenThread != null && ListenThread.IsAlive)
            {
                Thread.Sleep(100);
                if (timeElapsed++ == 100)
                    ListenThread.Abort();
            }

            // Остановка трекеров
            TrackerList.ForEach(t=>t.Dispose());
            

            Logger.Info("Сервер остановлен");
        }

        /// <summary>
        /// Основной поток сервера
        /// </summary>
        private void ListenForClients()
        {
            Logger.InfoFormat("Прослушивание {0}:{1}", IPAddress.Parse(((IPEndPoint)TcpListener.LocalEndpoint).Address.ToString()), ((IPEndPoint)TcpListener.LocalEndpoint).Port);

            try
            {
                // Запуск прослушивания
                TcpListener.Start();

                // Выполнение цикла пока не поступит сигнал
                while (!StopEvent.WaitOne(0))
                {
                    // Если никто не хочет подрубится то ждем некторое время
                    if (TcpListener.Pending() != true)
                    {
                        Thread.Sleep(500);
                        continue;
                    }

                    // Данная команда вернет результат только при подключении клиента
                    TcpClient client = TcpListener.AcceptTcpClient();
                    Logger.InfoFormat("Новое подключение по TCP, адрес клиента: {0}", client.Client.RemoteEndPoint);

                    try
                    {
                        var gpsTracker = new GpsTracker(client);
                        TrackerList.Add(gpsTracker);
                    }
                    catch(Exception ex)
                    {
                        Logger.Error("Не удалось создать трекер", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Во время прослушивания TCP порта произошла ошибка", ex);
            }
        }
        /*
        /// <summary>
        /// Отслеживание трекеров
        /// </summary>
        /// <param name="state"></param>
        private void OnTimer(object state)
        {
            if (!StopEvent.WaitOne(0))
                UpdateTrackerList();
        }

        /// <summary>
        /// Добавляет рабочие контроллеры, если они уже не добавлены ранее, удаляет нерабочие контроллеры
        /// </summary>
        public void UpdateTrackerList()
        {
            try
            {
                // выбираем из базы все трекеры
                using (var dc = new GpsTrackerDataContext())
                {
                    foreach (var tracker in dc.GpsTracker)
                    {
                        // удаляем нерабочие трекеры
                        if (tracker.Status != TrackerStatus.Enabled.ToString())
                        {
                            if (TrackerList.ContainsKey(tracker.Id))
                            {
                                // Ждем пока освободиться трекер
                                if (TrackerList.ContainsKey(tracker.Id))
                                    TrackerList[tracker.Id].WaitForReady(-1);
                                
                                TrackerList[tracker.Id].Dispose();
                                TrackerList.Remove(tracker.Id);
                            }
                        }
                        // добавляем рабочие контроллеры, если они уже не добавлены ранее
                        else if (!TrackerList.ContainsKey(tracker.Id))
                        {
                            try
                            {
                                TrackerList.Add(tracker.Id, new Controller(tracker, this));
                            }
                            catch (Exception ex)
                            {
                                Log.WarnFormat(
                                    "При добавлении контроллера ({0}) в есть произошла ошибка: {1}. Контроллер будет отключен.",
                                    tracker.ToString(), ex.Message);
                                tracker.Status = ControllerStatus.Fault.ToString();
                                DataCrud.UpdateController(tracker, Helper.SystemUsername);

                            }

                        }
                    }
                }

            }
            catch (SqlException ex)
            {
                var interval = Int32.Parse(ConfigurationManager.AppSettings["SqlExceptionWaitInterval"]);
                Log.ErrorFormat("Ошибка sql сервера при обновлении списка контроллеров. {0}. Будет произведена повторная попытка через {1} секунд. ", ex.Message, interval / 1000);
                StopEvent.WaitOne(interval, true);
            }
            catch (Exception ex)
            {
                var interval = Int32.Parse(ConfigurationManager.AppSettings["ExceptionWaitInterval"]);
                Log.ErrorFormat("Ошибка при обновлении списка контроллеров. {0}. Будет произведена повторная попытка через {1} секунд. ", ex.Message, interval / 1000);
                StopEvent.WaitOne(interval, true);
            }


        }
        */
        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {

            if (!IsDisposed)
            {
                IsDisposed = true;
                StopEvent.Set();
                Logger.Info("Отключение трекера...");
                //_timer.Dispose();
            }
            
        }
    }
}
