using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Dapper;
using Dapper.FastCrud;
using Gdp.Mdlp.Data;
using System.Text.RegularExpressions;
using Newtonsoft.Json;


namespace Gdp.Mdlp.Console
{
    static class ImportViadat
    {
        static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ImportViadat));
        const string configFileName = "import_viadat.json";
        static long ReadViadatTransLineId()
        {
            return JsonConvert.DeserializeObject<ImportViadatConfig>(System.IO.File.ReadAllText(configFileName)).lineid;
        }
        static void SaveViadatTransLineId(long lineid)
        {
            System.IO.File.WriteAllText(
                configFileName,
                JsonConvert.SerializeObject(new ImportViadatConfig
                {
                    lineid = lineid
                }));
        }
        public static void Execute(long fromLine , long toLine)
        {
            Log.Info("Import Viadat transactions ...");
            var viadatTransLineId = fromLine > 0 ? fromLine: ReadViadatTransLineId();
            Log.Info($"Starting from #{viadatTransLineId} ...");
            using (var conViadat = new SqlConnection(Properties.Settings.Default.ViadatConnectionString))
            using (var conMdlp = new SqlConnection(Properties.Settings.Default.MdlpConnectionString))
            {
                // open connections
                conViadat.Open();
                conMdlp.Open();
                // process inbpund
                bool done = false;
                while (!done)
                {
                    var trans = conViadat.Query(
                        @"SELECT TOP (100) * 
FROM WMS_NDC.dbo.h2trans h WITH(NOLOCK) 
WHERE h.lineid>@lineid AND h.ctransty IN (90 , 92) ORDER BY h.lineid ASC",
                        new { lineid = viadatTransLineId }).ToList();
                    if (trans.Count == 0)
                        done = true;
                    else
                    {
                        foreach (dynamic tran in trans)
                        {
                            viadatTransLineId = Convert.ToInt64(tran.lineid);
                            if (toLine > 0 && toLine < viadatTransLineId)
                            {
                                done = true;
                                break;
                            }
                            var r = LoadOrder(conViadat, conMdlp, tran, ViadatHelper.StringToDateTime(tran.dtimecre), viadatTransLineId);
                        }
                    }
                    SaveViadatTransLineId(viadatTransLineId);
                }
            }
        }
        static bool LoadOrder(SqlConnection conViadat, SqlConnection conMdlp, dynamic t, DateTime operationDT, long viadatTransLineId)
        {
            bool result = false;
            try
            {
                var ctransty = t.ctransty;
                var corderty = t.corderty;
                var corderid = t.corderid;
                if (ctransty == 90)
                    result = LoadShipping(conViadat, conMdlp, t, operationDT);
                else if (ctransty == 92)
                    result = LoadReceiping(conViadat, conMdlp, t, operationDT);
                if(result)
                    Log.Info($"Order [{corderty}] {corderid} at #{viadatTransLineId} processed");
                else
                    Log.Info($"Order [{corderty}] {corderid} at #{viadatTransLineId} skipped");
            }
            catch (Exception e)
            {
                Log.Error($"Transaction #{viadatTransLineId} failed to process", e);
            }
            return result;
        }
        static string Gtin(string gtin, string sscc, string sgtin)
        {
            if (IsNotEmpty(gtin) && gtin.Length>=14)
                return gtin.Substring(0, 14);
            else if (IsNotEmpty(sscc))
                return Barcode.TryGtinSmart(sscc);
            else if (IsNotEmpty(sgtin))
                return Barcode.TryGtinSmart(sgtin);
            else
                return null;
        }
        static bool IsNotEmpty(dynamic x)
        {
            return x != null && Convert.ToString(x) != string.Empty;
        }
        static dynamic IsNull(dynamic x, dynamic defaultValue)
        {
            return x ?? defaultValue;
        }
        static bool LoadReceiping(SqlConnection conViadat, SqlConnection conMdlp, dynamic tran, DateTime operationDT)
        {
            bool result = false;
            var t = conViadat.Query(
                @"SELECT DISTINCT h.cgln1, h.cgln2, h.ccontract_num, h.cdoc_num, h.cdoc_date, 
h.corderty, h.cprice, h.cvat, h.csgtin, h.csscc, h.cgtin, h.cbatchno, h.cfinanace_source,
h.ccontract_type, h.cturnover_type
FROM WMS_NDC.dbo.h2trans h WITH(NOLOCK) 
WHERE 
    h.corderid = @corderid 
    AND h.corderty = @corderty 
    AND h.ctransty=280
    AND (h.csgtin>'' OR h.csscc>'')",
                new
                {
                    tran.corderid,
                    tran.corderty
                }).ToList();
            if (t.Count > 0)
            {
                conMdlp.Transactioned((conn, transaction) =>
                {
                    var o = t[0];
                    var orderty = Convert.ToString(o.corderty);
                    // order
                    var order = new Receiping
                    {
                        SubjectId = o.cgln2,
                        ShipperId = o.cgln1,
                        Accepted = false,
                        ContractNo = o.ccontract_num,
                        ContractType = Convert.ToByte(o.ccontract_type ?? 1),
                        DocNo = o.cdoc_num,
                        DocDT = ViadatHelper.StringToDateTime(o.cdoc_date),
                        OperationDT = operationDT,
                        ReceiveType = Convert.ToByte(o.cturnover_type ?? (byte)(orderty == "I" ? 1 : 2)), /* Виды операций приемки 1 - поступление 2 - возврат от покупателя*/
                        SourceType = Convert.ToByte(o.cfinanace_source ?? 1) 
                    };
                    if (!conMdlp.Exists(order, transaction))
                    {
                        order.TrackCreate();
                        conMdlp.Insert(order, (x) => x.AttachToTransaction(transaction));
                        // details
                        var detailNo = 0;
                        var details = t
                            .Select(x => new ReceipingDetail
                            {
                                ReceipingId = order.ReceipingId,
                                DetailNo = detailNo++,
                                Cost = IsNull(x.cprice, 0),
                                VatValue = IsNull(x.cvat, 0),
                                Sgtin = IsNotEmpty(x.csgtin) ? Barcode.TrySgtinSmart(x.csgtin) : null,
                                Sscc = IsNotEmpty(x.csscc) ? Barcode.TrySsccSmart(x.csscc) : null,
                                Gtin = Gtin(x.cgtin, x.csscc, x.csgtin),
                                SeriesNumber = x.cbatchno,
                                Accepted = false
                            }).ToList();
                        details.ForEach(detail =>
                        {
                            if (IsNotEmpty(detail.Sscc))
                                conMdlp.RegisterSsccAsync(
                                    new SSCC
                                    {
                                        Sscc = detail.Sscc,
                                        State = 0,
                                        SubjectId = order.SubjectId
                                    }, transaction).Wait();
                            else if(IsNotEmpty(detail.Sgtin))
                                conMdlp.RegisterSgtinAsync(
                                    new SGTIN
                                    {
                                        Sgtin = detail.Sgtin,
                                        State = 0,
                                        SubjectId = order.SubjectId,
                                    }, transaction).Wait();
                            conMdlp.Insert(detail, (x) => x.AttachToTransaction(transaction));
                        });
                        // event
                        var evt = conMdlp.CreateEvent(EventTypeEnum.Receiping, transaction);
                        // EventReceiping / Details
                        conMdlp.Insert(order.ToEvent(evt.EventId), (x) => x.AttachToTransaction(transaction));
                        details.ForEach(detail => conMdlp.Insert(detail.ToEvent(evt.EventId), (x) => x.AttachToTransaction(transaction)));
                        // set pending state
                        evt.SetEventState(EventStateEnum.Pending);
                        conMdlp.Update(evt, (x) => x.AttachToTransaction(transaction));
                        result = true;
                    }
                });
            }
            return result;
        }
        static bool LoadShipping(SqlConnection conViadat, SqlConnection conMdlp, dynamic tran, DateTime operationDT)
        {
            bool result = false;
            var t = conViadat.Query(
                @"SELECT DISTINCT h.cgln1, h.cgln2, h.ccontract_num, h.cdoc_num, h.cdoc_date, 
h.corderty, h.cprice, h.cvat, h.csgtin, h.csscc, h.cgtin, h.cbatchno, h.cfinanace_source,
h.ccontract_type, h.cturnover_type
FROM WMS_NDC.dbo.h2trans h WITH(NOLOCK) 
WHERE 
    h.corderid = @corderid 
    AND h.corderty = @corderty 
    AND h.ctransty=280
    AND (h.csgtin>'' OR h.csscc>'')",
                new
                {
                    tran.corderid,
                    tran.corderty
                }).ToList();
            if (t.Count > 0)
            {
                conMdlp.Transactioned((conn, transaction) =>
                {
                    var o = t[0];
                    var orderty = Convert.ToString(o.corderty);
                    // order
                    var order = new Shipping
                    {
                        SubjectId = o.cgln1,
                        ReceiverId = o.cgln2,
                        Accepted = false,
                        ContractNo = o.ccontract_num,
                        ContractType = Convert.ToByte(o.ccontract_type ?? 1),
                        DocNo = o.cdoc_num,
                        DocDT = ViadatHelper.StringToDateTime(o.cdoc_date),
                        OperationDT = operationDT,
                        TurnoverType = Convert.ToByte(o.cturnover_type ??  Regex.IsMatch(orderty, @"[a-zA-Z][a-zA-Z]R") ? (byte)2 : (byte)1),
                        SourceType = Convert.ToByte(o.cfinanace_source ?? 1)
                    };
                    if (!conMdlp.Exists(order, transaction))
                    {
                        order.TrackCreate();
                        conMdlp.Insert(order, (x) => x.AttachToTransaction(transaction));
                        // details
                        var detailNo = 0;
                        var details = t
                            .Select(x => new ShippingDetail
                            {
                                ShippingId = order.ShippingId,
                                DetailNo = detailNo++,
                                Cost = IsNull(x.cprice, 0),
                                VatValue = IsNull(x.cvat, 0),
                                Sgtin = IsNotEmpty(x.csgtin) ? Barcode.TrySgtinSmart(x.csgtin) : null,
                                Sscc = IsNotEmpty(x.csscc) ? Barcode.TrySsccSmart(x.csscc) : null,
                                Gtin = Gtin(x.cgtin, x.csscc, x.csgtin),
                                SeriesNumber = x.cbatchno,
                                Accepted = false
                            }).ToList();
                        details.ForEach(detail => conMdlp.Insert(detail, (x) => x.AttachToTransaction(transaction)));
                        // event
                        var evt = conMdlp.CreateEvent(EventTypeEnum.Shipping, transaction); 
                        // EventShipping / Details
                        conMdlp.Insert(order.ToEvent(evt.EventId), (x) => x.AttachToTransaction(transaction));
                        details.ForEach(detail => conMdlp.Insert(detail.ToEvent(evt.EventId), (x) => x.AttachToTransaction(transaction)));
                        // set pending state
                        evt.SetEventState(EventStateEnum.Pending);
                        conMdlp.UpdateAsync(evt, (x)=>x.AttachToTransaction(transaction));
                        result = true;
                    }
                });
            }
            return result;
        }
    }
}
