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
    public class Accept : IProcessRequest
    {
        public async Task Execute(RequestTaskState state)
        {
            var accept = await state.Connection.GetAsync(
                new EventAccept
                {
                    EventId = state.Request.EventId
                }, x => x.AttachToTransaction(state.Transaction));
            var details = (await state.Connection.QueryAsync<EventAcceptDetail>(
                @"SELECT * FROM dbo.EventAcceptDetails WHERE EventId = @EventId",
                new
                {
                    state.Request.EventId
                }, state.Transaction)).ToList();
            state.XmlDocument = Documents.Create(
                new XElement
                (
                    "accept",
                    new XAttribute("action_id", ActionId.Accept),
                    new XElement("subject_id", accept.SubjectId),
                    new XElement("counterparty_id", accept.CounterpartyId),
                    new XElement("operation_date", Format.ToDateTimeOffset(accept.OperationDT ?? DateTime.Now)),
                    new XElement("order_details",
                        details
                            .OrderBy(x=>x.DetailNo)
                            .Select(x=>new XElement(
                                x.Sgtin!=null && x.Sgtin.Length>0 ? "sgtin" : "sscc",
                                x.Sgtin != null && x.Sgtin.Length > 0 ? x.Sgtin : x.Sscc
                                ))
                            )
                ));
        }
    }
}
