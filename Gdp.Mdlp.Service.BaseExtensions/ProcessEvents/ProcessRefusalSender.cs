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
    public class ProcessRefusalSender : IProcessEvent
    {
        public async Task<bool> Execute(EventTaskState state)
        {
            var evtRefusalSender = await state.Connection.GetAsync(
                new EventRefusalSender
                {
                    EventId = state.Event.EventId
                },
                x => x.AttachToTransaction(state.Transaction));
            var request = state.Event.CreateRequest(evtRefusalSender.SubjectId, ActionId.RefusalSender);
            await state.Connection.InsertAsync(request, (x) => x.AttachToTransaction(state.Transaction));
            return true;
        }
    }
}
