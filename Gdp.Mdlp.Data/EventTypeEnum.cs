using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp.Data
{
    public enum EventTypeEnum
    {
        Unpack = 1,
        QueryKizInfo = 2,
        Shipping = 3,
        Receiping = 4,
        TicketOperation = 5,
        AcceptNotification = 6,
        QueryHierarchyInfo = 7,
        Posting = 8,
        Accept = 9,
        RefusalSender = 10,
        IncomeDocument = 11
    }
}
