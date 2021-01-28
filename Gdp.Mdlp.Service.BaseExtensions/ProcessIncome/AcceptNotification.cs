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
    public class AcceptNotification : IProcessIncome
    {
        public async Task<bool> Execute(IncomeTaskState state)
        {
            bool processed = false;
            var xDocuments = state.XmlDocument.Element("documents");
            var xAcceptNotification = xDocuments.Elements("accept_notification").FirstOrDefault();
            var xOrderDetails = xAcceptNotification.Elements("order_details").FirstOrDefault();
            if (xAcceptNotification != null && xOrderDetails!=null)
            {
                // accept notification
                var eventAccept = new IncomeAcceptNotification
                {
                    IncomeId = state.Income.IncomeId,
                    SubjectId = xAcceptNotification.Elements("subject_id").FirstOrDefault()?.Value,
                    CounterpartyId = xAcceptNotification.Elements("counterparty_id").FirstOrDefault()?.Value,
                    OperationDT = Format.ParseDateTime(xAcceptNotification.Elements("operation_date").FirstOrDefault()?.Value)
                };
                // collect sscc/sgtins
                int i = 0;
                var eventAcceptDetails = xOrderDetails.Elements()
                    .Where(x=>x.Name == "sgtin" || x.Name == "sscc")
                    .Select(
                    x => new IncomeAcceptNotificationDetail
                    {
                        IncomeId = state.Income.IncomeId,
                        RowNo = i++,
                        Sgtin = x.Name=="sgtin" ? x.Value : null,
                        Sscc = x.Name == "sscc" ? x.Value : null
                    }).ToList();
                if (eventAcceptDetails.Count > 0)
                {
                    await state.Connection.InsertAsync(eventAccept, (x) => x.AttachToTransaction(state.Transaction));
                    foreach (var e in eventAcceptDetails)
                        await state.Connection.InsertAsync(e, (x) => x.AttachToTransaction(state.Transaction));
                    foreach (var detail in eventAcceptDetails)
                    {
                        if (detail.Sscc != null && detail.Sscc.Length > 0)
                            await state.Connection.UpdateSsccSubjectIdAsync(detail.Sscc, eventAccept.SubjectId, state.Transaction);
                        else if (detail.Sgtin != null && detail.Sgtin.Length > 0)
                            await state.Connection.UpdateSgtinSubjectIdAsync(detail.Sgtin, eventAccept.SubjectId, state.Transaction);
                    }
                    processed = true;
                }
            }
            return processed;
        }
    }
}
