using Gdp.Mdlp.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Gdp.Mdlp.Service.Extensibility
{
    public interface IProcessRequest
    {
        Task Execute(RequestTaskState state);
    }
}
