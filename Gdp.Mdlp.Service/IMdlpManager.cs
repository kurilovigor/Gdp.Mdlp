using Gdp.Mdlp.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Service
{
    interface IMdlpManager
    {
        AccountSystemConfiguration AccountSystem { get; }
        Dictionary<string, ClientAddress> ClientAddresses { get; }
        Service Service { get; }
        Connection MdlpConnection { get; }
        SqlConnection DbConnection { get; }
        CancellationToken CancelationToken { get; }
        Task TryActionAsync(Func<Task> onExecute, Func<Exception, Task> onError = null, string contextInfo = "");
        void TryAction(Action onExecute, Action<Exception> onError = null, string contextInfo = "");
        Task ForEachAsync<T>(IEnumerable<T> elements, Func<T, Task> task);
    }
}
