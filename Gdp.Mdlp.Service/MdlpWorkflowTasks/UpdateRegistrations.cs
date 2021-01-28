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
    class UpdateRegistrations: IMdlpWorkflowTask
    {
        static log4net.ILog Log { get; set; } = log4net.LogManager.GetLogger(typeof(UpdateRegistrations));
        public async Task ExecuteAsync(IMdlpManager manager)
        {
            var tasks = await manager.DbConnection.GetTaskRegistrationUpdates(manager.AccountSystem.ClientId);
            await manager.ForEachAsync(tasks, async (task)=> await ProcessTask(manager, task));
        }
        private async Task ProcessTask(IMdlpManager manager, TaskRegistrationUpdate task)
        {
            await manager.TryActionAsync(async () =>
            {
                await manager.MdlpConnection.ForEachPartnersReestr(new Model.PartnersFilter
                {
                    reg_entity_type = 1,
                    op_exec_date_start = task.OpExecStartDT.HasValue ? Format.ToDateTimeOffset(task.OpExecStartDT.Value) : null,
                    op_exec_date_end = task.OpExecEndDT.HasValue ? Format.ToDateTimeOffset(task.OpExecEndDT.Value) : null,
                    system_subj_id = task.SystemSubjId
                },
                async (x) =>
                {
                    try
                    {
                        if (!(await manager.DbConnection.RegistrationExists(x.system_subj_id)))
                        {
                            var reg = new Registration
                            {
                                SystemSubjId = x.system_subj_id,
                                OrgName = x.ORG_NAME,
                                Inn = x.inn,
                                Kpp = x.KPP,
                                Ogrn = x.OGRN,
                                CountryCode = x.country_code,
                                FederalSubjectCode = x.federal_subject_code,
                                FirstName = x.FIRST_NAME,
                                MiddleName = x.MIDDLE_NAME,
                                LastName = x.LAST_NAME
                            };
                            await manager.DbConnection.InsertAsync(reg);
                            Log.Info($"New registration {x.system_subj_id} {x.OGRN}");
                        }
                        if (x.branches != null)
                        {
                            foreach (var b in x.branches)
                                if (!(await manager.DbConnection.BranchExists(b.id)))
                                {
                                    var branch = new Branch
                                    {
                                        Id = b.id,
                                        SystemSubjId = x.system_subj_id,
                                        Status = b.status,
                                        Address = b.address_resolved?.address
                                    };
                                    await manager.DbConnection.InsertAsync(branch);
                                    Log.Info($"New branch {branch.Id} {branch.Address}");
                                }
                        }
                        if (x.safe_warehouses != null)
                        {
                            foreach (var w in x.safe_warehouses)
                                if (!(await manager.DbConnection.SafeWarehouseExists(w.id)))
                                {
                                    var warehouse = new SafeWarehous
                                    {
                                        Id = w.id,
                                        SystemSubjId = x.system_subj_id,
                                        Status = w.status,
                                        Address = w.address_resolved?.address
                                    };
                                    await manager.DbConnection.InsertAsync(warehouse);
                                    Log.Info($"New warehouse {warehouse.Id} {warehouse.Address}");
                                }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message);
                        Log.Error(JsonConvert.SerializeObject(x));
                    }
                },
                manager.CancelationToken);
            });
            await manager.DbConnection.DeleteAsync(task);
        }
    }
}
