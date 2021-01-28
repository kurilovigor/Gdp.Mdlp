using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using System.Data.SqlClient;
using Dapper;
using Dapper.FastCrud;
using Gdp.Mdlp.Data;
using Gdp.Mdlp.Utils;

namespace Gdp.Mdlp.Console
{
    public static class ImportTickets
    {
        static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ImportTickets));
        public static void ImportTicketFile(string fileName)
        {
            try
            {
                if (!File.Exists(fileName))
                    Log.Error($"File \"{fileName}\" not found");
                else
                {
                    XDocument doc = XDocument.Load(fileName);
                    var documentId = Path.GetFileNameWithoutExtension(fileName);
                    var xNode = doc.Element("documents").Elements().FirstOrDefault();
                    var action_id = xNode.Attribute("action_id")?.Value;
                    if (action_id == "211")
                    {
                        var kizInfo = new Service.BaseExtensions.ProcessTicket.KizInfo();
                        using (var connection = new SqlConnection(Properties.Settings.Default.MdlpConnectionString))
                        {
                            connection.Open();
                            var request = connection.QueryFirstOrDefault<Request>(
                                @"SELECT TOP (1) * FROM Requests r WHERE r.DocumentId=@DocumentId",
                                new
                                {
                                    DocumentId = documentId
                                });
                            if (request == null)
                                Log.Error($"Request for document \"{documentId}\" not found");
                            else
                            {
                                if (request.IsRequestState(RequestStateEnum.TickedDownloaded))
                                    Log.Warn($"Ticket for document \"{documentId}\" already downloaded");
                                else
                                {
                                    //kizInfo.Execute(connection,request,doc).Wait();
                                    connection.TransactionedAsync(async (c,t)=> 
                                    {
                                        await kizInfo.Execute(
                                            new Service.Extensibility.TicketTaskState
                                            {
                                                Connection = c,
                                                Transaction = t,
                                                Request = request,
                                                XmlDocument = doc
                                            });
                                    }).Wait(); 
                                    Log.Info($"Ticket for document \"{documentId}\" downloaded");
                                }
                            }
                        }
                    }
                    else
                        Log.Error($"Unknown action_id #{action_id} ({fileName})");
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed file: {fileName}", e);
            }
        }
        public static void ImportTicketsFolder(string folderName)
        {
            try
            {
                if (!Directory.Exists(folderName))
                    Log.Error($"Directory \"{folderName}\" not found");
                else
                {
                    foreach (var fileName in Directory.EnumerateFiles(folderName, "*.xml"))
                        ImportTicketFile(fileName);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}
