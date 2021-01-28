using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Gdp.Mdlp.Data;
using Dapper;
using Dapper.FastCrud;
using Gdp.Mdlp.Service.Extensibility;

namespace Gdp.Mdlp.Service.BaseExtensions.ProcessEvents
{
    public class ProcessQueryHierarchyInfo : IProcessEvent
    {
        public async Task<bool> Execute(EventTaskState state)
        {
            var queryHierarchyInfo = await state.Connection.GetAsync(new EventQueryHierarchyInfo { EventId = state.Event.EventId }, (e) => e.AttachToTransaction(state.Transaction));
            var request = state.Event.CreateRequest(queryHierarchyInfo.SubjectId, 0, ActionId.QueryHierarchyInfo);
            await state.Connection.InsertAsync(request, (e) => e.AttachToTransaction(state.Transaction));
            return true;
        }
    }
}
