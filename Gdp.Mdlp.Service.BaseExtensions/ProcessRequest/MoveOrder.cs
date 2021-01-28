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
    public class MoveOrder : IProcessRequest
    {
        public async Task Execute(RequestTaskState state)
        {
            var ship = await state.Connection.GetAsync(
                new EventShipping
                {
                    EventId = state.Request.EventId
                }, x => x.AttachToTransaction(state.Transaction));
            var details = (await state.Connection.QueryAsync<EventShippingDetail>(
                @"SELECT * FROM dbo.EventShippingDetails WHERE EventId = @EventId",
                new
                {
                    state.Request.EventId
                }, state.Transaction)).ToList();
            state.XmlDocument = Documents.Create(
                new XElement(
                    "move_order",
                    new XAttribute("action_id", ActionId.MoveOrder),
                    new XElement("subject_id", ship.SubjectId),
                    new XElement("receiver_id", ship.ReceiverId),
                    new XElement("operation_date", Format.ToDateTimeOffset(ship.OperationDT ?? DateTime.Now)),
                    new XElement("doc_num", ship.DocNo),
                    new XElement("doc_date", Format.ToDate(ship.DocDT)),
                    new XElement("turnover_type", ship.TurnoverType ?? 1),
                    new XElement("source", ship.SourceType ?? 1),
                    new XElement("contract_type", ship.ContractType ?? 1),
                    ship.ContractNo!=null && ship.ContractNo != string.Empty ? new XElement("contract_num", ship.ContractNo) : null,
                    new XElement("order_details",
                        details
                            .OrderBy(x=>x.DetailNo)
                            .Where(x=>x.Sscc != null || x.Sgtin != null)
                            .Select(x=>new XElement(
                                "union",
                                x.Sgtin !=null ? 
                                    new XElement("sgtin", x.Sgtin) : 
                                    new XElement(
                                        "sscc_detail", 
                                        new XElement("sscc", x.Sscc)
                                    ),
                                new XElement("cost", x.Cost),
                                new XElement("vat_value", x.VatValue)
                                ))
                        )
                    )
                );
        }
    }
}
