using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace GpsTrackerServer
{
    static class Helper
    {
        public static class SkdPerformanceCounter
        {
            public const string CategoryName = "GpsTrackerServer";
            public const string CommunicationQuality = "Quality of communication";
        }

        public static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

        public const string SystemUsername = "server";
        public const string UnknownUsername = "unknown";
        
        static Helper()
        {
            log4net.Config.XmlConfigurator.Configure();
            /*
            // Создание счетчиков
            try
            {
                if (!PerformanceCounterCategory.Exists(SkdPerformanceCounter.CategoryName))
                    PerformanceCounterCategory.Create(
                        SkdPerformanceCounter.CategoryName, "Монитор производительности СКД сервера", PerformanceCounterCategoryType.MultiInstance,
                        SkdPerformanceCounter.CommunicationQuality, "Показывает % команд, на которые отвечает контроллер");
            }
            catch (Exception ex)
            {
                Log.Error("Ошибка при создании категорий счетчиков производительности", ex);
            }*/
        }

        public static string FunctionToString(MethodBase method, Object[] param)
        {
            string result = (method.DeclaringType != null ? method.DeclaringType.ToString() : "Undefined type") + "." + method.Name + " with ";
            string result1 = result;
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                
                result1 = result1 +
                          (param.Length >= parameter.Position
                          ? String.Format("{0}:{1}, ", parameter.Name, param[parameter.Position] ?? "")
                               : "");
            }
            return result1;
        }

        public static string ClassToString(Object obj)
        {
            Type type = obj.GetType();
            string result = "";
            /*foreach (FieldInfo field in type.GetFields())
            {
                result = result + String.Format("{0}:{1} ", field.Name, field.GetValue(obj));
            }*/
            foreach (var member in type.GetMembers())
            {
                if(member.MemberType == MemberTypes.Field)
                    result = result + String.Format("{0}:{1}; ", member.Name, ((FieldInfo)member).GetValue(obj));

                else if (member.MemberType == MemberTypes.Property)
                    result = result + String.Format("{0}:{1}; ", member.Name, ((PropertyInfo)member).GetValue(obj, null));
            }
            return type.Name + "{" + result + "}";
        }

        public static byte[] StringHexToByteArray(string hexValues, char separator)
		{
            string[] hexValuesSplit = hexValues.Split(separator);
            return hexValuesSplit.Select(hex => byte.Parse(hex, System.Globalization.NumberStyles.HexNumber)).ToArray();
		}

        public static byte[] StringHexToByteArray(string hexValues)
        {
            var bytes = new List<byte>();
            for (int i = 0; i < hexValues.Length; i += 2)
            {
                bytes.Add(byte.Parse(hexValues.Substring(i, 2), System.Globalization.NumberStyles.HexNumber));
            }
            return bytes.ToArray();
        }

        public static string ByteArrayToStringHex(byte[] vals, char? separator)
        {
            string str = BitConverter.ToString(vals);
            if(separator != null)
                str = BitConverter.ToString(vals).Replace("-", separator.ToString());
            return str;
        }

        public static byte ComputeChecksum(byte[] bytes)
        {
            byte result = 0;
            foreach (byte b in bytes)
                result = (byte) (result ^ b);
            return result;
        }


        public static double Degrees2Digit(double degrees)
        {
            double degree = Math.Floor(degrees);
            double minute = (degrees - degree) * 100;
            minute /= 60.0;
            return degree + minute;
        }

        /// <summary>
        /// Перевод двоично-десятичного представление (BCD) в число
        /// </summary>
        /// <param name="bcd"></param>
        /// <returns></returns>
        public static long BcdToDec(byte[] bcd)
        {
            long dec = 0;
            int c = 1;
            bcd = bcd.Reverse().ToArray();
            foreach (var currentByte in bcd)
            {
                var high = (byte)(currentByte >> 4);
                var low = (byte)(currentByte & 0xF);
                int number = 10 * high + low;
                dec = dec + number * c;
                c*=100;
            }
            return dec;
        }

        /// <summary>
        /// Перевод двоично-десятичного представление (BCD) в число
        /// </summary>
        /// <param name="bcd"></param>
        /// <returns></returns>
        public static long BcdToDec(byte bcd)
        {
            return BcdToDec(new[] {bcd});
        }

        /// <summary>
        /// Перевод числа в двоично-десятичное представление (BCD)
        /// </summary>
        /// <param name="dec"></param>
        /// <returns></returns>
        public static byte[] DecToBcd(long dec)
        {
            var bcd = new List<byte>();
            byte packed = 0;
            int c = 0;

            if (dec == 0)
                return new[] {(byte) 0};

            while( dec != 0)
            {
                var bcdPart = (byte) (dec - ((dec/10)*10));
                if (c % 2 == 0)
                    packed = bcdPart;
                else
                {
                    packed = (byte)(packed | bcdPart << 4);
                    bcd.Add(packed);
                }
                if (c % 2 == 0 && dec / 10 == 0)
                {
                    bcd.Add(bcdPart);
                }

                dec /= 10;
                c++;
            }
            bcd.Reverse();
            return bcd.ToArray();
        }
    }
}
