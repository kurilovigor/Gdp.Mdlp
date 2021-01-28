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
    public class QueryHierarchyInfo : IProcessRequest
    {
        public async Task Execute(RequestTaskState state)
        {
            var queryHierarchyInfo = await state.Connection.GetAsync(
                new EventQueryHierarchyInfo
                {
                    EventId = state.Request.EventId
                }, x => x.AttachToTransaction(state.Transaction));
            state.XmlDocument = Documents.Create(
                    new XElement("query_hierarchy_info",
                        new XAttribute("action_id", ActionId.QueryHierarchyInfo),
                        new XElement("subject_id", queryHierarchyInfo.SubjectId),
                        new XElement("sscc", queryHierarchyInfo.Sscc)
                    ));
        }
    }
}
