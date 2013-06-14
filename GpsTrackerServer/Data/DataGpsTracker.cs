using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;

namespace GpsTrackerServer.Data
{
    [Table(Name = "T_GPS_TRACKER")]
    public class DataGpsTracker
    {
        [Column(IsPrimaryKey = true, IsDbGenerated = true)]
        public int Id;

        /// <summary>
        /// Номер телефона трекера
        /// </summary>
        [Column(Name = "C_PHONE")]
        public string Phone;

        /// <summary>
        /// Интервал в секундах между отправкой координат на сервер
        /// </summary>
        [Column(Name = "C_RESPONSE_INTERVAL")]
        public int ResponseInterval;

        /// <summary>
        /// Статус соединения трекера
        /// </summary>
        [Column(Name = "C_CONNECTION_STATUS")]
        public string ConnectionStatus;

        /// <summary>
        /// Идентификатор трекера (обычно номер телефона)
        /// </summary>
        [Column(Name = "C_DEVICE_ID")]
        public string DeviceId;

        /// <summary>
        /// Статус трекера
        /// </summary>
        [Column(Name = "C_STATUS")]
        public string Status;

        /// <summary>
        /// Название трекера
        /// </summary>
        [Column(Name = "C_NAME")]
        public string Name;

        /// <summary>
        /// IMEI код трекера
        /// </summary>
        [Column(Name = "C_IMEI")]
        public string Imei;

        /// <summary>
        /// Дата создания
        /// </summary>
        [Column(Name = "C_DATE_CREATED")]
        public DateTime? DateCreated;

        /// <summary>
        /// Комментарий
        /// </summary>
        [Column(Name = "C_COMMENT")]
        public string Comment;
    }
}
