using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Utils
{
    public static class Functional
    {
        public static IEnumerable<T> Chain<T>(Func<IEnumerable<T>> func)
        {
            while (true)
            {
                int i = 0;
                foreach (var element in func())
                {
                    yield return element;
                    i++;
                }
                if (i == 0)
                    break;
            }
        }
    }
}
