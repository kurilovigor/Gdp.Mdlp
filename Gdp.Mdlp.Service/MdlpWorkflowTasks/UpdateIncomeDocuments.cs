using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gdp.Mdlp.Data;
using Dapper;
using Dapper.FastCrud;
using Gdp.Mdlp.Utils;
using Newtonsoft.Json;

namespace Gdp.Mdlp.Service.MdlpWorkflowTasks
{
    class UpdateIncomeDocuments : BasicUpdateDocuments
    {
        static log4net.ILog Log { get; set; } = log4net.LogManager.GetLogger(typeof(UpdateIncomeDocuments));
        protected override string Folder => "Income";
        protected override async Task UpdateAsync(IMdlpManager manager, DateTime from, DateTime to)
        {
            await manager.MdlpConnection.ForEachDocumentsIncomeAsync(
                new Model.DocFilter
                {
                    processed_date_from = Format.ToMskDateTime(from),
                    processed_date_to = Format.ToMskDateTime(to),
                    doc_status = "PROCESSED_DOCUMENT"
                },
                async (d) =>
                {
                    await manager.DbConnection.TransactionedAsync(async (c, t) =>
                    {
                        var i = await c.GetIncomeDocumentAsync(d.request_id, d.document_id, t);
                        if (i == null)
                        {
                            i = new IncomeDocument
                            {
                                ClientId = manager.AccountSystem.ClientId,
                                DocStatus = d.doc_status,
                                DocType = d.doc_type,
                                DocumentId = d.document_id,
                                DT = Format.ParseDateTime(d.date),
                                FileUploadType = d.file_uploadtype,
                                ProcessDT = Format.ParseDateTime(d.processed_date),
                                RequestId = d.request_id,
                                SenderId = d.sender,
                                SenderSysId = d.sender_sys_id,
                                SysId = d.sys_id,
                                Version = d.version
                            };
                            i.TrackCreate();
                            i.SetDocumentState(IncomeDocumentStatesEnum.Pending);
                            await c.InsertAsync(i, x => x.AttachToTransaction(t));
                            await c.CreateTaskRegistrationUpdateIfNeeded(d.sender_sys_id, d.sender, manager.AccountSystem.ClientId, t);

                            Log.Info($"Income document {i.DocumentId} registered");
                        }
                    });
                },
                manager.CancelationToken);
        }
    }
}
