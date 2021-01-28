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
    public class ProcessShipping : IProcessEvent
    {
        public async Task<bool> Execute(EventTaskState state)
        {
            bool requestCreated = false;
            var ship = await state.Connection.GetAsync(
                new EventShipping
                {
                    EventId = state.Event.EventId
                },
                (x) => x.AttachToTransaction(state.Transaction));
            var details = (await state.Connection.QueryAsync<EventShippingDetail>(
                @"SELECT * FROM dbo.EventShippingDetails WHERE EventId = @EventId",
                new
                {
                    state.Event.EventId
                }, state.Transaction)).ToList();
            if (details.Count > 0)
            {
                foreach (var x in details)
                {
                    if (x.Sscc != null && x.Sscc != string.Empty)
                        await state.Connection.RegisterSsccAsync(
                            new SSCC
                            {
                                Sscc = x.Sscc,
                                State = 0,
                                SubjectId = ship.SubjectId
                            }, state.Transaction);
                    if (x.Sgtin != null && x.Sgtin != string.Empty)
                        await state.Connection.RegisterSgtinAsync(
                            new SGTIN
                            {
                                Sgtin = x.Sgtin,
                                State = 0,
                                SubjectId = ship.SubjectId
                            }, state.Transaction);
                }
                var request = state.Event.CreateRequest(ship.SubjectId, ActionId.MoveOrder);
                await state.Connection.InsertAsync(request, (x) => x.AttachToTransaction(state.Transaction));
                requestCreated = true;
            }
            return requestCreated;
        }

    }
}
