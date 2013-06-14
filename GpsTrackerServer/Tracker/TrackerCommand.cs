using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GpsTrackerServer.Tracker
{
    /// <summary>
    /// Команда трекера
    /// </summary>
    public class TrackerCommand : DataBlock
    {
        /// <summary>
        /// Минимальный размер команды
        /// </summary>
        public new const byte MinSize = 4;

        /// <summary>
        /// Максимальный размер команды
        /// </summary>
        public new const ushort MaxSize = MinSize + 1024;

        /// <summary>
        /// Типы команд
        /// </summary>
        static readonly Dictionary<string, TrackerCommandItem> CommandTypes = new Dictionary<string, TrackerCommandItem>()
            {
                // Команды от сервера
                {"AP01", new TrackerCommandItem{Description = "Answer handshake signal message"}},
                {"AP05", new TrackerCommandItem{Description = "Device login response message"}},
                
                // Команды от трекера
                {"BP00", new TrackerCommandItem{Description = "Handshake signal message", AnswerHandlerFunction = "OnHandshakeCommand"}},
                {"BP05", new TrackerCommandItem{Description = "Login message", AnswerHandlerFunction = "OnLoginCommand"}},
                {"BR00", new TrackerCommandItem{Description = "Isochronous and continues feedback message", AnswerHandlerFunction = "OnFeedbackMessage"}},
            };

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data">данные</param>
        public TrackerCommand(byte[] data) : base(data, MinSize, MaxSize)
        {
            if (data.Length > MaxSize || data.Length < MinSize)
                throw new Exception(String.Format("Размер команды должен быть от {0} до {1} байт", MinSize, MaxSize));    

            if(!CommandTypes.ContainsKey(ToString()))
                throw new Exception(String.Format("Неверный тип команды: '{0}'", CommandFirstType));
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data">данные</param>
        public TrackerCommand(string data)
            : this(Encoding.ASCII.GetBytes(data))
        {}

        /// <summary>
        /// Первичный тип команды
        /// </summary>
        public char CommandFirstType
        {
            get { return (char)Data[0]; }
        }

        /// <summary>
        /// Вторичный тип команды
        /// </summary>
        public char CommandSecondType
        {
            get { return (char)Data[1]; }
        }

        /// <summary>
        /// Серийный номер команды
        /// </summary>
        public string CommandSerialNumber
        {
            get
            {
                // Получение данных
                var serialNumberData = Data.Skip(2).Take(2).ToArray();
                // Проверка данных, должны быть все цифры
                if (!serialNumberData.All(b => char.IsDigit((char)b)))
                    throw new Exception("Неверный формат серийного номера команды");
                // Получение строки
                var serialNumberString = Encoding.ASCII.GetString(serialNumberData);
                return serialNumberString;
            }
        }

        /// <summary>
        /// Тип команды
        /// </summary>
        public TrackerCommandItem CommandType 
        {
            get
            {
                try
                {
                    return CommandTypes[ToString()];
                }
                catch
                {
                    throw new Exception(String.Format("Неверный тип команды: '{0}'", ToString()));
                }
            }
        }

        public new string ToString()
        {
            return String.Format("{0}{1}{2}", CommandFirstType, CommandSecondType, CommandSerialNumber);
        }
        

    }
}
