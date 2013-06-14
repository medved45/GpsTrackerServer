using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace GpsTrackerServer.Data
{
    public class GpsTrackerDataContext : DataContext
    {
        private static readonly MappingSource MappingSource = new AttributeMappingSource();

        protected readonly log4net.ILog Logger = log4net.LogManager.GetLogger
        (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string GetConnectionString()
        {
            try
            {
                return ConfigurationManager.AppSettings["DbConnectionString"];
            }
            catch
            {
                return "";
            }
        }

        public GpsTrackerDataContext()
            : base(GetConnectionString(), MappingSource)
        {
            this.ConnectionString = base.Connection.ConnectionString;
            ObjectTrackingEnabled = true;
        }

        public string ConnectionString { get; set; }

        public GpsTrackerDataContext(string connection)
            : base(connection, MappingSource)
        {
        }

        /// <summary>
        /// Таблица с трекерами
        /// </summary>
        public Table<DataGpsTracker> GpsTracker
        {
            get { return GetTable<DataGpsTracker>(); }
        }

        /// <summary>
        /// Таблица с данными трекеров
        /// </summary>
        public Table<DataGpsData> GpsData
        {
            get { return GetTable<DataGpsData>(); }
        }
    }
}
