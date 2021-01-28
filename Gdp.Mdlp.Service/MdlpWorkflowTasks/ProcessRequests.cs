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
using Gdp.Mdlp.Service.Extensibility;

namespace Gdp.Mdlp.Service.MdlpWorkflowTasks
{
    class ProcessRequests: IMdlpWorkflowTask
    {
        static log4net.ILog Log { get; set; } = log4net.LogManager.GetLogger(typeof(ProcessRequests));
        ProcessRequestFactory processRequest = null;
        ProcessTicketFactory processTicket = null;
        public async Task ExecuteAsync(IMdlpManager manager)
        {
            if(processRequest == null)
                processRequest = new ProcessRequestFactory(manager.Service.Configuration);
            if(processTicket == null)
                processTicket = new ProcessTicketFactory(manager.Service.Configuration);

            await manager.ForEachAsync(
                TakeAllRequests(manager), 
                async (request) => await ProcessRequest(manager, request));
        }
        protected IEnumerable<Request> TakeAllRequests(IMdlpManager manager)
        {
            return Functional.Chain(()=> TakeRequests(manager));
        }
        protected IEnumerable<Request> TakeRequests(IMdlpManager manager)
        {
            return manager.DbConnection.TakeRequests
                (
                    new RequestStateEnum[]
                    {
                        RequestStateEnum.Created,
                        //RequestStateEnum.DocumentSent,
                        RequestStateEnum.TickedDownloadPending,
                        RequestStateEnum.QueryReusingRecover
                    },
                    manager.ClientAddresses.Select(x => x.Key),
                    manager.Service.Configuration.Mdlp.RequestsTile,
                    manager.Service.Configuration.Mdlp.MinPriority
                );
        }

        private async Task ProcessRequest(IMdlpManager manager, Request request)
        {
            if (request.IsRequestState(RequestStateEnum.Created))
                await OnRequestSendDocument(manager, request);
            else if (request.IsRequestState(RequestStateEnum.TickedDownloadPending))
                await OnRequestDownloadTicked(manager, request);
            else if (request.IsRequestState(RequestStateEnum.QueryReusingRecover))
                await OnRequestQueryReusingRecover(manager, request);
        }
        private async Task OnRequestQueryReusingRecover(IMdlpManager manager, Request request)
        {
            await manager.TryActionAsync(
                async () =>
                {
                    request.QueryReusingRecoverAttempts = (request.QueryReusingRecoverAttempts ?? 0) + 1;
                    request.QueryReusingRecoverLastDT = DateTime.Now;
                    if (request.QueryReusingRecoverFirstDT == null)
                        request.QueryReusingRecoverFirstDT = request.QueryReusingRecoverLastDT;
                    var docs = await manager.MdlpConnection.DocumentsRequestAsync(request.RequestId);
                    if (docs != null && docs.Length > 0 && docs[0].document_id != null && docs[0].document_id != string.Empty)
                    {
                        request.DocumentId = docs[0].document_id;
                        request.SetRequestState(RequestStateEnum.DocumentSent);
                        await manager.DbConnection.UpdateAsync(request);
                    }
                    else
                    {
                        request.SetRequestState(RequestStateEnum.QueryReusingRecoverFailed);
                        await manager.DbConnection.UpdateAsync(request);
                    }
                },
                async (e) =>
                {
                    request.SetRequestState(RequestStateEnum.QueryReusingRecoverFailed);
                    await manager.DbConnection.UpdateAsync(request);
                });
        }
        private async Task OnRequestSendDocument(IMdlpManager manager, Request request)
        {
            Event evt = null;
            await manager.TryActionAsync(async () =>
            {
                evt = await manager.DbConnection.GetAsync<Event>(new Event { EventId = request.EventId });
                var addon = processRequest[request.ActionId];
                if (addon == null)
                {
                    Log.Warn($"Send document request {request.RequestId} failed! Unsupported action {request.ActionId}");
                }
                else
                {
                    await manager.DbConnection.TransactionedAsync(async (c, t) =>
                    {
                        var state = new RequestTaskState
                        {
                            Connection = c,
                            Transaction = t,
                            Request = request
                        };
                        await addon.Execute(state);
                        // send document
                        request.DocumentId = await manager.MdlpConnection.DocumentSendAsync(state.XmlDocument, request.RequestId);
                        request.DocumentSentDT = DateTime.Now;
                        await c.SetEventAndRequestStates(t, evt, request, EventStateEnum.Processing, RequestStateEnum.DocumentSent);
                    });
                    Log.Info($"Document {request.DocumentId} sent");
                }
            },
            async (e) =>
            {
                bool handled = false;
                if (e is ApiCallException)
                {
                    var apiError = (e as ApiCallException);
                    if (
                        apiError.Trace.StatusCode == System.Net.HttpStatusCode.BadRequest &&
                        apiError.Trace.Response != null &&
                        apiError.Trace.Response != string.Empty)
                    {
                        var r = JsonConvert.DeserializeObject<Mdlp.Model.BaseResponse>(apiError.Trace.Response);
                        if (r.error_code == "query.reusing")
                        {
                            await manager.DbConnection.SetEventAndRequestStates(evt, request, EventStateEnum.Processing, RequestStateEnum.QueryReusingRecover);
                            Log.Debug($"Document {request.DocumentId} query reusing recover");
                            handled = true;
                        }
                    }
                }
                if (!handled)
                    await manager.DbConnection.SetEventAndRequestStates(evt, request, EventStateEnum.Failed, RequestStateEnum.DocumentSendFailed);
            },
            $"Send document request {request.RequestId}");
        }
        private async Task OnRequestDownloadTicked(IMdlpManager manager, Request request)
        {
            bool processed = false;
            await manager.TryActionAsync(
                async () =>
                {
                    request.TicketDownloadAttempts = (request.TicketDownloadAttempts ?? 0) + 1;
                    request.TickedDownloadLastDT = DateTime.Now;
                    if (request.TickedDownloadFirstDT == null)
                        request.TickedDownloadFirstDT = request.TickedDownloadLastDT;
                    XDocument ticket = await manager.MdlpConnection.TicketDownloadByIdAsync(request.DocumentId);
                    if (manager.Service.Configuration.Mdlp.CacheTickets)
                    {
                        manager.TryAction(
                            () => ticket.Save($"cache\\ticket\\{request.DocumentId}.xml"),
                            (e) => Log.Error($"Save ticket {request.DocumentId} failed")
                        );
                    }
                    var evt = await manager.DbConnection.GetAsync(new Event { EventId = request.EventId });
                    var action = ticket.Element("documents").Elements().FirstOrDefault();
                    var action_id = action.Attribute("action_id")?.Value ?? string.Empty;
                    if (action_id == string.Empty)
                        Log.Info($"Invalid ticket \"{request.DocumentId}\"");
                    else
                    {
                        var addon = processTicket[action_id];
                        if (addon != null)
                            await manager.DbConnection.TransactionedAsync(async (c, t) => 
                                {
                                    processed = await addon.Execute (
                                        new TicketTaskState
                                        {
                                            Connection = c, 
                                            Transaction = t,
                                            Request = request,
                                            XmlDocument = ticket
                                        });
                                });
                    }

                    if (processed)
                    {
                        await manager.DbConnection.TransactionedAsync(async (c, t) =>
                        {
                            request.SetRequestState(RequestStateEnum.TickedDownloaded);
                            await c.UpdateAsync(request, (x)=>x.AttachToTransaction(t));
                            Log.Info($"Ticket {request.DocumentId} downloaded");

                            evt.SetEventState(EventStateEnum.Processed);
                            await c.UpdateAsync(evt, (x) => x.AttachToTransaction(t));
                        });
                    }
                    else
                    {
                        await manager.DbConnection.TransactionedAsync(async (c, t) =>
                        {
                            request.SetRequestState(RequestStateEnum.TickedDownloaded);
                            await c.UpdateAsync(request, (x) => x.AttachToTransaction(t));
                            Log.Warn($"Ticket {request.DocumentId} skipped");
                            evt.SetEventState(EventStateEnum.Failed);
                            await c.UpdateAsync(evt, (x) => x.AttachToTransaction(t));
                            Log.Error($"Event {evt.EventId} failed");
                        });
                    }
                },
                async (e) =>
                {
                    request.SetRequestState(RequestStateEnum.TickedDownloadFailed);
                    await manager.DbConnection.UpdateAsync(request);
                },
                $"Download ticket {request.DocumentId}");
        }
    }
}
