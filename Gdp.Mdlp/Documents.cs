using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Gdp.Mdlp
{
    public static class Documents
    {
        /// <summary>
        /// Версия документов
        /// </summary>
        public static string Version { get; set; } = "1.35";
        /// <summary>
        /// Создает экземпляр документа
        /// </summary>
        /// <param name="content">Содержимое документа</param>
        /// <returns></returns>
        public static XDocument Create(params object[] content)
        {
            return Create(Version, content);
        }
        public static XDocument Create(string version, params object[] content)
        {
            XNamespace ns = "http://www.w3.org/2001/XMLSchema-instance";
            return new XDocument(
                new XElement("documents",
                new XAttribute(XNamespace.Xmlns + "xsi", ns),
                new XAttribute("version", version),
                content
                ));
        }

        /// <summary>
        /// Создает документ распаковки
        /// </summary>
        /// <param name="subject_id">ИД субъекта</param>
        /// <param name="sscc">Код SSCC упаковки</param>
        /// <returns></returns>
        public static XDocument Unpack(string subject_id, string sscc)
        {
            return Create(new XElement("unit_unpack",
                        new XAttribute("action_id", ActionId.Unpack),
                        new XElement("subject_id", subject_id),
                        new XElement("operation_date", Format.NowDateTimeOffset()),
                        new XElement("sscc", sscc)
                    ));
        }
        public static XDocument Unpack(string subject_id, string sscc, DateTime operationDate)
        {
            return Create(new XElement("unit_unpack",
                        new XAttribute("action_id", ActionId.Unpack),
                        new XElement("subject_id", subject_id),
                        new XElement("operation_date", Format.ToDateTimeOffset(operationDate)),
                        new XElement("sscc", sscc)
                    ));
        }
        public static XDocument QueryKizInfo(string subject_id, string sgtin)
        {
            return Create(new XElement("query_kiz_info",
                        new XAttribute("action_id", ActionId.QueryKizInfo),
                        new XElement("subject_id", subject_id),
                        new XElement("sgtin", sgtin)
                    ));
        }
        public static XDocument QueryKizInfoSsccDown(string subject_id, string ssccDown)
        {
            return Create(new XElement("query_kiz_info",
                        new XAttribute("action_id", ActionId.QueryKizInfo),
                        new XElement("subject_id", subject_id),
                        new XElement("sscc_down", ssccDown)
                    ));
        }
        public static XDocument QueryKizInfoSsccUp(string subject_id, string ssccUp)
        {
            return Create(new XElement("query_kiz_info",
                        new XAttribute("action_id", ActionId.QueryKizInfo),
                        new XElement("subject_id", subject_id),
                        new XElement("sscc_up", ssccUp)
                    ));
        }

    }
}
