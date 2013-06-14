using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GpsTrackerServer.Data
{
    public class DataLocker : IDisposable
    {
        /// <summary>
        /// Таймаут ожидания разблокировки по умолчанию
        /// </summary>
        private const int DefaultTimeout = 5000;

        /// <summary>
        /// Блокировки: Таблица->ID записи->Блокировочный объект
        /// </summary>
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<int, object>> RowLocks = new ConcurrentDictionary<Type, ConcurrentDictionary<int, object>>();

        /// <summary>
        /// Текущая таблица блокировки
        /// </summary>
        private readonly Type _type;

        /// <summary>
        /// Текущий номер записи для блокировки
        /// </summary>
        private readonly int _rowId;

        /// <summary>
        /// Внутренний объект блокировки
        /// </summary>
        private static readonly object InternalLocker = new object();

        /// <summary>
        /// Флаг
        /// </summary>
        public bool Safe { get; private set; }

        /// <summary>
        /// Конструктор блокировки
        /// </summary>
        /// <param name="type">тип таблицы</param>
        /// <param name="rowId">номер строки</param>
        /// <param name="milliSecondTimeout">таймаут</param>
        private DataLocker(Type type, int rowId, int milliSecondTimeout)
        {
            object locker;

            lock (InternalLocker)
            {
                Safe = false;
                _type = type;
                _rowId = rowId;
                ConcurrentDictionary<int, object> rows;
                if (!RowLocks.TryGetValue(_type, out rows))
                {
                    rows = new ConcurrentDictionary<int, object>();
                    RowLocks.TryAdd(_type, rows);
                }


                if (!rows.TryGetValue(_rowId, out locker))
                {
                    locker = new object();
                    rows.TryAdd(_rowId, locker);
                }
            }

            Safe = Monitor.TryEnter(locker, milliSecondTimeout);
        }

        /// <summary>
        /// Блокировка
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rowId"></param>
        /// <param name="millisecondTimeout"></param>
        /// <param name="codeToRun"></param>
        public static void Lock(Type type, int rowId, Action codeToRun, int millisecondTimeout = DefaultTimeout)
        {
            using (var bolt = new DataLocker(type, rowId, millisecondTimeout))
            {
                if (bolt.Safe)
                    codeToRun();
                else throw new TimeoutException(string.Format("Не удалось получить монопольный доступ к таблице {0}, к строке id: {1} в течение {2}мс",
                                                             type, rowId, millisecondTimeout));
            }

        }

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (Safe)
            {
                lock (InternalLocker)
                {
                    ConcurrentDictionary<int, object> rows;
                    if (RowLocks.TryGetValue(_type, out rows))
                    {
                        object locker;
                        if (rows.TryGetValue(_rowId, out locker))
                        {
                            Monitor.Exit(locker);
                            rows.TryRemove(_rowId, out locker);
                        }
                    }
                    Safe = false;
                }
            }
        }

        #endregion
    }
    
}
