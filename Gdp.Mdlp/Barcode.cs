using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Gdp.Mdlp
{
    public static class Barcode
    {
        static string engMap = @"`1234567890-=\qwertyuiop[]asdfghjkl;'zxcvbnm,./";
        static string ruMap  = @"`1234567890-=\йцукенгшщзхъфывапролджэячсмитьбю.";
        static Regex sgtinBarcodeRegex = new Regex(@"^01(?<p1>[0-9]{14})21(?<p2>[_!-&%\-\/0-9A-Za-z]{13})");
        static Regex ssccBarcodeRegex = new Regex(@"^00(?<p1>[0-9]{18})");
        static Regex gtinBarcodeRegex = new Regex(@"^0[01](?<p1>[0-9]{14})");
        public static string ToSgtin(string code)
        {
            var m = sgtinBarcodeRegex.Match(code);
            if(!m.Success)
                throw new InvalidCastException($"Can not cast \"{code}\" to SGTIN");
            return m.Groups["p1"].Value + m.Groups["p2"].Value;
        }
        public static string ToGtin(string code)
        {
            var m = gtinBarcodeRegex.Match(code);
            if (!m.Success)
                throw new InvalidCastException($"Can not cast \"{code}\" to GTIN");
            return m.Groups["p1"].Value;
        }
        public static string ToSscc(string code)
        {
            var m = ssccBarcodeRegex.Match(code);
            if (!m.Success)
                throw new InvalidCastException($"Can not cast \"{code}\" to SSCC");
            return m.Groups["p1"].Value;
        }
        public static string ToGtinSmart(string code)
        {
            var m = gtinBarcodeRegex.Match(code);
            if (m.Success)
                return m.Groups["p1"].Value;
            m = gtinBarcodeRegex.Match(RuToEng(code));
            if (m.Success)
                return m.Groups["p1"].Value;
            throw new InvalidCastException($"Can not cast \"{code}\" to GTIN");
        }
        public static string TryGtinSmart(string code)
        {
            var m = gtinBarcodeRegex.Match(code);
            if (m.Success)
                return m.Groups["p1"].Value;
            m = gtinBarcodeRegex.Match(RuToEng(code));
            if (m.Success)
                return m.Groups["p1"].Value;
            return null;
        }
        public static string ToSsccSmart(string code)
        {
            var m = ssccBarcodeRegex.Match(code);
            if (m.Success)
                return m.Groups["p1"].Value;
            m = ssccBarcodeRegex.Match(RuToEng(code));
            if (m.Success)
                return m.Groups["p1"].Value;
            throw new InvalidCastException($"Can not cast \"{code}\" to SSCC");
        }
        public static string ToSgtinSmart(string code)
        {
            var m = sgtinBarcodeRegex.Match(code);
            if (m.Success)
                return m.Groups["p1"].Value + m.Groups["p2"].Value;
            m = sgtinBarcodeRegex.Match(RuToEng(code));
            if (m.Success)
                return m.Groups["p1"].Value + m.Groups["p2"].Value;
            throw new InvalidCastException($"Can not cast \"{code}\" to SGTIN");
        }
        public static string TrySgtinSmart(string code)
        {
            var m = sgtinBarcodeRegex.Match(code);
            if (m.Success)
                return m.Groups["p1"].Value + m.Groups["p2"].Value;
            m = sgtinBarcodeRegex.Match(RuToEng(code));
            if (m.Success)
                return m.Groups["p1"].Value + m.Groups["p2"].Value;
            return null;
        }
        public static string TrySsccSmart(string code)
        {
            var m = ssccBarcodeRegex.Match(code);
            if (m.Success)
                return m.Groups["p1"].Value;
            m = ssccBarcodeRegex.Match(RuToEng(code));
            if (m.Success)
                return m.Groups["p1"].Value;
            return null;
        }

        private static string RuToEng(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in text)
            {
                var c2 = char.ToLower(c);
                var i = ruMap.IndexOf(c2);
                if (i >= 0)
                {
                    if(c == c2)
                        sb.Append(engMap[i]);
                    else
                        sb.Append(char.ToUpper(engMap[i]));
                }
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
