using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GpsTrackerServer.Tracker
{
    public class TrackerCommandItem
    {
        /// <summary>
        /// Описание команды
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Название функции для обработки ответа
        /// </summary>
        public string AnswerHandlerFunction { get; set; }
    }
}
