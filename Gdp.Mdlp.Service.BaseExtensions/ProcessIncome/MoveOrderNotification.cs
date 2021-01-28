using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gdp.Mdlp.Data;
using Dapper;
using Dapper.FastCrud;
using System.Globalization;
using Gdp.Mdlp.Service.Extensibility;

namespace Gdp.Mdlp.Service.BaseExtensions.ProcessIncome
{
    public class MoveOrderNotification : IProcessIncome
    {
        public async Task<bool> Execute(IncomeTaskState state)
        {
            var xDocuments = state.XmlDocument.Element("documents");
            var xMsg = xDocuments.Elements("move_order_notification").FirstOrDefault();
            var xOrderDetails = xMsg.Elements("order_details").FirstOrDefault();
            var order = new IncomeMoveOrder
            {
                IncomeId = state.Income.IncomeId,
                ContractNo = xMsg.Element("contract_num")?.Value,
                ContractType = Convert.ToByte(xMsg.Element("contract_type")?.Value ?? "1"),
                DocDT = Format.ParseDate(xMsg.Element("doc_date")?.Value),
                DocNo = xMsg.Element("doc_num")?.Value,
                OperationDT = Format.ParseDateTime(xMsg.Element("operation_date")?.Value),
                ReceiverId = xMsg.Element("receiver_id")?.Value,
                SubjectId = xMsg.Element("subject_id")?.Value,
                SourceType = Convert.ToByte(xMsg.Element("source")?.Value ?? "1"),
                TurnoverType = Convert.ToByte(xMsg.Element("turnover_type")?.Value ?? "1")
            };
            var details = new List<IncomeMoveOrderDetail>();
            var ssccDetails = new List<IncomeMoveOrderSsccDetail>();
            var detailNo = 0;
            foreach (var xUnion in xOrderDetails?.Elements("union"))
            {
                var xSsccDetail = xUnion.Element("sscc_detail");
                var sscc = xSsccDetail?.Element("sscc")?.Value;
                var detail = new IncomeMoveOrderDetail
                {
                    Cost = Format.ParseDecimal(xUnion.Element("cost")?.Value),
                    VatValue = Format.ParseDecimal(xUnion.Element("vat_value")?.Value),
                    Sgtin = xUnion.Element("sgtin")?.Value,
                    Sscc = xSsccDetail?.Element("sscc")?.Value,
                    DetailNo = detailNo++,
                    IncomeId = state.Income.IncomeId
                };
                details.Add(detail);
                if (xSsccDetail != null && detail.Sscc!=null)
                {
                    var detailNo2 = 0;
                    foreach(var xDetail in xSsccDetail?.Elements("detail"))
                    {
                        var ssccDetail = new IncomeMoveOrderSsccDetail
                        {
                            IncomeId = state.Income.IncomeId,
                            DetailNo = detail.DetailNo,
                            Sscc = detail.Sscc,
                            DetailNo2 = detailNo2++,
                            Cost = Format.ParseDecimal(xDetail.Element("cost")?.Value),
                            VatValue = Format.ParseDecimal(xDetail.Element("vat_value")?.Value),
                            Gtin = xDetail.Element("gtin")?.Value,
                            SeriesNumber = xDetail.Element("series_number")?.Value
                        };
                        ssccDetails.Add(ssccDetail);
                    }
                }
            }
            await state.Connection.InsertAsync(order, (x) => x.AttachToTransaction(state.Transaction));
            foreach(var detail in details)
                await state.Connection.InsertAsync(detail, (x) => x.AttachToTransaction(state.Transaction));
            foreach (var ssccDetail in ssccDetails)
                await state.Connection.InsertAsync(ssccDetail, (x) => x.AttachToTransaction(state.Transaction));

            return true;
        }
    }
}
