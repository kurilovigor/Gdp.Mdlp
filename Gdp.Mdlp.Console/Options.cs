using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace Gdp.Mdlp.Console
{
    class Options
    {
        [Option("query_kiz_info", HelpText = "Import <query_kiz_info> events")]
        public string ImportQueryKizFileName { get; set; }
        [Option("import_tickets_folder", HelpText = "Import tickets folder")]
        public string ImportTicketsFolder { get; set; }
        [Option("import_ticket_file", HelpText = "Import ticket file")]
        public string ImportTicketFile { get; set; }
        [Option("import_viadat", HelpText = "Import from Viadat", Default =false)]
        public bool ImportViadat { get; set; }
        [Option("download", HelpText = "Download document")]
        public string DownloadDocumentId { get; set; }
        [Option("test", HelpText = "Run tests", Default = false)]
        public bool Test { get; set; }
        [Option("import_viadat_from", HelpText = "Import Viadat from transaction number", Default = 0)]
        public long ImportViadatFrom { get; set; }
        [Option("import_viadat_to", HelpText = "Import Viadat to transaction number", Default = 0)]
        public long ImportViadatTo { get; set; }
    }
}
