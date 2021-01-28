using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Service.Extensibility
{
    public interface IProcessIncome
    {
        Task<bool> Execute(IncomeTaskState state);
    }
}
