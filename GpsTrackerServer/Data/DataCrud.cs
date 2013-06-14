using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GpsTrackerServer.Data
{
    public static class DataCrud
    {
        #region Трекер CRUD
        /// <summary>
        /// Изменение трекера
        /// </summary>
        /// <param name="data">объект данных</param>
        public static void UpdateGpsTracker(DataGpsTracker data)
        {
            DataLocker.Lock(typeof(DataGpsTracker), data.Id, () =>
            {
                DataGpsTracker obj;
                using (var dc = new GpsTrackerDataContext())
                {
                    obj = (from _obj in dc.GpsTracker where _obj.Id == data.Id select _obj).FirstOrDefault();
                    if (obj == null)
                        throw new KeyNotFoundException(String.Format("{0}: Запись с таким id:{1} не найдена",
                                                                     MethodBase.GetCurrentMethod(), data.Id));
                }

                using (var dc = new GpsTrackerDataContext())
                {
                    dc.GpsTracker.Attach(data, obj);
                    dc.SubmitChanges();
                }
            });

        }

        /// <summary>
        /// Добавление трекера
        /// </summary>
        /// <param name="data">объект данных</param>
        public static void CreateGpsTracker(DataGpsTracker data)
        {
            using (var dc = new GpsTrackerDataContext())
            {
                data.DateCreated = DateTime.UtcNow;
                dc.GpsTracker.InsertOnSubmit(data);
                dc.SubmitChanges();
            }
        }

        /// <summary>
        /// Удаление трекера
        /// </summary>
        /// <param name="id">id объекта</param>
        /// <returns>true если все хорошо, false если объект с таким id не найден или произошло что-то плохое</returns>
        public static void DeleteGpsTracker(int id)
        {
            using (var dc = new GpsTrackerDataContext())
            {
                var obj =
                    (from _obj in dc.GpsTracker where _obj.Id == id select _obj).FirstOrDefault();
                if (obj == null)
                    throw new KeyNotFoundException(String.Format("{0}: Запись с таким id:{1} не найдена", MethodBase.GetCurrentMethod(), id));
                dc.GpsTracker.DeleteOnSubmit(obj);
                dc.SubmitChanges();
            }

        }

        #endregion

        #region Данные трекера CRUD
        /// <summary>
        /// Изменение данных трекера
        /// </summary>
        /// <param name="data">объект данных</param>
        public static void UpdateGpsData(DataGpsData data)
        {
            DataLocker.Lock(typeof(DataGpsData), data.Id, () =>
            {
                DataGpsData obj;
                using (var dc = new GpsTrackerDataContext())
                {
                    obj = (from _obj in dc.GpsData where _obj.Id == data.Id select _obj).FirstOrDefault();
                    if (obj == null)
                        throw new KeyNotFoundException(String.Format("{0}: Запись с таким id:{1} не найдена",
                                                                     MethodBase.GetCurrentMethod(), data.Id));
                }

                using (var dc = new GpsTrackerDataContext())
                {
                    dc.GpsData.Attach(data, obj);
                    dc.SubmitChanges();
                }
            });

        }

        /// <summary>
        /// Добавление данных трекера
        /// </summary>
        /// <param name="data">объект данных</param>
        public static void CreateGpsData(DataGpsData data)
        {
            using (var dc = new GpsTrackerDataContext())
            {
                data.CreateDate = DateTime.UtcNow;
                dc.GpsData.InsertOnSubmit(data);
                dc.SubmitChanges();
            }
        }

        /// <summary>
        /// Удаление данных трекера
        /// </summary>
        /// <param name="id">id объекта</param>
        /// <returns>true если все хорошо, false если объект с таким id не найден или произошло что-то плохое</returns>
        public static void DeleteGpsData(int id)
        {
            using (var dc = new GpsTrackerDataContext())
            {
                var obj =
                    (from _obj in dc.GpsData where _obj.Id == id select _obj).FirstOrDefault();
                if (obj == null)
                    throw new KeyNotFoundException(String.Format("{0}: Запись с таким id:{1} не найдена", MethodBase.GetCurrentMethod(), id));
                dc.GpsData.DeleteOnSubmit(obj);
                dc.SubmitChanges();
            }

        }

        #endregion
    }
}
