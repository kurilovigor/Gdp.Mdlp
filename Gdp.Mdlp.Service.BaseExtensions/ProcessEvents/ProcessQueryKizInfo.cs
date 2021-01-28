using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdp.Mdlp.Data;
using Dapper;
using Dapper.FastCrud;
using Gdp.Mdlp.Service.Extensibility;

namespace Gdp.Mdlp.Service.BaseExtensions.ProcessEvents
{
    public class ProcessQueryKizInfo : IProcessEvent
    {
        public async Task<bool> Execute(EventTaskState state)
        {
            var queryKizInfo = await state.Connection.GetAsync(new EventQueryKiz { EventId = state.Event.EventId }, (e) => e.AttachToTransaction(state.Transaction));
            var request = state.Event.CreateRequest(queryKizInfo.SubjectId, 0, ActionId.QueryKizInfo);
            await state.Connection.InsertAsync(request, (e) => e.AttachToTransaction(state.Transaction));
            return true;
        }
    }
}
