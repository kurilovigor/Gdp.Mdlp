using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Gdp.Mdlp.Data;
using Dapper;
using Dapper.FastCrud;
using System.Globalization;
using Gdp.Mdlp.Service.Extensibility;

namespace Gdp.Mdlp.Service.BaseExtensions.ProcessTicket
{
    public class KizInfo : IProcessTicket
    {
        public async Task<bool> Execute(TicketTaskState state)
        {
            var xDocuments = state.XmlDocument.Element("documents");
            var xKizInfo = xDocuments.Element("kiz_info");
            var xResult = xKizInfo.Element("result");
            var xSgtin = xKizInfo.Elements("sgtin").FirstOrDefault();
            var xInfoSgtin = xSgtin?.Elements("info_sgtin").FirstOrDefault();
            var xSsccUp = xKizInfo.Elements("sscc_up").FirstOrDefault();
            var xSsccUpInfo = xSsccUp?.Elements("info").FirstOrDefault();
            var expirationDate = xInfoSgtin?.Elements("expiration_date").FirstOrDefault()?.Value ?? string.Empty;
            var xSsccDown = xKizInfo.Elements("sscc_down").FirstOrDefault();

            var ticket = new Ticket
            {
                DocumentId = state.Request.DocumentId,
                RequestId = state.Request.RequestId,
                EventId = state.Request.EventId,
                ActionId = xKizInfo.Attribute("action_id").Value
            };
            await state.Connection.InsertAsync(ticket, x => x.AttachToTransaction(state.Transaction));
            var ticketKizInfo = new TicketKizInfo
            {
                DocumentId = state.Request.DocumentId,
                RequestId = state.Request.RequestId,
                EventId = state.Request.EventId,
                Found = Convert.ToBoolean(xResult?.Elements("found").FirstOrDefault()?.Value),
                Status = xInfoSgtin?.Elements("status").FirstOrDefault()?.Value,
                Gtin = xInfoSgtin?.Elements("gtin").FirstOrDefault()?.Value,
                Sgtin = xResult?.Elements("sgtin").FirstOrDefault()?.Value,
                SeriesNumber = xInfoSgtin?.Elements("series_number").FirstOrDefault()?.Value,
                ExpirationDate = Format.ParseDate(expirationDate),
                Sscc = (xInfoSgtin?.Element("sscc")?.Value) ?? (xSsccUpInfo?.Element("sscc")?.Value),
                SsccUp = xResult?.Element("sscc")?.Value,
                Level = xSsccUpInfo?.Element("level") == null ? (int?)null : Convert.ToInt32(xSsccUpInfo?.Element("level").Value)
            };
            if (ticketKizInfo.Sscc != null)
            {
                var sscc = await state.Connection.RegisterSsccAsync(
                    new SSCC
                    {
                        Sscc = ticketKizInfo.Sscc,
                        IsUnpacked = false,
                        IsUnpackRequested = false,
                        SubjectId = state.Request.SubjectId,
                        State = 0
                    },
                    state.Transaction);
            }
            await state.Connection.InsertAsync(ticketKizInfo, x => x.AttachToTransaction(state.Transaction));
            // sgtin
            byte sgtinState = 0;
            if (ticketKizInfo.Status != null && ticketKizInfo.Status != string.Empty)
                sgtinState = state.Connection.RegisterSgtinState(ticketKizInfo.Status, state.Transaction);
            if (ticketKizInfo.Sgtin != null)
            {
                var sgtin = await state.Connection.RegisterSgtinAsync(
                    new SGTIN
                    {
                        Sgtin = ticketKizInfo.Sgtin,
                        State = sgtinState,
                        SubjectId = state.Request.SubjectId,
                        SeriesNumber = ticketKizInfo.SeriesNumber,
                        Gtin = ticketKizInfo.Gtin,
                        ExpirationDate = ticketKizInfo.ExpirationDate,
                        ParentSscc = ticketKizInfo.Sscc
                    }, state.Transaction);
                if (sgtin.ParentSscc != ticketKizInfo.Sscc)
                {
                    sgtin.ParentSscc = ticketKizInfo.Sscc;
                    sgtin.State = sgtinState;
                    sgtin.SeriesNumber = ticketKizInfo.SeriesNumber;
                    sgtin.Gtin = ticketKizInfo.Gtin;
                    sgtin.ExpirationDate = ticketKizInfo.ExpirationDate;
                    sgtin.TrackUpdate();
                    await state.Connection.UpdateAsync(sgtin, x=>x.AttachToTransaction(state.Transaction));
                }
            }
            // tree
            var xTrees = xSsccDown?.Elements("tree");
            if (xTrees != null)
            {
                var nodeNo = 0;
                foreach (var xTree in xTrees)
                {
                    var xTreeSscc = xTree.Element("sscc");
                    var xTreeSgtin = xTree.Element("sgtin");
                    var xTreeSgtinInfo = xTreeSgtin?.Element("info_sgtin");
                    var xTreeParentSscc = xTree.Element("parent_sscc");
                    var ticketTreeInfo = new TicketKizInfoTreeInfo
                    {
                        RequestId = state.Request.RequestId,
                        EventId = state.Request.EventId,
                        NodeNo = nodeNo++,
                        Sscc = xTreeSscc?.Value,
                        Sgtin = xTreeSgtinInfo?.Element("sgtin").Value,
                        Gtin = xTreeSgtinInfo?.Element("gtin").Value,
                        Status = xTreeSgtinInfo?.Element("status").Value,
                        SeriesNumber = xTreeSgtinInfo?.Element("series_number").Value,
                        ExpirationDate = Format.ParseDate(xTreeSgtinInfo?.Element("expiration_date").Value),
                        ParentSscc = xTreeParentSscc?.Value
                    };
                    var parentSscc = ticketTreeInfo.ParentSscc ?? ticketKizInfo.SsccDown;
                    if (parentSscc != null)
                    {
                        var sscc = await state.Connection.RegisterSsccAsync(
                            new SSCC
                            {
                                Sscc = parentSscc,
                                IsUnpacked = false,
                                IsUnpackRequested = false,
                                SubjectId = state.Request.SubjectId,
                                ParentSscc = ticketKizInfo.SsccDown,
                                State = 0
                            },
                            state.Transaction);
                        if (sscc.ParentSscc != ticketKizInfo.SsccDown)
                        {
                            sscc.ParentSscc = ticketKizInfo.SsccDown;
                            sscc.IsUnpacked = false;
                            sscc.IsUnpackRequested = false;
                            sscc.TrackUpdate();
                            await state.Connection.UpdateAsync(sscc, x => x.AttachToTransaction(state.Transaction));
                        }
                    }
                    if (ticketTreeInfo.Sscc != null)
                    {
                        var sscc = await state.Connection.RegisterSsccAsync(
                            new SSCC
                            {
                                Sscc = ticketTreeInfo.Sscc,
                                IsUnpacked = false,
                                IsUnpackRequested = false,
                                SubjectId = state.Request.SubjectId,
                                ParentSscc = parentSscc,
                                State = 0
                            },
                            state.Transaction);
                        if (sscc.ParentSscc != parentSscc)
                        {
                            sscc.ParentSscc = parentSscc;
                            sscc.IsUnpacked = false;
                            sscc.IsUnpackRequested = false;
                            sscc.TrackUpdate();
                            await state.Connection.UpdateAsync(sscc, x => x.AttachToTransaction(state.Transaction));
                        }
                    }
                    if (ticketTreeInfo.Sgtin != null)
                    {
                        if (ticketTreeInfo.Status != null && ticketTreeInfo.Status != string.Empty)
                            sgtinState = state.Connection.RegisterSgtinState(ticketTreeInfo.Status, state.Transaction);
                        else
                            sgtinState = 0;
                        var sgtin = await state.Connection.RegisterSgtinAsync(
                            new SGTIN
                            {
                                Sgtin = ticketTreeInfo.Sgtin,
                                State = sgtinState,
                                SubjectId = state.Request.SubjectId,
                                SeriesNumber = ticketTreeInfo.SeriesNumber,
                                Gtin = ticketTreeInfo.Gtin,
                                ExpirationDate = ticketTreeInfo.ExpirationDate,
                                ParentSscc = parentSscc
                            }, state.Transaction);
                        if (sgtin.ParentSscc != parentSscc)
                        {
                            sgtin.ParentSscc = parentSscc;
                            sgtin.State = sgtinState;
                            sgtin.SeriesNumber = ticketTreeInfo.SeriesNumber;
                            sgtin.Gtin = ticketTreeInfo.Gtin;
                            sgtin.ExpirationDate = ticketTreeInfo.ExpirationDate;
                            sgtin.TrackUpdate();
                            await state.Connection.UpdateAsync(sgtin, x => x.AttachToTransaction(state.Transaction));
                        }
                    }
                    await state.Connection.InsertAsync(ticketTreeInfo, x=>x.AttachToTransaction(state.Transaction));
                }
            }

            return true;
        }
    }
}
