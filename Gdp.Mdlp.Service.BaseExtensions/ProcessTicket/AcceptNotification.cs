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

namespace Gdp.Mdlp.Service.BaseExtensions.ProcessTicket
{
    public class AcceptNotification : IProcessTicket
    {
        public async Task<bool> Execute(TicketTaskState state)
        {
            bool processed = false;
            var xDocuments = state.XmlDocument.Element("documents");
            var xAcceptNotification = xDocuments.Elements("accept_notification").FirstOrDefault();
            var xOrderDetails = xAcceptNotification.Elements("order_details").FirstOrDefault();
            if (xAcceptNotification != null && xOrderDetails!=null)
            {
                // accept notification
                var eventAccept = new EventAcceptNotification
                {
                    SubjectId = xAcceptNotification.Elements("subject_id").FirstOrDefault()?.Value,
                    CounterpartyId = xAcceptNotification.Elements("counterparty_id").FirstOrDefault()?.Value,
                    OperationDT = Format.ParseDateTime(xAcceptNotification.Elements("operation_date").FirstOrDefault()?.Value)
                };
                // collect sscc
                int iSscc = 0;
                var eventAcceptSSCCs = xOrderDetails.Elements("sscc").Select(
                    x => new EventAcceptNotificationSSCC
                    {
                        RowNo = iSscc++,
                        Sscc = x.Value
                    }).ToList();
                // collect sgtin
                int iSgtin = 0;
                var eventAcceptSGTINs = xOrderDetails.Elements("sgtin").Select(
                    x => new EventAcceptNotificationSGTIN
                    {
                        RowNo = iSgtin++,
                        Sgtin = x.Value
                    }).ToList();
                if (eventAcceptSSCCs.Count > 0 || eventAcceptSGTINs.Count > 0)
                {
                    // root event
                    var evt = await state.Connection.CreateEventAsync(EventTypeEnum.AcceptNotification, state.Transaction);
                    eventAccept.EventId = evt.EventId;
                    // accept notification event
                    await state.Connection.InsertAsync(eventAccept, (x) => x.AttachToTransaction(state.Transaction));
                    foreach (var e in eventAcceptSSCCs)
                    {
                        e.EventId = evt.EventId;
                        await state.Connection.InsertAsync(e, (x) => x.AttachToTransaction(state.Transaction));
                    }
                    foreach (var e in eventAcceptSGTINs)
                    {
                        e.EventId = evt.EventId;
                        await state.Connection.InsertAsync(e, (x) => x.AttachToTransaction(state.Transaction));
                    }
                    // set event pending state
                    evt.SetEventState(EventStateEnum.Pending);
                    await state.Connection.UpdateAsync(evt, (x)=>x.AttachToTransaction(state.Transaction));
                    processed = true;
                }
            }
            return processed;
        }
    }
}
