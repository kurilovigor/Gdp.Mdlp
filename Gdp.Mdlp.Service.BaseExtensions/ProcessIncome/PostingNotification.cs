using Gdp.Mdlp.Service.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdp.Mdlp.Data;
using Dapper;
using Dapper.FastCrud;


namespace Gdp.Mdlp.Service.BaseExtensions.ProcessIncome
{
    public class PostingNotification : IProcessIncome
    {
        public async Task<bool> Execute(IncomeTaskState state)
        {
            bool processed = false;
            var xDocuments = state.XmlDocument.Element("documents");
            var xPostingNotification = xDocuments.Element("posting_notification");
            if (xPostingNotification != null)
            {
                var xDetails = xPostingNotification.Element("order_details");
                if (xDetails != null)
                {
                    var postingNotification = new IncomePostingNotification
                    {
                        IncomeId = state.Income.IncomeId,
                        SubjectId = xPostingNotification.Element("subject_id")?.Value,
                        Inn = xPostingNotification.Element("inn")?.Value,
                        OrgName = xPostingNotification.Element("organisation_name")?.Value,
                        OwnerId = xPostingNotification.Element("owner_id")?.Value,
                        OperationDT = Format.ParseDateTime(xPostingNotification.Element("operation_date")?.Value)
                    };
                    var postingNotificationDetails = new List<IncomePostingNotificationDetail>();
                    var detailNo = 0;
                    postingNotificationDetails.AddRange(xDetails
                        .Elements("sgtin")
                        .Select(x=>new IncomePostingNotificationDetail
                        {
                            IncomeId = state.Income.IncomeId,
                            DetailNo = detailNo++,
                            Sgtin = x.Value
                        }));
                    postingNotificationDetails.AddRange(xDetails
                        .Elements("sscc")
                        .Select(x => new IncomePostingNotificationDetail
                        {
                            IncomeId = state.Income.IncomeId,
                            DetailNo = detailNo++,
                            Sscc = x.Value
                        }));
                    await state.Connection.InsertAsync(postingNotification, x=>x.AttachToTransaction(state.Transaction));
                    foreach (var detail in postingNotificationDetails)
                        await state.Connection.InsertAsync(detail, x => x.AttachToTransaction(state.Transaction));
                    foreach (var detail in postingNotificationDetails)
                    {
                        if (detail.Sscc != null && detail.Sscc.Length > 0)
                            await state.Connection.UpdateSsccSubjectIdAsync(detail.Sscc, postingNotification.SubjectId, state.Transaction);
                        else if (detail.Sgtin != null && detail.Sgtin.Length > 0)
                            await state.Connection.UpdateSgtinSubjectIdAsync(detail.Sgtin, postingNotification.SubjectId, state.Transaction);
                    }
                    processed = true;
                }
            }
            return processed;
        }
    }
}
