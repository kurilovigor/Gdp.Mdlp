using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp
{
    public class PagedResponse<T> 
    {
        public long Count { get; set; }
        public T[] Records { get; set; }
    }
}
