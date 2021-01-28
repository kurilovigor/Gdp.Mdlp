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

namespace Gdp.Mdlp.Service.BaseExtensions.ProcessRequest
{
    public class RefusalSender : IProcessRequest
    {
        public async Task Execute(RequestTaskState state)
        {
            var order = await state.Connection.GetAsync(
                new EventRefusalSender
                {
                    EventId = state.Request.EventId
                }, x => x.AttachToTransaction(state.Transaction));
            var details = (await state.Connection.QueryAsync<EventRefusalSenderDetail>(
                @"SELECT * FROM dbo.EventRefusalSenderDetails WHERE EventId=@EventId",
                new 
                {
                    state.Request.EventId
                }, state.Transaction)).ToList();
            state.XmlDocument = Documents.Create(
                new XElement(
                    "refusal_sender",
                    new XAttribute("action_id", ActionId.RefusalSender),
                    new XElement("subject_id", order.SubjectId),
                    new XElement("operation_date", Format.ToDateTimeOffset(order.OperationDT ?? DateTime.Now)),
                    new XElement("receiver_id", order.ReceiverId),
                    new XElement("reason", order.Reason),
                    new XElement("order_details",
                        details
                            .OrderBy(x => x.DetailNo)
                            .Where(x => x.Sscc != null || x.Sgtin != null)
                            .Select
                            (   x =>
                                x.Sgtin != null ?
                                    new XElement("sgtin", x.Sgtin) :
                                    new XElement("sscc", x.Sscc)
                            )
                        )
                    )
                );
        }
    }
}
