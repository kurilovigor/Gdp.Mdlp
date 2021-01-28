using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Model
{
    public class Payment
    {
        public DateTime? created_date { get; set; }
        public DateTime? payment_date { get; set; }
        public int tariff { get; set; }
    }
}
