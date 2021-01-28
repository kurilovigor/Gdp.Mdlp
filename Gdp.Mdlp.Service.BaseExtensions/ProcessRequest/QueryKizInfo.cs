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
    public class QueryKizInfo : IProcessRequest
    {
        public async Task Execute(RequestTaskState state)
        {
            var queryKizInfo = await state.Connection.GetAsync(new EventQueryKiz { EventId = state.Request.EventId }, x=>x.AttachToTransaction(state.Transaction));
            if (queryKizInfo.Sgtin != null && queryKizInfo.Sgtin != string.Empty)
                state.XmlDocument = Documents.QueryKizInfo(queryKizInfo.SubjectId, queryKizInfo.Sgtin);
            else if (queryKizInfo.SsccDown != null && queryKizInfo.SsccDown != string.Empty)
                state.XmlDocument = Documents.QueryKizInfoSsccDown(queryKizInfo.SubjectId, queryKizInfo.SsccDown);
            else if (queryKizInfo.SsccUp != null && queryKizInfo.SsccUp != string.Empty)
                state.XmlDocument = Documents.QueryKizInfoSsccUp(queryKizInfo.SubjectId, queryKizInfo.SsccUp);
            else
                throw new Exception($"Invalid event {queryKizInfo.EventId} (SsccDown and Sgtin are empty)");
        }
    }
}
