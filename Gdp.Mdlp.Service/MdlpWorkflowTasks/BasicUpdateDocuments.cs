using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdp.Mdlp.Data;
using Dapper;
using Dapper.FastCrud;


namespace Gdp.Mdlp.Service.MdlpWorkflowTasks
{
    class BasicUpdateDocuments : IMdlpWorkflowTask
    {
        protected virtual string Folder
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        protected virtual async Task UpdateAsync(IMdlpManager manager, DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }
        public async Task ExecuteAsync(IMdlpManager manager)
        {
            var to = DateTime.Now;
            var p = await manager.DbConnection.GetAsync<FolderUpdateDT>(
                new FolderUpdateDT
                {
                    ClientId = manager.AccountSystem.ClientId,
                    Folder = Folder
                });
            var from = p?.DT ?? to.AddMinutes(-15);

            if (from < to)
                await manager.TryActionAsync(async ()=> 
                {
                    await UpdateAsync(manager, from, to);
                    if (p == null)
                    {
                        p = new FolderUpdateDT
                        {
                            ClientId = manager.AccountSystem.ClientId,
                            Folder = Folder,
                            DT = to
                        };
                        await manager.DbConnection.InsertAsync(p);
                    }
                    else
                    {
                        p.DT = to;
                        await manager.DbConnection.UpdateAsync(p);
                    }
                });
        }
    }
}
