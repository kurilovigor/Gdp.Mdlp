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
    class UpdateOutcomeDocuments : BasicUpdateDocuments
    {
        static log4net.ILog Log { get; set; } = log4net.LogManager.GetLogger(typeof(UpdateOutcomeDocuments));
        protected override string Folder => "Outcome";
        protected override async Task UpdateAsync(IMdlpManager manager, DateTime from, DateTime to)
        {
            await manager.MdlpConnection.ForEachDocumentsOutcomeAsync(
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
                        var i = await c.GetOutcomeDocumentAsync(d.request_id, d.document_id, t);
                        if (i == null)
                        {
                            i = new OutcomeDocument
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
                                SysId = d.sys_id,
                                Version = d.version
                            };
                            i.TrackCreate();
                            i.SetDocumentState(OutcomeDocumentStatesEnum.Pending);
                            await c.InsertAsync(i, x => x.AttachToTransaction(t));
                            Log.Info($"Outcome document {i.DocumentId} registered");
                                    // update requests pending state
                                    var requests = await c.QueryAsync<Request>(
                                @"SELECT * FROM dbo.Requests r WHERE r.DocumentId=@DocumentId AND r.RequestStateId=1",
                                new { i.DocumentId }, t);
                            foreach (var r in requests)
                            {
                                r.SetRequestState(RequestStateEnum.TickedDownloadPending);
                                await c.UpdateAsync(r, x => x.AttachToTransaction(t));
                            }
                        }
                    });
                },
                manager.CancelationToken);
        }
    }
}
