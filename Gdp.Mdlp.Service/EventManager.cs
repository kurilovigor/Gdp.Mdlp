using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gdp.Mdlp.Data;
using Dapper;
using Dapper.FastCrud;
using System.Xml.Linq;
using Gdp.Mdlp.Utils;
using Gdp.Mdlp.Service.Extensibility;

namespace Gdp.Mdlp.Service
{
    class EventManager: IDisposable
    {
        static log4net.ILog Log { get; set; } = log4net.LogManager.GetLogger(typeof(EventManager));
        SqlConnection connection = null;
        ProcessEventsFactory processEventsFactory;
        CancellationTokenSource tokenSource;
        CancellationToken token;
        Task executeTask;
        public Service Service { get; private set; }
        public EventManager(Service service)
        {
            Service = service;
            processEventsFactory = new ProcessEventsFactory(service.Configuration);
        }
        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }

        public async Task Start()
        {
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            executeTask = Task.Run(new Func<Task>(Execute));
        }
        public async Task Stop()
        {
            tokenSource.Cancel();
            await executeTask;
        }
        public async Task Execute()
        {
            var config = Service.Configuration.Event;
            try
            {
                using (connection = new SqlConnection(Service.Configuration.Event.ConnectionString))
                {
                    connection.Open();
                    while (!token.IsCancellationRequested)
                    {
                        foreach (var e in TakeAllEvents())
                        {
                            if (token.IsCancellationRequested)
                                break;
                            await OnEvent(e);
                        }
                        if (token.IsCancellationRequested)
                            break;
                        await Task.Delay(config.RefreshInterval, token);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Log.Debug("A task was canceled");
            }
            catch (Exception e)
            {
                Log.Fatal(e);
                Service.Halt();
            }
            Log.Info("Left execution loop");
        }

        protected IEnumerable<Event> TakeEvents()
        {
            return connection.TakeEvents(
                EventStateEnum.Pending, 
                Service.Configuration.Event.EventsTile,
                Service.Configuration.Event.MinPriority);
        }
        protected IEnumerable<Event> TakeAllEvents()
        {
            return Functional.Chain<Event>(new Func<IEnumerable<Event>>(TakeEvents));
        }
        protected async Task OnEvent(Event evt)
        {
            try
            {
                var addon = processEventsFactory[((EventTypeEnum)evt.EventTypeId).ToString()];
                if (addon != null)
                {
                    await connection.TransactionedAsync(async (c, t) =>
                    {
                        var result = await addon.Execute(
                            new EventTaskState
                            {
                                Connection = c,
                                Transaction = t,
                                Event = evt 
                            });
                        evt.SetEventState(result ? EventStateEnum.Processing : EventStateEnum.Skipped);
                        await c.UpdateAsync(evt, (e) => e.AttachToTransaction(t));

                    });
                    Log.Debug($"Event {evt.EventId} [{(EventTypeEnum)evt.EventTypeId}] enqueued");
                }
                else
                {
                    evt.SetEventState(EventStateEnum.Skipped);
                    await connection.UpdateAsync(evt);
                    Log.Debug($"Event {evt.EventId} [{(EventTypeEnum)evt.EventTypeId}] skipped");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Event {evt.EventId} failed", e);
            }
        }
    }
}
