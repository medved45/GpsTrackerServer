using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GpsTrackerServer.Tracker
{
    /// <summary>
    /// Фрейм данных трекера
    /// </summary>
    public class TrackerFrame : DataBlock
    {
        /// <summary>
        /// Минимальный размер фрейма
        /// </summary>
        public new const byte MinSize = 18;

        /// <summary>
        /// Максимальный размер фрейма
        /// </summary>
        public new const ushort MaxSize = MinSize + 1024;

        /// <summary>
        /// Флаг начала '('
        /// </summary>
        public const byte BeginFrame = 0x28;

        /// <summary>
        /// Флаг окончания ')'
        /// </summary>
        public const byte EndFrame = 0x29;

        /// <summary>
        /// Разделитель для строки представления hex
        /// </summary>
        public const char HexSeparator = ' ';

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data">данные</param>
        public TrackerFrame(byte[] data) : base(data, MinSize, MaxSize)
        {
            if (data.Length > MaxSize || data.Length < MinSize)
                throw new Exception(String.Format("Размер фрейма должен быть от {0} до {1} байт", MinSize, MaxSize));    

            if(data.First() != BeginFrame)
                throw new Exception("Неверный флаг начала фрейма");

            if (data.Last() != EndFrame)
                throw new Exception("Неверный флаг окончания фрейма");

            // Проверка формата команды
            try
            {
                Command.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Ошибка создания фрейма {0}", Encoding.ASCII.GetString(data)), ex);
            }
        }

        /// <summary>
        /// Создание нового фрейма
        /// </summary>
        /// <param name="deviceId">id устройства</param>
        /// <param name="command">команда</param>
        /// <param name="messageBody">даннфе команды</param>
        /// <returns></returns>
        public static TrackerFrame NewFrame(string deviceId, TrackerCommand command, string messageBody)
        {
            var data = new List<byte> {BeginFrame, Convert.ToByte('0')};
            data.AddRange(Encoding.ASCII.GetBytes(deviceId));
            data.AddRange(command.BlockData);
            data.AddRange(Encoding.ASCII.GetBytes(messageBody));
            data.Add(EndFrame);
            return new TrackerFrame(data.ToArray());
        }

        /// <summary>
        /// Id устройства в виде строки
        /// </summary>
        public string DeviceIdString
        {
            get
            {
                // Получение данных
                var deviceIdData = Data.Skip(2).Take(11).ToArray();
                // Проверка данных, должн ыбыть все цифры
                if (!deviceIdData.All(b => char.IsDigit((char)b)))
                    throw new Exception("Неверный формат идентификатора устройства");
                // Получение строки
                return Encoding.ASCII.GetString(deviceIdData);
            }
        }

        /// <summary>
        /// Id устройства
        /// </summary>
        public int DeviceId
        {
            get
            {
                return int.Parse(DeviceIdString);
            }
        }

        /// <summary>
        /// Команда
        /// </summary>
        public byte[] CommandData
        {
            get
            {
                return Data.Skip(13).Take(4).ToArray();
            }
        }

        /// <summary>
        /// Команда в виде строки
        /// </summary>
        public string CommandString
        {
            get
            {
                return Encoding.ASCII.GetString(CommandData);
            }
        }

        /// <summary>
        /// Тело команды
        /// </summary>
        public byte[] MessageBody
        {
            get
            {
                return Data.Skip(17).Take(Data.Length - 18).ToArray();
            }
        }

        /// <summary>
        /// Тело команды  в виде строки
        /// </summary>
        public string MessageBodyString
        {
            get
            {
                return Encoding.ASCII.GetString(MessageBody);
            }
        }

        /// <summary>
        /// Команда фрейма
        /// </summary>
        public TrackerCommand Command
        {
            get
            {
                return new TrackerCommand(CommandData);
            }
        }

        public new string ToString()
        {
            return DeviceIdString + Command.ToString() + MessageBodyString;
        }
    }
}
