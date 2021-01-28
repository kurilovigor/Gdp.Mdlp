using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gdp.Mdlp.Data;
using Dapper;
using Dapper.FastCrud;
using Gdp.Mdlp.Service.Extensibility;

namespace Gdp.Mdlp.Service.BaseExtensions.ProcessRequest
{
    class Unpack : IProcessRequest
    {
        public async Task Execute(RequestTaskState state)
        {
            var unpack = await state.Connection.GetAsync(new EventUnpack { EventId = state.Request.EventId }, x => x.AttachToTransaction(state.Transaction));
            state.XmlDocument = Documents.Unpack(unpack.SubjectId, unpack.Sscc, unpack.OperationDT ?? DateTime.Now);
        }
    }
}
