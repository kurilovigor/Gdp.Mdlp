using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Service
{
    interface IMdlpWorkflowTask
    {
        Task ExecuteAsync(IMdlpManager manager);
    }
}
