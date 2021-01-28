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

namespace Gdp.Mdlp.Service.BaseExtensions.ProcessIncome
{
    public class ReceiveOrderErrorsNotification : IProcessIncome
    {
        public async Task<bool> Execute(IncomeTaskState state)
        {
            bool processed = false;
            var xDocuments = state.XmlDocument.Element("documents");
            var xOrder = xDocuments.Elements("receive_order_errors_notification").FirstOrDefault();

            if (xOrder != null)
            {
                var xOrderDetails = xOrder.Elements("order_details").FirstOrDefault();
                var order = new IncomeReceiveOrderError
                {
                    IncomeId = state.Income.IncomeId,
                    SubjectId = xOrder.Element("subject_id")?.Value,
                    ShipperId = xOrder.Element("shipper_id")?.Value,
                    OperationDT = Format.ParseDateTime(xOrder.Element("operation_date")?.Value)
                };
                var detailNo = 0;
                var details = xOrderDetails
                    .Elements("errors")
                    .Select(x => new IncomeReceiveOrderErrorDetail
                    {
                        IncomeId = state.Income.IncomeId,
                        DetailNo = detailNo++,
                        ErrorCode = x.Element("error_code")?.Value,
                        ErrorDesc = x.Element("error_desc")?.Value,
                        ObjectId = x.Element("object_id")?.Value
                    })
                    .ToList();
                if (details.Count > 0)
                {
                    await state.Connection.InsertAsync(order, (x) => x.AttachToTransaction(state.Transaction));
                    foreach (var detail in details)
                        await state.Connection.InsertAsync(detail, (x) => x.AttachToTransaction(state.Transaction));
                    processed = true;
                }
            }
            return processed;
        }
    }
}
