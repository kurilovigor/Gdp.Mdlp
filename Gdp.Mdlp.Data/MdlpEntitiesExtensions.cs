using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using Dapper.FastCrud;

namespace Gdp.Mdlp.Data
{
    public static class MdlpEntitiesExtensions
    {
        static Dictionary<string, SGTINState> SgtinStates = null;
        static Dictionary<string, SSCCState> SsccStates = null;
        static log4net.ILog Log { get; set; } = log4net.LogManager.GetLogger(typeof(MdlpEntitiesExtensions));
        class CreateEntityStub
        {
            public DateTime CreateDT { get; set; }
            public DateTime? UpdateDT { get; set; }
            public string CreateUser { get; set; }
            public string UpdateUser { get; set; }

        }
        class UpdateEntityStub
        {
            public DateTime? UpdateDT { get; set; }
            public string UpdateUser { get; set; }

        }
        static IMapper _Mapper;
        static IMapper Mapper
        {
            get
            {
                if (_Mapper == null)
                    lock (typeof(MdlpEntitiesExtensions))
                        if (_Mapper == null)
                            CreateMapper();
                return _Mapper;
            }
        }
        static void CreateMapper()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<CreateEntityStub, ClientAddress>();
                cfg.CreateMap<UpdateEntityStub, ClientAddress>();
                cfg.CreateMap<UpdateEntityStub, SGTIN>();
                cfg.CreateMap<CreateEntityStub, SGTIN>();
                cfg.CreateMap<UpdateEntityStub, SSCC>();
                cfg.CreateMap<CreateEntityStub, SSCC>();
                cfg.CreateMap<UpdateEntityStub, Event>();
                cfg.CreateMap<CreateEntityStub, Event>();
                cfg.CreateMap<CreateEntityStub, Lock>();
                cfg.CreateMap<UpdateEntityStub, Request>();
                cfg.CreateMap<CreateEntityStub, Request>();
                cfg.CreateMap<UpdateEntityStub, Receiping>();
                cfg.CreateMap<CreateEntityStub, Receiping>();
                cfg.CreateMap<UpdateEntityStub, Shipping>();
                cfg.CreateMap<CreateEntityStub, Shipping>();
                cfg.CreateMap<UpdateEntityStub, IncomeDocument>();
                cfg.CreateMap<CreateEntityStub, IncomeDocument>();
                cfg.CreateMap<UpdateEntityStub, OutcomeDocument>();
                cfg.CreateMap<CreateEntityStub, OutcomeDocument>();
                cfg.CreateMap<Receiping, EventReceiping>();
                cfg.CreateMap<EventReceiping, Receiping>();
                cfg.CreateMap<ReceipingDetail, EventReceipingDetail>();
                cfg.CreateMap<EventReceipingDetail, ReceipingDetail>();
                cfg.CreateMap<Shipping, EventShipping>();
                cfg.CreateMap<EventShipping, Shipping>();
                cfg.CreateMap<ShippingDetail, EventShippingDetail>();
                cfg.CreateMap<EventShippingDetail, ShippingDetail>();
            });
            _Mapper = config.CreateMapper();
        }
        static CreateEntityStub GetCreateEntityStub()
        {
            var s = new CreateEntityStub
            {
                CreateDT = DateTime.Now,
                CreateUser = Properties.Settings.Default.ServiceName
            };
            s.UpdateDT = s.CreateDT;
            s.UpdateUser = s.CreateUser;
            return s;
        }
        static UpdateEntityStub GetUpdateEntityStub()
        {
            var s = new UpdateEntityStub
            {
                UpdateDT = DateTime.Now,
                UpdateUser = Properties.Settings.Default.ServiceName
            };
            return s;
        }
        public static void TrackUpdate(this ClientAddress entity)
        {
            Mapper.Map(GetUpdateEntityStub(), entity);
        }
        public static void TrackCreate(this ClientAddress entity)
        {
            Mapper.Map(GetCreateEntityStub(), entity);
        }

        public static void TrackUpdate(this SGTIN entity)
        {
            Mapper.Map(GetUpdateEntityStub(), entity);
        }
        public static void TrackCreate(this SGTIN entity)
        {
            Mapper.Map(GetCreateEntityStub(), entity);
        }
        public static void TrackUpdate(this SSCC entity)
        {
            Mapper.Map(GetUpdateEntityStub(), entity);
        }
        public static void TrackCreate(this SSCC entity)
        {
            Mapper.Map(GetCreateEntityStub(), entity);
        }
        public static void TrackUpdate(this Event entity)
        {
            Mapper.Map(GetUpdateEntityStub(), entity);
        }
        public static void TrackCreate(this Event entity)
        {
            Mapper.Map(GetCreateEntityStub(), entity);
        }
        public static void TrackCreate(this Lock entity)
        {
            Mapper.Map(GetCreateEntityStub(), entity);
        }
        public static void TrackUpdate(this Request entity)
        {
            Mapper.Map(GetUpdateEntityStub(), entity);
        }
        public static void TrackCreate(this Request entity)
        {
            Mapper.Map(GetCreateEntityStub(), entity);
        }
        public static void TrackUpdate(this Receiping entity)
        {
            Mapper.Map(GetUpdateEntityStub(), entity);
        }
        public static void TrackCreate(this Receiping entity)
        {
            Mapper.Map(GetCreateEntityStub(), entity);
        }
        public static void TrackUpdate(this Shipping entity)
        {
            Mapper.Map(GetUpdateEntityStub(), entity);
        }
        public static void TrackCreate(this Shipping entity)
        {
            Mapper.Map(GetCreateEntityStub(), entity);
        }
        public static void TrackUpdate(this OutcomeDocument entity)
        {
            Mapper.Map(GetUpdateEntityStub(), entity);
        }
        public static void TrackCreate(this OutcomeDocument entity)
        {
            Mapper.Map(GetCreateEntityStub(), entity);
        }
        public static void SetEventState(this Event entity, EventStateEnum eventState)
        {
            entity.EventStateId = (byte)eventState;
            TrackUpdate(entity);
        }
        public static void SetRequestState(this Request entity, RequestStateEnum requestState)
        {
            entity.RequestStateId = (byte)requestState;
            TrackUpdate(entity);
        }
        public static bool IsRequestState(this Request entity, RequestStateEnum requestState)
        {
            return entity.RequestStateId == (byte)requestState;
        }
        public static bool IsEventType(this Event entity, EventTypeEnum et)
        {
            return entity.EventTypeId == (byte)et;
        }
        public static Request CreateRequest(this Event entity, string subjectId, string actionId)
        {
            return CreateRequest(entity, subjectId, 0, actionId);
        }
        public static Request CreateRequest(this Event entity, string subjectId, byte sequenceId, string actionId)
        {
            Request r = new Request();
            r.EventId = entity.EventId;
            r.RequestId = Guid.NewGuid().ToString("D");
            r.SubjectId = subjectId;
            r.SequenceId = sequenceId;
            r.ActionId = actionId;
            r.Priority = entity.Priority;
            r.SetRequestState(RequestStateEnum.Created);
            r.TrackCreate();
            return r;
        }
        public static IEnumerable<ClientAddress> GetByClientId(this SqlConnection connection, string clientId)
        {
            return connection.Query<ClientAddress>(
                @"SELECT * FROM ClientAddresses ca WHERE ca.ClientId=@ClientId",
                new
                {
                    ClientId = clientId
                });
        }
        public static async Task<Request> GetRequestByEventIdAsync(this SqlConnection connection, long eventId, byte sequenceId)
        {
            return await connection.QueryFirstOrDefaultAsync<Request>(
                @"SELECT * FROM Requests r WHERE r.EventId=@EventId AND r.SequenceId=@SequenceId", 
                new
                {
                    EventId = eventId,
                    SequenceId = sequenceId
                });
        }
        public static IEnumerable<Event> TakeEvents(this SqlConnection connection, EventStateEnum eventState, int maxRecords, int minPriority)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT");
            if (maxRecords >= 0)
            {
                sql
                    .Append(" TOP(")
                    .Append(maxRecords)
                    .Append(")");
            }
            sql.Append(" * FROM [Events] e WHERE e.LockCount=0 AND e.EventStateId=@EventStateId");
            sql
                .Append(" AND e.[Priority]>=")
                .Append(minPriority);
                
            sql.Append(" ORDER BY e.[Priority] DESC, e.UpdateDT ASC");
            return connection.Query<Event>(
                sql.ToString(),
                new
                {
                    EventStateId = (byte)eventState
                });
        }
        public static IEnumerable<Request> TakeRequests(
            this SqlConnection connection, 
            RequestStateEnum[] requestStates,
            IEnumerable<string> addresses,
            int maxRecords,
            int minPriority,
            SqlTransaction transaction = null)
        {
            var statesFilter = string.Join(",", 
                requestStates.Select(x => Convert.ToString((byte)x)));
            var addressesFilter = string.Join(",",
                addresses.Select(x => $"'{x}'"));
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT");
            if (maxRecords >= 0)
            {
                sql
                    .Append(" TOP(")
                    .Append(maxRecords)
                    .Append(")");
            }
            sql.Append(" * FROM Requests r WITH(NOLOCK) WHERE r.RequestStateId IN (");
            sql.Append(statesFilter);
            sql.Append(") AND r.SubjectId IN (");
            sql.Append(addressesFilter);
            sql.Append(")");
            sql
                .Append(" AND r.[Priority]>=")
                .Append(minPriority);
            sql.Append(" ORDER BY r.[Priority] DESC, r.UpdateDT ASC");
            //Log.Debug(sql.ToString());
            return connection.Query<Request>(sql.ToString(), null, transaction);
        }
        public static void Transactioned(this SqlConnection connection, Action<SqlConnection, SqlTransaction> action)
        {
            using (var transaction = connection.BeginTransaction())
            {
                action(connection, transaction);
                transaction.Commit();
            }
        }
        public static async Task TransactionedAsync(this SqlConnection connection, Func<SqlConnection, SqlTransaction, Task> func)
        {
            using (var transaction = connection.BeginTransaction())
            {
                await func(connection, transaction);
                transaction.Commit();
            }
        }
        public static bool Exists(this SqlConnection connection, Receiping receiping, SqlTransaction transaction = null)
        {
            return connection.ExecuteScalar<int>(
                @"SELECT COUNT(*) AS cnt FROM Receipings r WHERE r.SubjectId = @SubjectId AND r.ShipperId=@ShipperId AND r.DocNo=@DocNo AND r.DocDT=@DocDT",
                new
                {
                    receiping.SubjectId,
                    receiping.ShipperId,
                    receiping.DocNo,
                    receiping.DocDT
                }, transaction) > 0;
        }
        public static bool Exists(this SqlConnection connection, Shipping shipping, SqlTransaction transaction = null)
        {
            return connection.ExecuteScalar<int>(
                @"SELECT COUNT(*) AS cnt FROM Shippings s WHERE s.SubjectId = @SubjectId AND s.ReceiverId=@ReceiverId AND s.DocNo=@DocNo AND s.DocDT=@DocDT",
                new
                {
                    shipping.SubjectId,
                    shipping.ReceiverId,
                    shipping.DocNo,
                    shipping.DocDT
                }, transaction) > 0;
        }
        public static async Task<bool> ExistsAsync(this SqlConnection connection, Receiping receiping, SqlTransaction transaction = null)
        {
            return await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) AS cnt FROM Receipings r WHERE r.SubjectId = @SubjectId AND r.ShipperId=@ShipperId AND r.DocNo=@DocNo AND r.DocDT=@DocDT",
                new
                {
                    receiping.SubjectId,
                    receiping.ShipperId,
                    receiping.DocNo,
                    receiping.DocDT
                }, transaction) > 0;
        }
        public static async Task<bool> ExistsAsync(this SqlConnection connection, Shipping shipping, SqlTransaction transaction = null)
        {
            return await connection.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) AS cnt FROM Shippings s WHERE s.SubjectId = @SubjectId AND s.ReceiverId=@ReceiverId AND s.DocNo=@DocNo AND s.DocDT=@DocDT",
                new
                {
                    shipping.SubjectId,
                    shipping.ReceiverId,
                    shipping.DocNo,
                    shipping.DocDT
                }, transaction) > 0;
        }
        public static EventReceiping ToEvent(this Receiping receiping, long eventId)
        {
            var evt = Mapper.Map<EventReceiping>(receiping);
            evt.EventId = eventId;
            return evt;
        }
        public static EventReceipingDetail ToEvent(this ReceipingDetail receipingDetail, long eventId)
        {
            var evtDetail = Mapper.Map<EventReceipingDetail>(receipingDetail);
            evtDetail.EventId = eventId;
            return evtDetail;
        }
        public static EventShipping ToEvent(this Shipping shipping, long eventId)
        {
            var evt = Mapper.Map<EventShipping>(shipping);
            evt.EventId = eventId;
            return evt;
        }
        public static EventShippingDetail ToEvent(this ShippingDetail shippingDetail, long eventId)
        {
            var evtDetail = Mapper.Map<EventShippingDetail>(shippingDetail);
            evtDetail.EventId = eventId;
            return evtDetail;
        }
        public static async Task<Event> CreateEventIncomeDocumentAsync(this SqlConnection connection, IncomeDocument document, SqlTransaction transaction = null)
        {
            var evt = await CreateEventAsync(connection, EventTypeEnum.IncomeDocument, transaction);
            var evtIncomeDocument = new EventIncomeDocument
            {
                EventId = evt.EventId,
                IncomeId = document.IncomeId,
                DocType = document.DocType
            };
            await connection.InsertAsync(evtIncomeDocument, (x) => x.AttachToTransaction(transaction));
            return evt;
        }
        public static Event CreateEventIncomeDocument(this SqlConnection connection, IncomeDocument document, SqlTransaction transaction = null)
        {
            var evt = CreateEvent(connection, EventTypeEnum.IncomeDocument, transaction);
            var evtIncomeDocument = new EventIncomeDocument
            {
                EventId = evt.EventId,
                IncomeId = document.IncomeId,
                DocType = document.DocType
            };
            connection.Insert(evtIncomeDocument, (x) => x.AttachToTransaction(transaction));
            return evt;
        }

        public static Event CreateEvent(this SqlConnection connection, EventTypeEnum eventType, SqlTransaction transaction = null)
        {
            var evt = new Event
            {
                EventTypeId = (byte)eventType,
                EventStateId = (byte)EventStateEnum.Created,
                LockCount = 0,
                Priority = 10,
                ProcessFlag0 = 0,
                ProcessFlag1 = 0,
                ProcessFlag2 = 0,
                ProcessFlag3 = 0,
                ProcessFlag4 = 0,
                ProcessFlag5 = 0
            };
            evt.TrackCreate();
            connection.Insert(evt, (x) => x.AttachToTransaction(transaction));
            return evt;
        }
        public static async Task<Event> CreateEventAsync(this SqlConnection connection, EventTypeEnum eventType, SqlTransaction transaction = null)
        {
            var evt = new Event
            {
                EventTypeId = (byte)eventType,
                EventStateId = (byte)EventStateEnum.Created,
                LockCount = 0,
                Priority = 10,
                ProcessFlag0 = 0,
                ProcessFlag1 = 0,
                ProcessFlag2 = 0,
                ProcessFlag3 = 0,
                ProcessFlag4 = 0,
                ProcessFlag5 = 0
            };
            evt.TrackCreate();
            await connection.InsertAsync(evt, (x) => x.AttachToTransaction(transaction));
            return evt;
        }
        public static async Task<SSCC> UpdateSsccSubjectIdAsync
            (
                this SqlConnection connection,
                string sscc,
                string subjectId,
                SqlTransaction transaction = null
            )
        {
            var s = await connection.RegisterSsccAsync(
                new SSCC
                {
                    Sscc = sscc,
                    IsUnpacked = false,
                    IsUnpackRequested = false,
                    SubjectId = subjectId,
                    State = 0
                },
                transaction);
            if (s.SubjectId != subjectId)
            {
                s.SubjectId = subjectId;
                s.TrackUpdate();
                await connection.UpdateAsync(s, x => x.AttachToTransaction(transaction));
            }
            return s;
        }
        public static async Task<SGTIN> UpdateSgtinSubjectIdAsync
            (
                this SqlConnection connection,
                string sgtin,
                string subjectId,
                SqlTransaction transaction = null
            )
        {
            // update SGTINs SubjectId
            var s = await connection.RegisterSgtinAsync(
                new SGTIN
                {
                    Sgtin = sgtin,
                    State = 0,
                    SubjectId = subjectId
                }, transaction);
            if (s.SubjectId != subjectId)
            {
                s.SubjectId = subjectId;
                s.TrackUpdate();
                await connection.UpdateAsync(s, x => x.AttachToTransaction(transaction));
            }
            return s;
        }
        public static byte RegisterSgtinState
        (
            this SqlConnection connection,
            string stateName,
            SqlTransaction transaction = null
        )
        {
            lock (typeof(MdlpEntitiesExtensions))
            {
                if (SgtinStates == null)
                {
                    SgtinStates = connection
                        .Query<SGTINState>(@"SELECT * FROM dbo.SGTINStates", null, transaction)
                        .ToDictionary(x => x.Name);
                }
                if (!SgtinStates.ContainsKey(stateName))
                {
                    var s = new SGTINState
                    {
                        Name = stateName,
                        State = Convert.ToByte(SgtinStates.Max(x => x.Value.State) + 1)
                    };
                    connection.Insert(s, (e) => { e.AttachToTransaction(transaction); });
                    SgtinStates.Add(s.Name, s);
                }
            }
            return SgtinStates[stateName].State;
        }
        public static byte RegisterSsccState
            (
                this SqlConnection connection,
                string stateName,
                SqlTransaction transaction = null
            )
        {
            lock (typeof(MdlpEntitiesExtensions))
            {
                if (SsccStates == null)
                {
                    SsccStates = connection
                        .Query<SSCCState>(@"SELECT * FROM dbo.SSCCStates
", null, transaction)
                        .ToDictionary(x => x.Name);
                }
                if (!SsccStates.ContainsKey(stateName))
                {
                    var s = new SSCCState
                    {
                        Name = stateName,
                        State = Convert.ToByte(SsccStates.Max(x => x.Value.State) + 1)
                    };
                    connection.Insert(s, (e) => { e.AttachToTransaction(transaction); });
                    SsccStates.Add(s.Name, s);
                }
            }
            return SsccStates[stateName].State;
        }

        public static async Task<SSCC> RegisterSsccAsync
            (
                this SqlConnection connection,
                SSCC sscc,
                SqlTransaction transaction = null
            )
        {
            var entity = await connection.QueryFirstOrDefaultAsync<SSCC>(
                @"SELECT TOP (1) * FROM SSCCs s WHERE s.Sscc = @Sscc",
                new
                {
                    sscc.Sscc
                }, transaction);
            if (entity == null)
            {
                sscc.TrackCreate();
                await connection.InsertAsync(sscc, (e) => e.AttachToTransaction(transaction));
                return sscc;
            }
            else
                return entity;
        }
        public static async Task<SGTIN> RegisterSgtinAsync
            (
                this SqlConnection connection,
                SGTIN sgtin,
                SqlTransaction transaction = null
            )
        {
            var entity = await connection.QueryFirstOrDefaultAsync<SGTIN> (
                @"SELECT TOP (1) * FROM SGTINs s WHERE s.Sgtin=@sgtin",
                new
                {
                    sgtin.Sgtin
                },
                transaction);
            if (entity == null)
            {
                sgtin.TrackCreate();
                await connection.InsertAsync(sgtin, (e) => e.AttachToTransaction(transaction));
                return sgtin;
            }
            else
                return entity;
        }
        public static Param GetParam(this SqlConnection connection, string id, SqlTransaction transaction = null)
        {
            return connection.Get(new Param { Id = id }, (x) => x.AttachToTransaction(transaction));
        }
        public static async Task<Param> GetParamAsync(this SqlConnection connection, string id, SqlTransaction transaction = null)
        {
            return await connection.GetAsync(new Param { Id = id }, (x) => x.AttachToTransaction(transaction));
        }
        public static void SetParam(this SqlConnection connection, Param param, SqlTransaction transaction = null)
        {
            var p = connection.Get(param, (x) => x.AttachToTransaction(transaction));
            if (p == null)
                connection.Insert(param, (x) => x.AttachToTransaction(transaction));
            else
                connection.Update(param, (x) => x.AttachToTransaction(transaction));
        }
        public static async Task SetParamAsync(this SqlConnection connection, Param param, SqlTransaction transaction = null)
        {
            var p = await connection.GetAsync(param, (x) => x.AttachToTransaction(transaction));
            if (p == null)
                await connection.InsertAsync(param, (x) => x.AttachToTransaction(transaction));
            else
                await connection.UpdateAsync(param, (x) => x.AttachToTransaction(transaction));
        }
        public static string GetStringParam(this SqlConnection connection, string id, SqlTransaction transaction = null)
        {
            return GetParam(connection, id, transaction)?.Value;
        }
        public static async Task<string> GetStringParamAsync(this SqlConnection connection, string id, SqlTransaction transaction = null)
        {
            return (await GetParamAsync(connection, id, transaction))?.Value;
        }
        public static long? GetLongParam(this SqlConnection connection, string id, SqlTransaction transaction = null)
        {
            var s = GetParam(connection, id, transaction)?.Value;
            if (s == null || s == string.Empty)
                return null;
            else
                return Convert.ToInt64(s);
        }
        public static async Task<long?> GetLongParamAsync(this SqlConnection connection, string id, SqlTransaction transaction = null)
        {
            var s = (await GetParamAsync(connection, id, transaction))?.Value;
            if (s == null || s == string.Empty)
                return null;
            else
                return Convert.ToInt64(s);
        }
        public static DateTime? GetDateTimeParam(this SqlConnection connection, string id, SqlTransaction transaction = null)
        {
            var s = GetParam(connection, id, transaction)?.Value;
            if (s == null || s == string.Empty)
                return null;
            else
                return Convert.ToDateTime(s);
        }
        public static async Task<DateTime?> GetDateTimeParamAsync(this SqlConnection connection, string id, SqlTransaction transaction = null)
        {
            var s = (await GetParamAsync(connection, id, transaction))?.Value;
            if (s == null || s == string.Empty)
                return null;
            else
                return Convert.ToDateTime(s);
        }
        public static void SetStringParam(this SqlConnection connection, string id, string value, SqlTransaction transaction = null)
        {
            var param = new Param
            {
                Id = id,
                Value = value
            };
            var p = connection.Get(param, (x) => x.AttachToTransaction(transaction));
            if (p == null)
                connection.Insert(param, (x) => x.AttachToTransaction(transaction));
            else
                connection.Update(param, (x) => x.AttachToTransaction(transaction));
        }
        public static async Task SetStringParamAsync(this SqlConnection connection, string id, string value, SqlTransaction transaction = null)
        {
            var param = new Param
            {
                Id = id,
                Value = value
            };
            var p = await connection.GetAsync(param, (x) => x.AttachToTransaction(transaction));
            if (p == null)
                await connection.InsertAsync(param, (x) => x.AttachToTransaction(transaction));
            else
                await connection.UpdateAsync(param, (x) => x.AttachToTransaction(transaction));
        }
        public static void SetLongParam(this SqlConnection connection, string id, long? value, SqlTransaction transaction = null)
        {
            SetStringParam(connection, id, value?.ToString(), transaction);
        }
        public static async Task SetLongParamAsync(this SqlConnection connection, string id, long? value, SqlTransaction transaction = null)
        {
            await SetStringParamAsync(connection, id, value?.ToString(), transaction);
        }
        public static void SetDateTimeParam(this SqlConnection connection, string id, DateTime? value, SqlTransaction transaction = null)
        {
            SetStringParam(connection, id, value?.ToString(), transaction);
        }
        public static async Task SetDateTimeParamAsync(this SqlConnection connection, string id, DateTime? value, SqlTransaction transaction = null)
        {
            await SetStringParamAsync(connection, id, value?.ToString(), transaction);
        }
        public static async Task<List<IncomeDocument>> GetIncomeDocuments(this SqlConnection connection, string clientId, IncomeDocumentStatesEnum state)
        {
            return (await connection.QueryAsync<IncomeDocument>(
                @"SELECT * FROM  IncomeDocuments i WHERE i.ClientId = @ClientId AND i.DocumentStateId = @DocumentStateId",
                new
                {
                    ClientId = clientId,
                    DocumentStateId = (byte)state
                })).ToList();
        }
        public static void TrackUpdate(this IncomeDocument entity)
        {
            Mapper.Map(GetUpdateEntityStub(), entity);
        }
        public static void TrackCreate(this IncomeDocument entity)
        {
            Mapper.Map(GetCreateEntityStub(), entity);
        }
        public static void SetDocumentState(this IncomeDocument income, IncomeDocumentStatesEnum state)
        {
            income.DocumentStateId = (byte)state;
            income.TrackUpdate();
        }
        public static void SetDocumentState(this OutcomeDocument outcome, OutcomeDocumentStatesEnum state)
        {
            outcome.DocumentStateId = (byte)state;
            outcome.TrackUpdate();
        }
        public static async Task<List<TaskRegistrationUpdate>> GetTaskRegistrationUpdates(
            this SqlConnection connection,
            string clientId,
            SqlTransaction transaction = null)
        {
            return (await connection.QueryAsync<TaskRegistrationUpdate>(
                @"SELECT * FROM [dbo].[TaskRegistrationUpdates] WHERE ClientId=@ClientId",
                new
                {
                    ClientId = clientId
                }, transaction))
                .ToList();

        }
        public static async Task<bool> RegistrationExists(this SqlConnection connection, string systemSubjId, SqlTransaction transaction = null)
        {
            return (await connection.GetAsync(new Registration { SystemSubjId = systemSubjId }, (x) => x.AttachToTransaction(transaction))) != null;
        }
        public static async Task<bool> BranchExists(this SqlConnection connection, string id, SqlTransaction transaction = null)
        {
            return (await connection.GetAsync(new Branch { Id =id }, (x) => x.AttachToTransaction(transaction))) != null;
        }
        public static async Task<bool> SafeWarehouseExists(this SqlConnection connection, string id, SqlTransaction transaction = null)
        {
            return (await connection.GetAsync(new SafeWarehous { Id = id }, (x) => x.AttachToTransaction(transaction))) != null;
        }
        public static async Task<IncomeDocument> GetIncomeDocumentAsync(this SqlConnection connection, string requestId, string documentId, SqlTransaction transaction)
        {
            return await connection.QueryFirstOrDefaultAsync<IncomeDocument>(
                @"SELECT * FROM IncomeDocuments i WHERE i.DocumentId=@DocumentId AND i.RequestId=@RequestId",
                new
                {
                    DocumentId = documentId,
                    RequestId = requestId
                }, transaction);
        }
        public static async Task<OutcomeDocument> GetOutcomeDocumentAsync(this SqlConnection connection, string requestId, string documentId, SqlTransaction transaction)
        {
            return await connection.QueryFirstOrDefaultAsync<OutcomeDocument>(
                @"SELECT * FROM OutcomeDocuments i WHERE i.DocumentId=@DocumentId AND i.RequestId=@RequestId",
                new
                {
                    DocumentId = documentId,
                    RequestId = requestId
                }, transaction);
        }
        public static async Task CreateTaskRegistrationUpdateIfNeeded(
            this SqlConnection connection, 
            string systemSubjId, 
            string subjId,
            string clientId,
            SqlTransaction transaction)
        {
            bool needRefreshSender = false;
            var sender = await connection.GetAsync(new Registration { SystemSubjId = systemSubjId }, x => x.AttachToTransaction(transaction));
            if (sender == null)
                needRefreshSender = true;
            else
            {
                var branch = await connection.GetAsync(new Branch { Id = subjId }, x => x.AttachToTransaction(transaction));
                if (branch == null)
                {
                    var whs = await connection.GetAsync(new SafeWarehous { Id = subjId }, x => x.AttachToTransaction(transaction));
                    if (whs == null)
                        needRefreshSender = true;
                }
            }
            if (needRefreshSender)
            {
                var taskRegUpdate = await connection.QueryFirstOrDefaultAsync<TaskRegistrationUpdate>(
                    @"SELECT * FROM TaskRegistrationUpdates t WHERE t.SystemSubjId=@SystemSubjId",
                    new
                    {
                        SystemSubjId = systemSubjId
                    },
                    transaction);
                if (taskRegUpdate == null)
                {
                    taskRegUpdate = new TaskRegistrationUpdate
                    {
                        ClientId = clientId,
                        SystemSubjId = systemSubjId
                    };
                    await connection.InsertAsync(taskRegUpdate, x => x.AttachToTransaction(transaction));
                }
            }
        }

        public static async Task SetEventAndRequestStates(
            this SqlConnection connection,
            Event evt,
            Request request,
            EventStateEnum eventState,
            RequestStateEnum requestState
            )
        {
            await connection.TransactionedAsync(async (conn, transaction) => {
                request.SetRequestState(requestState);
                await conn.UpdateAsync(request, (x) => x.AttachToTransaction(transaction));
                if (evt != null)
                {
                    evt.SetEventState(eventState);
                    await conn.UpdateAsync(evt, (x) => x.AttachToTransaction(transaction));
                }
            });
        }

        public static async Task SetEventAndRequestStates(
            this SqlConnection connection,
            SqlTransaction transaction,
            Event evt,
            Request request,
            EventStateEnum eventState,
            RequestStateEnum requestState
            )
        {
            request.SetRequestState(requestState);
            await connection.UpdateAsync(request, (x) => x.AttachToTransaction(transaction));
            if (evt != null)
            {
                evt.SetEventState(eventState);
                await connection.UpdateAsync(evt, (x) => x.AttachToTransaction(transaction));
            }
        }
    }
}
