using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Gdp.Mdlp.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       if (o.ImportTicketFile != null && o.ImportTicketFile != string.Empty)
                           ImportTickets.ImportTicketFile(o.ImportTicketFile);
                       if (o.ImportTicketsFolder != null && o.ImportTicketsFolder != string.Empty)
                           ImportTickets.ImportTicketsFolder(o.ImportTicketsFolder);
                       if (o.ImportViadat)
                           ImportViadat.Execute(o.ImportViadatFrom, o.ImportViadatTo);
                       if (o.DownloadDocumentId != null && o.DownloadDocumentId!= string.Empty)
                           SendDocument.DownloadDocument(o.DownloadDocumentId).Wait();
                       if (o.Test)
                           Tests.Execute().Wait();
                       /*
                       if (o.ImportQueryKizFileName != null && o.ImportQueryKizFileName != string.Empty)
                           SimpleService.ExecuteImport(o.ImportQueryKizFileName).Wait();
                       if (o.Events || o.Tickets)
                           SimpleService.ExecuteMdlp(o.Events , o.Tickets).Wait();
                           */
                   });
        }
    }
}
