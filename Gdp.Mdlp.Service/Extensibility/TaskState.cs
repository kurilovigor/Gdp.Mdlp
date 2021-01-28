using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Service.Extensibility
{
    public class TaskState
    {
        public SqlConnection Connection { get; set; }
        public SqlTransaction Transaction { get; set; }
    }
}
