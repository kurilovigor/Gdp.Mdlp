using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdp.Mdlp.Data;
using Dapper;
using Dapper.FastCrud;
using System.Data.SqlClient;
using Gdp.Mdlp.Service.Extensibility;

namespace Gdp.Mdlp.Service.BaseExtensions.ProcessEvents
{
    public class ProcessPosting : IProcessEvent
    {
        public async Task<bool> Execute(EventTaskState state)
        {
            var posting = await state.Connection.GetAsync(new EventPosting { EventId = state.Event.EventId }, (e) => e.AttachToTransaction(state.Transaction));
            var request = state.Event.CreateRequest(posting.SubjectId, 0, ActionId.Posting);
            await state.Connection.InsertAsync(request, (e) => e.AttachToTransaction(state.Transaction));
            return true;
        }
    }
}
