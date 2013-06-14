using System;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using GpsTrackerServer.Data;

namespace GpsTrackerServer.Tracker
{
    /// <summary>
    /// Статусы трекера
    /// </summary>
    public enum TrackerStatus
    {
        Enabled,
        Disabled,
        Fault
    }

    /// <summary>
    /// Статусы соединения трекера
    /// </summary>
    public enum TrackerConnectionStatus
    {
        Offline,
        Online
    }

    /// <summary>
    /// GPS трекер
    /// </summary>
    public class GpsTracker : IDisposable
    {
        /// <summary>
        /// Логгер
        /// </summary>
        protected log4net.ILog Logger;

        /// <summary>
        /// Максимальное количество байт, при чтении данных из TCP клиента
        /// </summary>
        private const int MaxReadDataSize = 4096;

        /// <summary>
        /// Флаг disposed
        /// </summary>
        protected bool IsDisposed;

        /// <summary>
        /// Данные трекера в БД
        /// </summary>
        protected DataGpsTracker GpsTrackerData;

        /// <summary>
        /// TCP клиент, по которому кодключен трекер
        /// </summary>
        protected TcpClient TcpClient;

        /// <summary>
        /// Событие остановки работы трекера
        /// </summary>
        protected ManualResetEvent StopEvent;

        /// <summary>
        /// Событие готовности трекера принимать команды
        /// </summary>
        protected ManualResetEvent ReadyEvent;

        /// <summary>
        /// Флаг готовности трекера
        /// </summary>
        public bool Ready
        {
            get { return ReadyEvent.WaitOne(0); }
        }

        /// <summary>
        /// Поток сервера для потока данных
        /// </summary>
        protected Thread DataThread;

        /// <summary>
        /// Идентификатор трекера
        /// </summary>
        public string DeviceId { 
            get {
                return GpsTrackerData == null ? "*" : GpsTrackerData.DeviceId;
            }
        }


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tcpClient">TCP клиент, ассоциированный с трекером</param>
        public GpsTracker(TcpClient tcpClient)
        {
            log4net.Config.XmlConfigurator.Configure();

            if(!tcpClient.Connected)
                throw new Exception("Невозможно создать трекер без подключенного TCP клиента");
            TcpClient = tcpClient;
            // создание логгера
            Logger = log4net.LogManager.GetLogger(String.Format("{0} [{1}:{2}]", MethodBase.GetCurrentMethod().DeclaringType, TcpClient.Client.RemoteEndPoint, DeviceId));
            Logger.Info("Инициализация трекера...");

            // Инициализация события остановки
            StopEvent = new ManualResetEvent(false);

            // Инициализация события готовности трекера
            ReadyEvent = new ManualResetEvent(false);

            // запуск основного потока
            DataThread = new Thread(ReadData);
            DataThread.Start();
        }

        /// <summary>
        /// Ожидание готовности трекера
        /// </summary>
        /// <param name="timeout">таймаут</param>
        /// <returns></returns>
        public bool WaitForReady(int timeout)
        {
            return ReadyEvent.WaitOne(timeout);
        }

        /// <summary>
        /// Основной поток сервера
        /// </summary>
        private void ReadData()
        {
            try
            {
                // Установка максимального размера буффера данных
                TcpClient.ReceiveBufferSize = MaxReadDataSize;
                // Получение потока данных от TCP клиента
                var clientStream = TcpClient.GetStream();
                // Буфер данных
                var data = new byte[TcpClient.ReceiveBufferSize];
                
                while (!StopEvent.WaitOne(0) && TcpClient.Connected)
                {
                    try
                    {
                        //Установка исходного размера буфера данных
                        Array.Resize(ref data, TcpClient.ReceiveBufferSize);
                        // Чтение данных
                        var bytesRead = clientStream.Read(data, 0, TcpClient.ReceiveBufferSize);
                        //Установка размера буфера данных исходя из полученных данных
                        Array.Resize(ref data, bytesRead);
                        Logger.InfoFormat("Получено {0} байт", bytesRead);
                    }
                    catch (System.IO.IOException ex)
                    {
                        Logger.Info("Произошло отключение TCP клиента");
                        break;
                    }
                    catch(Exception ex)
                    {
                        Logger.Error("Ошибка чтения данных TCP клиента", ex);
                        break;
                    }

                    try
                    {
                        // Обработка фрейма
                        OnReceiveFrame(new TrackerFrame(data));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(String.Format("Ошибка обработки полученного фрейма {0}", Encoding.ASCII.GetString(data)), ex);
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Error("Произошла ошибка в потоке чтения данных TCP клиента", ex);
            }
        }

        /// <summary>
        /// Отправка фрейма данных
        /// </summary>
        /// <param name="command"></param>
        /// <param name="messageBody"></param>
        protected void SendFrame(TrackerCommand command, string messageBody)
        {
            TrackerFrame frame = null;
            try
            {
                frame = CreateFrame(command, messageBody);
                Logger.InfoFormat("Отправка фрейма {0}", frame.ToString());
                TcpClient.Client.Send(frame.BlockData);
            }
            catch (Exception ex)
            {
                Logger.Error(String.Format("Ошибка при отправке фрейма {0}", frame == null ? "(фрейм не создан)" : frame.ToString()), ex);
            }

        }

        /// <summary>
        /// Отправка фрейма данных
        /// </summary>
        /// <param name="command"></param>
        /// <param name="messageBody"></param>
        protected void SendFrame(string command, string messageBody)
        {
            SendFrame(new TrackerCommand(command), messageBody);
        }

        /// <summary>
        /// Отправка фрейма данных
        /// </summary>
        /// <param name="command"></param>
        protected void SendFrame(TrackerCommand command)
        {
            SendFrame(command, "");
        }

        /// <summary>
        /// Отправка фрейма данных
        /// </summary>
        /// <param name="command"></param>
        protected void SendFrame(string command)
        {
            SendFrame(command, "");
        }

        /// <summary>
        /// Создание фрейма команды
        /// </summary>
        /// <param name="command"></param>
        /// <param name="messageBody"></param>
        /// <returns></returns>
        public TrackerFrame CreateFrame(TrackerCommand command, string messageBody)
        {
            return TrackerFrame.NewFrame(DeviceId, command, messageBody);
        }

        /// <summary>
        /// Получение фрейма
        /// </summary>
        /// <param name="frame">фрейм</param>
        protected void OnReceiveFrame(TrackerFrame frame)
        {
            Logger.DebugFormat("Получен фрейм {0}, команда {1}", frame.ToString(), frame.Command.CommandType.Description);

            // Проверка 
            CheckHandshake(frame);
            
            // Если обработчик ответа не указан то выходим
            if (String.IsNullOrEmpty(frame.Command.CommandType.AnswerHandlerFunction))
                return;

            // Имем обработчик
            var handler = GetType().GetMethod(frame.Command.CommandType.AnswerHandlerFunction);
            if (handler == null)
            {
                throw new Exception(String.Format("Функция обработки ответа не найдена: {0}",
                                   frame.Command.CommandType.AnswerHandlerFunction));
            }
            // Вызов обработчика
            handler.Invoke(this, new object[]{frame});
        }

        /// <summary>
        /// Проверяем есть ли в нашей базе запись о данном трекере
        /// </summary>
        /// <param name="frame"></param>
        protected void CheckHandshake(TrackerFrame frame)
        {
            // Если есть данные по трекеру значит все хорошо
            if (GpsTrackerData != null)
                return;

            using (var dc = new GpsTrackerDataContext())
            {
                var tracker =
                    (from obj in dc.GpsTracker where obj.DeviceId == frame.DeviceIdString select obj).FirstOrDefault();
                // Если вообще  в базе нет трекера с таким ID то создаем его
                if (tracker == null)
                {
                    Logger.InfoFormat("Подключенный трекер не обнаружен в базе данных. В базе данных будет создан новый трекер (id {0}).", frame.DeviceIdString);
                    var dataGpsTracker = new DataGpsTracker()
                        {
                            Phone = frame.DeviceIdString,
                            DeviceId = frame.DeviceIdString,
                            ResponseInterval = 30,
                            Comment = "Автоматически создан сервером",
                            ConnectionStatus = TrackerConnectionStatus.Online.ToString(),
                            Status = TrackerStatus.Disabled.ToString(),
                            Name = String.Format("tracker#{0}", frame.DeviceIdString)
                        };
                    DataCrud.CreateGpsTracker(dataGpsTracker);
                }
                else
                {
                    GpsTrackerData = tracker;
                    GpsTrackerData.ConnectionStatus = TrackerConnectionStatus.Online.ToString();
                    DataCrud.UpdateGpsTracker(GpsTrackerData);
                }
                // Переименование логгера
                Logger = log4net.LogManager.GetLogger(String.Format("{0} [{1}:{2}]", MethodBase.GetCurrentMethod().DeclaringType, TcpClient.Client.RemoteEndPoint, DeviceId));
            }

        }

        /// <summary>
        /// Рукопожатие
        /// </summary>
        /// <param name="frame"></param>
        public void OnHandshakeCommand(TrackerFrame frame)
        {
            // Согласно спецификации
            SendFrame("AP01", "HSO");
        }

        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="frame"></param>
        public void OnLoginCommand(TrackerFrame frame)
        {
            // Согласно спецификации
            SendFrame("AP05");
        }

        public void OnFeedbackMessage(TrackerFrame frame)
        {
            try
            {
                var gpsLocation = new GpsLocation(frame.MessageBody);
                Logger.InfoFormat("Получены координаты трекера: {0}", gpsLocation.ToString());
                var gpsData = new DataGpsData
                    {
                        TrackerId = GpsTrackerData.Id,
                        GpsDate = gpsLocation.Date,
                        CreateDate = DateTime.UtcNow,
                        Latitude = gpsLocation.Latitude,
                        LatitudeIndicator = gpsLocation.LatitudeIndicator,
                        Longitude = gpsLocation.Longitude,
                        LongitudeIndicator = gpsLocation.LongitudeIndicator,
                        Speed = gpsLocation.Speed,
                        Orientation = gpsLocation.Orientation,
                        AvailabilityData = gpsLocation.AvailabilityData,
                        IoState = gpsLocation.IoState,
                        MilePost = gpsLocation.MilePost,
                        MileData = gpsLocation.MileData,
                    };
                DataCrud.CreateGpsData(gpsData);
                
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка при обработке полученных координат", ex);
            }
        }

        /// <summary>
        /// Disposing
        /// </summary>
        public void Dispose()
        {

            if (!IsDisposed)
            {
                Logger.Info("Остановка трекера...");
                // Сигналим об остановке сервера
                StopEvent.Set();

                // Обновляем состояние подключения трекера
                if (GpsTrackerData != null)
                {
                    GpsTrackerData.ConnectionStatus = TrackerConnectionStatus.Offline.ToString();
                    DataCrud.UpdateGpsTracker(GpsTrackerData);
                }

                // Закрываем соединение
                TcpClient.Close();

                // Ждем максимум 10 секунд пока основной поток остановиться
                var timeElapsed = 0;
                while (DataThread != null && DataThread.IsAlive)
                {
                    Thread.Sleep(100);
                    if (timeElapsed++ == 100)
                        DataThread.Abort();
                }

                Logger.Info("Трекер остановлен");

                IsDisposed = true;
            }

        }
    }
}
