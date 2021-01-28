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
using Gdp.Mdlp.Service.Extensibility;

namespace Gdp.Mdlp.Service.BaseExtensions.ProcessIncome
{
    public class RefusalSenderNotification : IProcessIncome
    {
        public async Task<bool> Execute(IncomeTaskState state)
        {
            bool processed = false;
            var xDocuments = state.XmlDocument.Element("documents");
            var xOrder = xDocuments.Elements("refusal_sender_notification").FirstOrDefault();

            if (xOrder != null)
            {
                var xOrderDetails = xOrder.Elements("order_details").FirstOrDefault();
                var order = new IncomeRefusalSenderNotification
                {
                    IncomeId = state.Income.IncomeId,
                    SubjectId = xOrder.Element("subject_id")?.Value,
                    ReceiverId = xOrder.Element("receiver_id")?.Value,
                    Reason = xOrder.Element("reason")?.Value,
                    OperationDT = Format.ParseDateTime(xOrder.Element("operation_date")?.Value)
                };
                var detailNo = 0;
                var details = xOrderDetails
                    .Elements()
                    .Where(x => x.Name == "sgtin" || x.Name == "sscc")
                    .Select(x => new IncomeRefusalSenderNotificationDetail
                    {
                        IncomeId = state.Income.IncomeId,
                        DetailNo = detailNo++,
                        Sgtin = x.Name == "sgtin" ? x.Value : null,
                        Sscc = x.Name == "sscc" ? x.Value : null
                    })
                    .ToList();
                if (details.Count > 0)
                {
                    await state.Connection.InsertAsync(order, (x) => x.AttachToTransaction(state.Transaction));
                    foreach (var detail in details)
                        await state.Connection.InsertAsync(detail, (x) => x.AttachToTransaction(state.Transaction));
                    processed = true;
                }
            }
            return processed;
        }
    }
}
