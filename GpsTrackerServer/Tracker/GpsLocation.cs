using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GpsTrackerServer.Tracker
{
    /// <summary>
    /// Данные GPS, полученные от трекера
    /// </summary>
    public class GpsLocation : DataBlock
    {
        /// <summary>
        /// Размер данных
        /// </summary>
        public const byte Size = 62;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="data"></param>
        public GpsLocation(byte[] data) : base(data, Size, Size )
        {
        }

        /// <summary>
        /// Дата трека
        /// </summary>
        public DateTime Date
        {
            get
            {
                var date = Encoding.ASCII.GetString(Data.Take(6).ToArray());
                var time = Encoding.ASCII.GetString(Data.Skip(33).Take(6).ToArray());
                try
                {
                    return DateTime.ParseExact(String.Format("{0} {1}", date, time), "yyMMdd HHmmss",
                                               CultureInfo.InvariantCulture);
                }
                catch(Exception ex)
                {
                    throw new Exception(String.Format("Не удалось разобрать блок даты {0} и времени {1}", date, time), ex);
                }
            }
        }

        /// <summary>
        /// Доступность данных, "A" доступно, "V" - недоступно
        /// </summary>
        public char AvailabilityData
        {
            get { return (char) Data[6]; }
        }

        /// <summary>
        /// Широта
        /// </summary>
        public double Latitude
        {
            get { return Helper.Degrees2Digit(Convert.ToDouble(Encoding.ASCII.GetString(Data.Skip(7).Take(9).ToArray()), CultureInfo.InvariantCulture) / 100.0); }
        }

        /// <summary>
        /// Широта Север(N) или Юг(S)
        /// </summary>
        public char LatitudeIndicator
        {
            get { return (char)Data[16]; }
        }

        /// <summary>
        /// Долгота
        /// </summary>
        public double Longitude
        {
            get { return Helper.Degrees2Digit(Convert.ToDouble(Encoding.ASCII.GetString(Data.Skip(17).Take(10).ToArray()), CultureInfo.InvariantCulture) / 100.0); }
        }

        /// <summary>
        /// Долгота Запад(W) или Восток(E)
        /// </summary>
        public char LongitudeIndicator
        {
            get { return (char)Data[27]; }
        }

        /// <summary>
        /// Скорость
        /// </summary>
        public double Speed
        {
            get
            {
                return Convert.ToDouble(Encoding.ASCII.GetString(Data.Skip(28).Take(5).ToArray()), CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Направление
        /// </summary>
        public double Orientation
        {
            get
            {
                return Convert.ToDouble(Encoding.ASCII.GetString(Data.Skip(39).Take(6).ToArray()), CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Стостояние I/O
        /// </summary>
        public string IoState
        {
            get
            {
                return Encoding.ASCII.GetString(Data.Skip(45).Take(8).ToArray());
            }
        }

        /// <summary>
        /// MilePost
        /// </summary>
        public char MilePost
        {
            get { return (char)Data[53]; }
        }

        /// <summary>
        /// Mile Data
        /// </summary>
        public string MileData
        {
            get { return Encoding.ASCII.GetString(Data.Skip(54).Take(8).ToArray()); }
        }

        public new string ToString()
        {
            return String.Format("{0}{1}, {2}{3}", Latitude, LatitudeIndicator, Longitude, LongitudeIndicator);
        }
    }
}
