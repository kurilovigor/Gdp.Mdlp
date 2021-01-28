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
    public class Posting : IProcessRequest
    {
        public async Task Execute(RequestTaskState state)
        {
            var posting = await state.Connection.GetAsync(
                new EventPosting
                {
                    EventId = state.Request.EventId
                }, x => x.AttachToTransaction(state.Transaction));
            var details = (await state.Connection.QueryAsync<EventPostingDetail>(
                @"SELECT * FROM dbo.EventPostingDetails WHERE EventId = @EventId",
                new
                {
                    state.Request.EventId
                }, state.Transaction)).ToList();
            state.XmlDocument = Documents.Create(
                new XElement(
                    "posting",
                    new XAttribute("action_id", ActionId.Posting),
                    new XElement("subject_id", posting.SubjectId),
                    new XElement("shipper_info",
                        new XElement("inn", posting.ShipperInn),
                        new XElement("kpp", posting.ShipperKpp)
                    ),
                    new XElement("operation_date", Format.ToDateTimeOffset(posting.OperationDT ?? DateTime.Now)),
                    new XElement("doc_num", posting.DocNo),
                    new XElement("doc_date", Format.ToDate(posting.DocDT)),
                    new XElement("contract_type", posting.ContractType ?? 1),
                    new XElement("source", posting.SourceType ?? 1),
                    posting.ContractNo != null && posting.ContractNo != string.Empty ? new XElement("contract_num", posting.ContractNo) : null,
                    new XElement("order_details",
                        details
                            .OrderBy(x => x.DetailNo)
                            .Where(x => x.Sgtin != null)
                            .Select(x => new XElement(
                                "union",
                                new XElement("sgtin", x.Sgtin),
                                new XElement("cost", x.Cost),
                                new XElement("vat_value", x.VatValue)
                                ))
                        )
                    )
                );
        }
    }
}
