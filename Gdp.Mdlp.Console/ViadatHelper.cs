using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Console
{
    static class ViadatHelper
    {
        const string DATETIME_FORMAT = "yyyy'-'MM'-'dd HH':'mm':'ss";
        public static string Now
        {
            get
            {
                return DateTimeToString(DateTime.Now);
            }
        }
        public static string DateTimeToString(DateTime? dt)
        {
            if (dt == null)
                return null;
            if (!dt.HasValue)
                return null;
            else
                return dt.Value.ToString(DATETIME_FORMAT);
        }
        public static DateTime? StringToDateTime(string dt)
        {
            return (dt == null || dt == string.Empty) ?
                (DateTime?)null :
                DateTime.ParseExact(dt, DATETIME_FORMAT, CultureInfo.InvariantCulture);
        }
    }
}
