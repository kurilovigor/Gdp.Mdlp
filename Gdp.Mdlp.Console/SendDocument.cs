using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Gdp.Mdlp.Console
{
    public static class SendDocument
    {
        static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(SendDocument));
        public static Connection CreateConnection()
        {
            var config = Gdp.Mdlp.Service.ServiceConfiguration.Load("config.test.json");
            return new Connection(config.MdlpApi, config.Mdlp.Connections.First().AccountSystem);
        }
        public static async Task DownloadDocument(string documentId)
        {
            using (var connection = CreateConnection())
            {
                try
                {
                    Log.Debug("Login ...");
                    await connection.LoginAsync(false);
                    var doc = await connection.DocumentDownloadByIdAsync(documentId);
                    var path = "cache\\documents";
                    if (!System.IO.Directory.Exists(path))
                        System.IO.Directory.CreateDirectory(path);
                    var fileName = System.IO.Path.Combine(path, documentId + ".xml");
                    doc.Save(fileName);
                    Log.Debug($"Document saved to: {fileName}");
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
                finally
                {
                    Log.Debug("Logout ...");
                    await connection.LogoutAsync();
                }
            }
        }
        public static async Task Execute()
        {
            using (var connection = CreateConnection())
            {
                try
                {
                    /*
                    var doc = Documents.Unpack(
                        "00000000005380",
                        "046018089902906716");
                    */
                    //Log.Debug(doc.ToString());

                    Log.Debug("Login ...");
                    await connection.LoginAsync(false);

                    /*
                    Log.Debug("PublicSgtinsByList ...");
                    await connection.PublicSgtinsByListAsync(new string[] 
                    {
                        Barcode.ToSgtin("010383895708076221184397364519591EE0692MabG2rYp5cQKd3lGrmaklvMNj"),
                        Barcode.ToSgtin("0103838989543945215GML1GLACFHCA91EE0692aCM554QzQPLCSkVcH3+dGIXwj"),
                        Barcode.ToSgtin("0103838989543945215GN5AIXCCSJM591EE06925V4O1lCT+BLc1pIc4GXEiHuQd")
                    });
                    */
                    /*
                    Log.Debug("Send document ...");
                    var request_id = RequestId.New();
                    Log.Debug($"request_id: {request_id}");
                    var document_id = await connection.SendDocumentAsync(doc, request_id);
                    Log.Debug($"document_id: {document_id}");
                    */
                    /*
                    var document_id = "4c615547-7ba4-4bd6-87a7-cc7eb4ec41d8";
                    Log.Debug($"DocumentDownloadLink {document_id} ...");
                    var link = await connection.DocumentDownloadLink(document_id);
                    Log.Debug($"Document {document_id} link {link}");

                    Log.Debug($"DocumentDownloadByLink {link} ...");
                    var xmlDocument = await connection.DocumentDownloadByLink(link);
                    Log.Debug(xmlDocument.ToString());

                    Log.Debug($"TicketDownloadLink {document_id} ...");
                    link = await connection.TicketDownloadLink(document_id);
                    Log.Debug($"Ticket {document_id} link {link}");

                    Log.Debug($"TiketDownloadByLink {link} ...");
                    xmlDocument = await connection.TiketDownloadByLink(link);
                    Log.Debug(xmlDocument.ToString());
                    var doc_size = await connection.MaximumDocumentSizeAsync();
                    Log.Debug($"doc_size: {doc_size}");

                    await connection.DocumentCancelAsync(
                        "4c615547-7ba4-4bd6-87a7-cc7eb4ec41d8",
                        "73d509d2-a158-447c-b960-89d638861bc2");
                    */
                    /*
                    connection.TraceApiCalls = true;
                    var r = await connection.AddressAll();
                    connection.TraceApiCalls = false;
                    */
                    /*
                    connection.TraceApiCalls = true;
                    var r = await connection.DocumentsIncomeAsync(
                        Model.DocFilter
                            .Create()
                            .StartDate(DateTime.Now.AddHours(-1))
                            .EndDate(DateTime.Now)
                            ,0, 10);
                    Cache.Save("DocumentsIncome.json", r);
                    connection.TraceApiCalls = false;
                    */
                    /*
                    connection.TraceApiCalls = true;
                    var r = await connection.SgtinReestrAsync(
                        Model.SgtinReestrFilter
                            .Create()
                            .Gtin(Barcode.ToGtin("010383895708076221184397364519591EE0692MabG2rYp5cQKd3lGrmaklvMNj"))
                        , 1, 10);
                    Cache.Save("SgtinReestrAsync.json", r);
                    connection.TraceApiCalls = false;
                    */
                    /*
                    connection.TraceApiCalls = true;
                    var r = await connection.BranchReestr(
                        new Model.BranchFilter
                        {
                            branch_id = "00000000005380"
                        }
                        , 1, 10);
                    Cache.Save("BranchReestr.json", r);
                    connection.TraceApiCalls = false;
                    */
                    /*
                    connection.TraceApiCalls = true;
                    var r = await connection.PartnersReestr(
                        new Model.PartnersFilter
                        {
                            reg_entity_type = 1
                        }
                        , 1, 10);
                    Cache.Save("PartnersReestr.json", r);
                    connection.TraceApiCalls = false;
                    */
                    connection.TraceApiCalls = true;
                    var r = await connection.SsccHierarchyAsync(Barcode.ToSscc("00042604433509509420"));
                    Cache.Save("SsccHierarchy.json", r);
                    connection.TraceApiCalls = false;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
                finally
                {
                    Log.Debug("Logout ...");
                    await connection.LogoutAsync();
                }
            }
        }
    }
}
