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

namespace Gdp.Mdlp.Service
{
    class MdlpManager : IDisposable, IMdlpManager
    {
        log4net.ILog Log { get; set; } = log4net.LogManager.GetLogger(typeof(MdlpManager));
        bool refreshedMdlpAddresses = false;
        CancellationTokenSource tokenSource;
        public CancellationToken CancelationToken { get; set; }
        public SqlConnection DbConnection { get; set; } = null;
        Task executeTask;
        public Connection MdlpConnection { get; set; } = null;
        public AccountSystemConfiguration AccountSystem { get; set; }
        public Dictionary<string, ClientAddress> ClientAddresses { get; set; }
        public Service Service { get; private set; }
        public int RefreshInterval { get; set; }
        public bool LoggedIn { get; set; } = false;
        public List<IMdlpWorkflowTask> Workflow { get; set; } = new List<IMdlpWorkflowTask>();
        public MdlpManager(Service service, Uri api, AccountSystemConfiguration accountSystem)
        {
            Service = service;
            AccountSystem = accountSystem;
            MdlpConnection = new Connection(api, accountSystem);
            // build workflow
            // TODO: build from configuration
            Workflow.Add(new MdlpWorkflowTasks.UpdateRegistrations());
            Workflow.Add(new MdlpWorkflowTasks.UpdateOutcomeDocuments());
            Workflow.Add(new MdlpWorkflowTasks.UpdateIncomeDocuments());
            Workflow.Add(new MdlpWorkflowTasks.DownloadIncomeDocuments());
            Workflow.Add(new MdlpWorkflowTasks.ProcessRequests());
        }
        public async Task Start()
        {
            tokenSource = new CancellationTokenSource();
            CancelationToken = tokenSource.Token;
            executeTask = Task.Run(new Func<Task>(Execute));
        }
        public async Task Stop()
        {
            tokenSource.Cancel();
            await executeTask;
        }
        public void Dispose()
        {
            if (MdlpConnection != null)
            {
                MdlpConnection.Dispose();
                MdlpConnection = null;
            }
            if (DbConnection != null)
            {
                DbConnection.Dispose();
                DbConnection = null;
            }
        }
        async Task RefreshMdlpAddresses()
        {
            var addresses = DbConnection
                .GetByClientId(AccountSystem.ClientId)
                .ToDictionary(x => x.SubjectId);
            try
            {
                var addressEntries = await MdlpConnection.AddressAllAsync();
                foreach (var subjectId in addresses.Keys.ToList())
                {
                    if (addressEntries.FirstOrDefault(x => x.address_id == subjectId) == null)
                    {
                        await DbConnection.DeleteAsync(addresses[subjectId]);
                        addresses.Remove(subjectId);
                    }
                }
                foreach (var a in addressEntries)
                {
                    if (!addresses.ContainsKey(a.address_id))
                    {
                        var newAddress = new ClientAddress();
                        newAddress.ClientId = AccountSystem.ClientId;
                        newAddress.ClientName = AccountSystem.Name;
                        newAddress.SubjectId = a.address_id;
                        newAddress.AddressDescription = a.address.address_description;
                        newAddress.TrackCreate();
                        addresses.Add(newAddress.SubjectId, newAddress);
                        await DbConnection.InsertAsync(newAddress);
                    }
                }
            }
            catch (Exception e)
            {
                LogError(e);
            }
            ClientAddresses = addresses;
            refreshedMdlpAddresses = ClientAddresses.Count > 0;
            if (refreshedMdlpAddresses)
                LogDebug($"Refreshed client addresses ({AccountSystem.Name}):\n{String.Join("\n", ClientAddresses.Select(x=>x.Key))}");
        }
        async Task Login()
        {
            await MdlpConnection.LoginAsync();
            LoggedIn = true;
            LogInfo($"Logged in ({AccountSystem.Name})");
        }
        public async Task Execute()
        {
            try
            {
                using (DbConnection = new SqlConnection(Service.Configuration.Event.ConnectionString))
                {
                    DbConnection.Open();
                    while (!CancelationToken.IsCancellationRequested)
                    {
                        RefreshInterval = Service.Configuration.Mdlp.RefreshInterval;
                        await TryActionAsync(async ()=> 
                        {
                            // login
                            if (!LoggedIn)
                                await Login();
                            // update addresses
                            if (!refreshedMdlpAddresses)
                                await RefreshMdlpAddresses();
                            await ForEachAsync(
                                Workflow, 
                                async (task) => 
                                {
                                    log4net.LogicalThreadContext.Properties["CustomTreadInfo"] = $" [{AccountSystem.Name}]";
                                    await task.ExecuteAsync(this);
                                });
                        });
                        // следующая итерация
                        await Task.Delay(RefreshInterval, CancelationToken);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                LogDebug($"A task was canceled ({AccountSystem.Name})");
            }
            catch (Exception ae)
            {
                bool handled = false;
                if (ae is AggregateException)
                {
                    foreach (var e in (ae as AggregateException).InnerExceptions)
                        if (e is TaskCanceledException)
                        {
                            if (tokenSource.IsCancellationRequested)
                                tokenSource.Cancel();
                            
                            LogDebug($"A task was canceled ({AccountSystem.Name})");
                            handled = true;
                        }
                }
                if (!handled)
                {
                    LogFatal(ae);
                    Service.Halt();
                }
            }
            // do logout
            if (LoggedIn)
            {
                try
                {
                    await MdlpConnection.LogoutAsync();
                    LogInfo($"Logged out ({AccountSystem.Name})");
                }
                catch (TaskCanceledException)
                {
                    LogDebug($"A task was canceled ({AccountSystem.Name})");
                }
                catch (ApiCallException e)
                {
                    if(e.Trace.StatusCode != System.Net.HttpStatusCode.Unauthorized)
                        LogError(e);
                }
                catch (Exception e)
                {
                    LogError(e);
                }
            }
        }
        public void TryAction(Action onExecute, Action<Exception> onError = null, string contextInfo = "")
        {
            try
            {
                onExecute();
            }
            catch (ApiException e)
            {
                LogError(e, contextInfo);
                RefreshInterval = Service.Configuration.Mdlp.ApiErrorRecoverInterval;
                onError?.Invoke(e);
            }
            catch (ApiCallException e)
            {
                LogError(e, contextInfo);
                if (e.Trace.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LoggedIn = false;
                    RefreshInterval = Service.Configuration.Mdlp.ReloginInterval;
                }
                else
                    RefreshInterval = Service.Configuration.Mdlp.ApiErrorRecoverInterval;
                onError?.Invoke(e);
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                LogError(e, contextInfo);
                RefreshInterval = Service.Configuration.Mdlp.ApiErrorRecoverInterval;
                onError?.Invoke(e);
            }
            catch (Exception e)
            {
                LogFatal(e, contextInfo);
                Service.Halt();
                tokenSource.Cancel();
                onError?.Invoke(e);
            }
        }
        public async Task TryActionAsync(Func<Task> onExecute, Func<Exception, Task> onError = null, string contextInfo = "")
        {
            try
            {
                await onExecute();
            }
            catch (ApiException e)
            {
                LogError(e, contextInfo);
                RefreshInterval = Service.Configuration.Mdlp.ApiErrorRecoverInterval;
                if (onError != null)
                    await onError(e);
            }
            catch (ApiCallException e)
            {
                LogError(e, contextInfo);
                if (e.Trace.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LoggedIn = false;
                    RefreshInterval = Service.Configuration.Mdlp.ReloginInterval;
                }
                else
                    RefreshInterval = Service.Configuration.Mdlp.ApiErrorRecoverInterval;
                if (onError != null)
                    await onError(e);
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                LogError(e, contextInfo);
                RefreshInterval = Service.Configuration.Mdlp.ApiErrorRecoverInterval;
                if (onError != null)
                    await onError(e);
            }
            catch (TaskCanceledException)
            {
                if (!tokenSource.IsCancellationRequested)
                    tokenSource.Cancel();
                throw;
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                    if (e is TaskCanceledException)
                    {
                        if (tokenSource.IsCancellationRequested)
                            tokenSource.Cancel();
                        throw;
                    }
                await DoErrorAsync(ae, onError, contextInfo);
            }
            catch (Exception e)
            {
                await DoErrorAsync(e, onError, contextInfo);
            }
        }
        private async Task DoErrorAsync(Exception e, Func<Exception, Task> onError, string contextInfo)
        {
            LogFatal(e, contextInfo);
            Service.Halt();
            tokenSource.Cancel();
            if (onError != null)
                await onError(e);
        }
        private async Task<T> DoErrorAsync<T>(Exception e, Func<Exception, Task<T>> onError, string contextInfo)
        {
            T result = default(T);
            LogFatal(e, contextInfo);
            Service.Halt();
            tokenSource.Cancel();
            if (onError != null)
                result = await onError(e);
            return result;
        }
        protected async Task<T> TryFuncAsync<T>(Func<Task<T>> onExecute, Func<Exception, Task<T>> onError = null, string contextInfo = "")
        {
            T result = default(T);
            try
            {
                result = await onExecute();
            }
            catch (ApiException e)
            {
                LogError(e, contextInfo);
                RefreshInterval = Service.Configuration.Mdlp.ApiErrorRecoverInterval;
                if (onError != null)
                    result = await onError(e);
            }
            catch (ApiCallException e)
            {
                LogError(e, contextInfo);
                if (e.Trace.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    LoggedIn = false;
                    RefreshInterval = Service.Configuration.Mdlp.ReloginInterval;
                }
                else
                    RefreshInterval = Service.Configuration.Mdlp.ApiErrorRecoverInterval;
                if (onError != null)
                    result = await onError(e);
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                LogError(e, contextInfo);
                RefreshInterval = Service.Configuration.Mdlp.ApiErrorRecoverInterval;
                if (onError != null)
                    result = await onError(e);
            }
            catch (TaskCanceledException)
            {
                if (!tokenSource.IsCancellationRequested)
                    tokenSource.Cancel();
                throw;
            }
            catch (AggregateException ae)
            {
                foreach (var e in ae.InnerExceptions)
                    if (e is TaskCanceledException)
                    {
                        if (tokenSource.IsCancellationRequested)
                            tokenSource.Cancel();
                        throw;
                    }
                result = await DoErrorAsync(ae, onError, contextInfo);
            }
            catch (Exception e)
            {
                result = await DoErrorAsync(e, onError, contextInfo);
            }
            return result;
        }
        protected void LogError(Exception e, string contextInfo = "")
        {
            log4net.LogicalThreadContext.Properties["CustomTreadInfo"] = $" [{AccountSystem.Name}]";
            if (contextInfo == string.Empty)
            {
                if(e is ApiCallException)
                    Log.Error(e.Message);
                else
                    Log.Error(e);
            }
            else
            {
                if (e is ApiCallException)
                    Log.Error(contextInfo + " failed \n"+e.Message);
                else
                    Log.Error(contextInfo + " failed", e);
            }
        }
        protected void LogFatal(Exception e, string contextInfo = "")
        {
            log4net.LogicalThreadContext.Properties["CustomTreadInfo"] = $" [{AccountSystem.Name}]";
            if (contextInfo == string.Empty)
                Log.Fatal(e);
            else
                Log.Fatal(contextInfo + " failed", e);
        }
        protected void LogInfo(string message)
        {
            log4net.LogicalThreadContext.Properties["CustomTreadInfo"] = $" [{AccountSystem.Name}]";
            Log.Info(message);
        }
        protected void LogDebug(string message)
        {
            log4net.LogicalThreadContext.Properties["CustomTreadInfo"] = $" [{AccountSystem.Name}]";
            Log.Debug(message);
        }
        public async Task ForEachAsync<T>(IEnumerable<T> elements, Func<T, Task> onElement)
        {
            foreach (var element in elements)
            {
                if (CancelationToken.IsCancellationRequested)
                    break;
                if (!(ClientAddresses != null && ClientAddresses.Count > 0 && LoggedIn))
                    break;
                await onElement(element);
            }
        }
    }
}
