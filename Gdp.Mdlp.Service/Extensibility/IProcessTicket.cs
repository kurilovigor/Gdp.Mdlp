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
    public interface IProcessTicket
    {
        Task<bool> Execute(TicketTaskState state);
    }
}
