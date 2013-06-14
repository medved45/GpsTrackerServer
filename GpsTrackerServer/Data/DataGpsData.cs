using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;

namespace GpsTrackerServer.Data
{
    [Table(Name = "T_GPS_DATA")]
    public class DataGpsData
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id;

        /// <summary>
        /// Трекер
        /// </summary>
        [Column(Name = "ID_TRACKER")]
        public int TrackerId;

        /// <summary>
        /// Дата с GPS приемника
        /// </summary>
        [Column(Name = "C_GPS_DATE")]
        public DateTime GpsDate;

        /// <summary>
        /// Дата создания записи
        /// </summary>
        [Column(Name = "C_CREATE_DATE")]
        public DateTime CreateDate;

        /// <summary>
        /// Доступность данных, "A" доступно, "V" - недоступно
        /// </summary>
        [Column(Name = "C_AVAILABILITY_DATA")]
        public char AvailabilityData;

        /// <summary>
        /// Широта
        /// </summary>
        [Column(Name = "C_LATITUDE")]
        public double Latitude;

        /// <summary>
        /// Широта Север(N) или Юг(S)
        /// </summary>
        [Column(Name = "C_LATITUDE_INDICATOR")]
        public char LatitudeIndicator;

        /// <summary>
        /// Долгота
        /// </summary>
        [Column(Name = "C_LONGITUDE")]
        public double Longitude;

        /// <summary>
        /// Долгота Запад(W) или Восток(E)
        /// </summary>
        [Column(Name = "C_LONGITUDE_INDICATOR")]
        public char LongitudeIndicator;

        /// <summary>
        /// Скорость
        /// </summary>
        [Column(Name = "C_SPEED")]
        public double Speed;

        /// <summary>
        /// Напрваление
        /// </summary>
        [Column(Name = "C_ORIENTATION")]
        public double Orientation;

        /// <summary>
        /// Состояние I/O
        /// </summary>
        [Column(Name = "C_IOSTATE")]
        public string IoState;

        /// <summary>
        /// MilePost
        /// </summary>
        [Column(Name = "C_MILE_POST")]
        public char MilePost;

        /// <summary>
        /// MileData
        /// </summary>
        [Column(Name = "C_MILE_DATA")]
        public string MileData;
    }
}
