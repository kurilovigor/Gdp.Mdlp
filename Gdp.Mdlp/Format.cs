using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp
{
    public static class Format
    {
        static NumberFormatInfo DefaultNumberFormatInfo = new NumberFormatInfo() { NumberDecimalSeparator = "." };
        public static string ToDate(DateTime dt)
        {
            return dt.ToString("dd.MM.yyyy");
        }
        /// <summary>
        /// Форматирует дату согласно ISO
        /// </summary>
        /// <param name="dt">дата</param>
        /// <returns></returns>
        public static string ToDateTimeOffset(DateTime dt)
        {
            return DateTime.SpecifyKind(dt, DateTimeKind.Local).ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
        }
        /// <summary>
        /// Возвращает время в формате MskDateTime
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string ToMskDateTime(DateTime dt)
        {
            return dt.ToString("s");
        }
        /// <summary>
        /// Возвращает строку для текущего времени
        /// </summary>
        /// <returns></returns>
        public static string NowDateTimeOffset()
        {
            return ToDateTimeOffset(DateTime.Now);
        }
        /// <summary>
        /// Возвращает строку для текущего времени в формате MskDateTime
        /// </summary>
        /// <returns></returns>
        public static string MskTimeStamp()
        {
            return DateTime.Now.ToString("s");
        }
        public static DateTime? ParseDate(string value)
        {
            if (value == null)
                return null;
            if (value == string.Empty)
                return null;
            return DateTime.ParseExact(value, "dd'.'MM'.'yyyy", CultureInfo.InvariantCulture);
        }
        public static DateTime? ParseDateTime(string value)
        {
            if (value == null)
                return null;
            if (value == string.Empty)
                return null;
            return Convert.ToDateTime(value);
            //return DateTime.ParseExact(value, "o", CultureInfo.InvariantCulture);
        }

        public static decimal? ParseDecimal(string value)
        {
            if (value == null)
                return null;
            if (value == string.Empty)
                return null;
            return decimal.Parse(value, DefaultNumberFormatInfo);
        }
    }
}
