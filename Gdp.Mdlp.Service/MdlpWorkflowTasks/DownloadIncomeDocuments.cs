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
    class DownloadIncomeDocuments : IMdlpWorkflowTask
    {
        static log4net.ILog Log { get; set; } = log4net.LogManager.GetLogger(typeof(DownloadIncomeDocuments));
        ProcessIncomesFactory processIncome = null;
        public async Task ExecuteAsync(IMdlpManager manager)
        {
            if (processIncome == null)
                processIncome = new ProcessIncomesFactory(manager.Service.Configuration);
            var incomes = await manager.DbConnection.GetIncomeDocuments(manager.AccountSystem.ClientId, IncomeDocumentStatesEnum.Pending);
            await manager.ForEachAsync(incomes, async (income)=> await ProcessIncome(manager, income));
        }
        protected async Task ProcessIncome(IMdlpManager manager, IncomeDocument income)
        {
            bool processed = false;
            await manager.TryActionAsync(
                async () =>
                {
                    var process = processIncome[income.DocType];
                    if (process != null)
                    {
                        income.DownloadAttempts = (income.DownloadAttempts ?? 0) + 1;
                        income.DownloadAttemptLastDT = DateTime.Now;
                        if (income.DownloadAttemptFirstDT == null)
                            income.DownloadAttemptFirstDT = income.DownloadAttemptLastDT;
                        var xIncome = await manager.MdlpConnection.DocumentDownloadByIdAsync(income.DocumentId);
                        if (manager.Service.Configuration.Mdlp.CacheIncomes)
                        {
                            manager.TryAction(
                                () => xIncome.Save($"cache\\income\\{income.DocumentId}.xml"),
                                (e) => Log.Error($"Save income {income.DocumentId} failed")
                            );

                        }
                        await manager.DbConnection.TransactionedAsync(async (c,t)=> 
                        {
                            processed = await process.Execute(
                                new IncomeTaskState
                                {
                                    Connection = c,
                                    Transaction = t,
                                    Income = income,
                                    XmlDocument = xIncome
                                });
                        });
                    }
                    if (processed)
                    {
                        income.SetDocumentState(IncomeDocumentStatesEnum.Downloaded);
                        await manager.DbConnection.UpdateAsync(income);
                        Log.Info($"Income {income.DocumentId} downloaded");
                    }
                    else
                    {
                        income.SetDocumentState(IncomeDocumentStatesEnum.Skipped);
                        await manager.DbConnection.UpdateAsync(income);
                        Log.Warn($"Income {income.DocumentId} skipped");
                    }
                },
                async (e) =>
                {
                    income.SetDocumentState(IncomeDocumentStatesEnum.DownloadFailed);
                    await manager.DbConnection.UpdateAsync(income);
                },
                $"Download income {income.DocumentId}");
        }
    }
}
