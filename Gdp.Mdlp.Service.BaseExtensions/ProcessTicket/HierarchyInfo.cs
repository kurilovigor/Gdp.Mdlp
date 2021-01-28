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

namespace Gdp.Mdlp.Service.BaseExtensions.ProcessTicket
{
    public class HierarchyInfo : IProcessTicket
    {
        public async Task<bool> Execute(TicketTaskState state)
        {
            var xDocuments = state.XmlDocument.Element("documents");
            var xInfo = xDocuments.Element("hierarchy_info");
            var xSsccUp = xInfo.Element("sscc_up");
            var xSsccDown = xInfo.Element("sscc_down");
            var xOperationWarnings = xInfo.Element("operation_warnings");

            var ticket = new Ticket
            {
                DocumentId = state.Request.DocumentId,
                RequestId = state.Request.RequestId,
                EventId = state.Request.EventId,
                ActionId = xInfo.Attribute("action_id").Value
            };
            await state.Connection.InsertAsync(ticket, x => x.AttachToTransaction(state.Transaction));

            if (xSsccUp != null)
                await ProcessSsccUpInfo(state.Connection, state.Transaction, state.Request, xSsccUp, 0,  null);

            if (xSsccDown != null)
                await ProcessSsccDownInfo(state.Connection, state.Transaction, state.Request, xSsccDown, 0, null);

            if (xOperationWarnings != null)
            {
                int operationWarningNo = 0;
                foreach (var xOperationWarning in xOperationWarnings.Elements("operation_warning"))
                {
                    var ticketOperationWarning = new TicketHierarchyInfoOpWarning
                    {
                        RequestId = state.Request.RequestId,
                        EventId = state.Request.EventId,
                        OperationWarningNo = operationWarningNo++,
                        OperationWarning = xOperationWarning.Value
                    };
                    await state.Connection.InsertAsync(ticketOperationWarning, x => x.AttachToTransaction(state.Transaction));
                }
            }
            return true;
        }

        async Task<int> ProcessSsccUpInfo
            (
                SqlConnection connection, 
                SqlTransaction transaction, 
                Request request, 
                XElement parent, 
                int nodeNo,
                string parentSscc
            )
        {
            var n = nodeNo;
            foreach (var xSsccInfo in parent.Elements("sscc_info"))
            {
                var ticketHierarchyInfoSsccUp = new TicketHierarchyInfoSsccUp
                {
                    RequestId = request.RequestId,
                    EventId = request.EventId,
                    NodeNo = n++,
                    PackingDate = Format.ParseDateTime(xSsccInfo.Element("packing_date")?.Value),
                    Sscc = xSsccInfo.Element("sscc")?.Value,
                    ParentSscc = parentSscc
                };
                await connection.InsertAsync(ticketHierarchyInfoSsccUp, x => x.AttachToTransaction(transaction));
                var xChilds = xSsccInfo.Element("childs");
                if(xChilds!=null)
                    n = await ProcessSsccUpInfo(connection, transaction, request, xChilds, n, ticketHierarchyInfoSsccUp.Sscc);
            }
            return n;
        }
        async Task<int> ProcessSsccDownInfo
            (
                SqlConnection connection,
                SqlTransaction transaction,
                Request request,
                XElement parent,
                int nodeNo,
                string parentSscc
            )
        {
            var n = nodeNo;
            foreach (var xSsccInfo in parent.Elements("sscc_info"))
            {
                var ticketHierarchyInfoSsccDown = new TicketHierarchyInfoSsccDown
                {
                    RequestId = request.RequestId,
                    EventId = request.EventId,
                    NodeNo = n++,
                    PackingDate = Format.ParseDateTime(xSsccInfo.Element("packing_date")?.Value),
                    Sscc = xSsccInfo.Element("sscc")?.Value,
                    ParentSscc = parentSscc
                };
                await connection.InsertAsync(ticketHierarchyInfoSsccDown, x => x.AttachToTransaction(transaction));
                var xChilds = xSsccInfo.Element("childs");
                if (xChilds != null)
                    n = await ProcessSsccDownInfo(connection, transaction, request, xChilds, n, ticketHierarchyInfoSsccDown.Sscc);
            }
            foreach (var xSgtinInfo in parent.Elements("sgtin_info"))
            {
                var ticketHierarchyInfoSgtinDown = new TicketHierarchyInfoSgtinDown
                {
                    RequestId = request.RequestId,
                    EventId = request.EventId,
                    NodeNo = n++,
                    Status = xSgtinInfo?.Element("status")?.Value,
                    Gtin = xSgtinInfo?.Element("gtin")?.Value,
                    Sgtin = xSgtinInfo?.Element("sgtin")?.Value,
                    SeriesNumber = xSgtinInfo?.Element("series_number")?.Value,
                    ExpirationDate = Format.ParseDate(xSgtinInfo?.Element("expiration_date")?.Value),
                    Sscc = xSgtinInfo?.Element("sscc")?.Value,
                };
                await connection.InsertAsync(ticketHierarchyInfoSgtinDown, x => x.AttachToTransaction(transaction));
            }
            return n;
        }
    }
}

