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
    class Operation : IProcessTicket
    {
        public async Task<bool> Execute(TicketTaskState state)
        {
            var xDocuments = state.XmlDocument.Element("documents");
            var xResult = xDocuments.Element("result");
            var acceptTime = Format.ParseDateTime(xResult.Attributes("accept_time").FirstOrDefault()?.Value);
            var operation = xResult.Elements("operation").FirstOrDefault()?.Value;
            var operationResult = xResult.Elements("operation_result").FirstOrDefault()?.Value;
            var ticket = new Ticket
            {
                DocumentId = state.Request.DocumentId,
                RequestId = state.Request.RequestId,
                EventId = state.Request.EventId,
                ActionId = xResult.Attribute("action_id").Value
            };
            await state.Connection.InsertAsync(ticket, (e) => { e.AttachToTransaction(state.Transaction); });
            var ticketOp = new TicketOperation
            {
                DocumentId = state.Request.DocumentId,
                RequestId = state.Request.RequestId,
                EventId = state.Request.EventId,
                AcceptTime = acceptTime,
                Operation = operation,
                OperationId = xResult.Elements("operation_id").FirstOrDefault()?.Value,
                OperationComment = xResult.Elements("operation_comment").FirstOrDefault()?.Value,
                OperationResult = operationResult
            };
            await state.Connection.InsertAsync(ticketOp, (e) => { e.AttachToTransaction(state.Transaction); });
            int errorNo = 0;
            foreach (var xError in xResult.Elements("errors"))
            {
                var error_code = xError.Elements("error_code").FirstOrDefault()?.Value;
                var error_desc = xError.Elements("error_desc").FirstOrDefault()?.Value;
                var object_id = xError.Elements("object_id").FirstOrDefault()?.Value;
                var ticketOpError = new TicketOperationError
                {
                    RequestId = state.Request.RequestId,
                    EventId = state.Request.EventId,
                    ErrorNo = errorNo,
                    error_code = error_code != null ? (int?)Convert.ToInt32(error_code) : null,
                    error_desc = error_desc,
                    object_id = object_id
                };
                await state.Connection.InsertAsync(ticketOpError, (e) => { e.AttachToTransaction(state.Transaction); });
                errorNo++;
            }
            // EventTicketOperation
            var evt = await state.Connection.CreateEventAsync(EventTypeEnum.TicketOperation, state.Transaction);
            var evtTicketOp = new EventTicketOperation
            {
                EventId = evt.EventId,
                ParentEventId = state.Request.EventId,
                ParentRequestId = state.Request.RequestId,
                Operation = operation
            };
            await state.Connection.InsertAsync(evtTicketOp, (x) => x.AttachToTransaction(state.Transaction));
            evt.SetEventState(EventStateEnum.Pending);
            await state.Connection.UpdateAsync(evt, (x) => x.AttachToTransaction(state.Transaction));

            return true;
        }
    }
}
