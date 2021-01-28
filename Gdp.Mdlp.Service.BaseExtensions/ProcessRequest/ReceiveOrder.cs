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
    public class ReceiveOrder : IProcessRequest
    {
        public async Task Execute(RequestTaskState state)
        {
            var receip = await state.Connection.GetAsync(
                new EventReceiping
                {
                    EventId = state.Request.EventId
                }, x => x.AttachToTransaction(state.Transaction));
            var details = (await state.Connection.QueryAsync<EventReceipingDetail>(
                @"SELECT * FROM dbo.EventReceipingDetails WHERE EventId = @EventId",
                new
                {
                    state.Request.EventId
                }, state.Transaction)).ToList();
            state.XmlDocument = Documents.Create(
                new XElement(
                    "receive_order",
                    new XAttribute("action_id", ActionId.ReceiveOrder),
                    new XElement("subject_id", receip.SubjectId),
                    new XElement("shipper_id", receip.ShipperId),
                    new XElement("operation_date", Format.ToDateTimeOffset(receip.OperationDT ?? DateTime.Now)),
                    new XElement("doc_num", receip.DocNo),
                    new XElement("doc_date", Format.ToDate(receip.DocDT)),
                    new XElement("receive_type", receip.ReceiveType ?? 1),
                    new XElement("source", receip.SourceType ?? 1),
                    new XElement("contract_type", receip.ContractType ?? 1),
                    receip.ContractNo != null && receip.ContractNo !=string.Empty ? new XElement("contract_num", receip.ContractNo) : null,
                    new XElement("order_details",
                        details
                            .OrderBy(x => x.DetailNo)
                            .Where(x => x.Sscc != null || x.Sgtin != null)
                            .Select(x => new XElement(
                                "union",
                                x.Sgtin != null ?
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
