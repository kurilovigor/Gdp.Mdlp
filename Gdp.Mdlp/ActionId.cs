using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gdp.Mdlp
{
    public static class ActionId
    {
        public const string Result = "200";
        public const string QueryKizInfo = "210";
        public const string KizInfo = "211";
        public const string QueryHierarchyInfo = "220";
        public const string HierarchyInfo = "221";
        public const string RefusalSender = "251";
        public const string MoveOrder = "415";
        public const string ReceiveOrder = "416";
        public const string MoveOrderNotification = "601";
        public const string ReceiveOrderNotification = "602";
        public const string RefusalSenderNotification = "605";
        public const string RefusalReceiverNotification = "606";
        public const string AcceptNotification = "607";
        public const string ReceiveOrderErrorsNotification = "617";
        public const string Accept = "701";
        public const string Posting = "702";
        public const string Unpack = "912";
    }
}
