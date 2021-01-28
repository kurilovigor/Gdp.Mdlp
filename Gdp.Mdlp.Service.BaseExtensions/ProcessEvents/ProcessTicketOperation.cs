using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdp.Mdlp.Data;
using Dapper;
using Dapper.FastCrud;
using Gdp.Mdlp.Service.Extensibility;

namespace Gdp.Mdlp.Service.BaseExtensions.ProcessEvents
{
    public class ProcessTicketOperation : IProcessEvent
    {
        class AcceptedDetail
        {
            public string SubjectId { get; set; }
            public string CounterpartyId { get; set; }
            public string Sgtin { get; set; }
            public string Sscc { get; set; }
        }
        public async Task<bool> Execute(EventTaskState state)
        {
            bool processed = false;
            var evtTicketOp = await state.Connection.GetAsync(
                new EventTicketOperation
                {
                    EventId = state.Event.EventId
                }, 
                (x)=>x.AttachToTransaction(state.Transaction));
            if (evtTicketOp.Operation == "912")
            {
                var ticketOp = await GetTicketOperationAsync(state.Connection, state.Transaction, evtTicketOp);
                if (string.Compare(ticketOp.OperationResult, "Accepted", true) == 0)
                {
                    var eventUnpack = await state.Connection.GetAsync(new EventUnpack { EventId = ticketOp.EventId }, (e) => e.AttachToTransaction(state.Transaction));
                    await state.Connection.RegisterSsccAsync(
                        new SSCC
                        {
                            Sscc = eventUnpack.Sscc,
                            State = 1,
                            SubjectId = eventUnpack.SubjectId
                        }, state.Transaction);
                    processed = true;
                }
            }
            else if (evtTicketOp.Operation == "701")
            {
                var ticketOp = await GetTicketOperationAsync(state.Connection, state.Transaction, evtTicketOp);
                if (
                    string.Compare(ticketOp.OperationResult, "Accepted", true) == 0 || 
                    string.Compare(ticketOp.OperationResult, "Partial", true) == 0)
                {
                    var evt = await state.Connection.GetAsync<EventAccept>(new EventAccept { EventId=ticketOp.EventId }, x=>x.AttachToTransaction(state.Transaction));
                    if (evt?.AcceptedActionId == "602")
                    {
                        var acceptedDetails = await state.Connection.QueryAsync<AcceptedDetail>(
                            @"SELECT 
	ea.SubjectId, ea.CounterpartyId, ead.Sgtin, ead.Sscc
FROM dbo.EventAccepts ea WITH(NOLOCK) 
	JOIN dbo.EventAcceptDetails ead WITH(NOLOCK) ON ead.EventId = ea.EventId
WHERE 
	ea.EventId=@EventId
	AND COALESCE(ead.Sgtin, ead.Sscc,'') NOT IN (
		SELECT toe.[object_id] 
		FROM dbo.TicketOperationErrors toe WITH(NOLOCK) 
		WHERE toe.RequestId = @RequestId AND toe.EventId=@EventId)",
                            new
                            {
                                ticketOp.EventId,
                                ticketOp.RequestId
                            }, state.Transaction);
                        foreach (var detail in acceptedDetails)
                        {
                            if (detail.Sscc != null && detail.Sscc.Length > 0)
                                await state.Connection.UpdateSsccSubjectIdAsync(detail.Sscc, detail.CounterpartyId, state.Transaction);
                            else if (detail.Sgtin != null && detail.Sgtin.Length > 0)
                                await state.Connection.UpdateSgtinSubjectIdAsync(detail.Sgtin, detail.CounterpartyId, state.Transaction);
                        }
                    }
                }
            }
            return processed;
        }
        async Task<TicketOperation> GetTicketOperationAsync(SqlConnection connection, SqlTransaction transaction, EventTicketOperation eventTicketOperation)
        {
            return await connection.GetAsync(
                new TicketOperation
                {
                    RequestId = eventTicketOperation.ParentRequestId,
                    EventId = eventTicketOperation.ParentEventId
                },
                (x) => x.AttachToTransaction(transaction));
        }
    }
}
