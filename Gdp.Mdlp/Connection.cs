using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Web;
using System.IO;
using System.Threading;

namespace Gdp.Mdlp
{
    /// <summary>
    /// Соединение с МДЛП
    /// </summary>
    public class Connection : IDisposable
    {
        Semaphore _HttpRequestSemaphore = new Semaphore(1, 1);
        static log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Connection));
        public bool TraceApiCalls { get; set; } = false;
        protected AccountSystemConfiguration AccountSystem { get; set; }
        protected Uri Api { get; set; }
        protected X509Certificate2 Certificate { get; set; }
        protected Session Session { get; set; }
        public bool SkipApiErrorsInLoops { get; set; } = true;
        public int SkipApiErrorsDelay { get; set; } = 1000;

        #region CryptographyManager
        CryptographyManager _CryptographyManager;
        public CryptographyManager CryptographyManager
        {
            get
            {
                if (_CryptographyManager == null)
                    lock (this)
                        if (_CryptographyManager == null)
                            _CryptographyManager = new CryptographyManager();
                return _CryptographyManager;
            }
        }
        #endregion
        #region Client
        HttpClient _Client;
        protected HttpClient Client
        {
            get
            {
                NeedClient();
                return _Client;
            }
        }
        protected void NeedClient()
        {
            if (_Client == null)
                lock (this)
                    if (_Client == null)
                    {
                        _Client = new HttpClient();
                        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    }
        }
        #endregion
        #region IDisposable
        public void Dispose()
        {
            if (_Client != null)
            {
                _Client.Dispose();
                _Client = null;
            }
            if (_CryptographyManager != null)
            {
                _CryptographyManager.Dispose();
                _CryptographyManager = null;
            }
            if (_HttpRequestSemaphore!=null)
            {
                _HttpRequestSemaphore.Dispose();
                _HttpRequestSemaphore = null;
            }
        }
        #endregion
        public Connection(Uri api, AccountSystemConfiguration accountSystem)
        {
            AccountSystem = accountSystem;
            Api = api;
        }
        #region Login/Logout
        public async Task LoginAsync(bool useSessionCache = false)
        {
            if (useSessionCache)
                LoadSession();
            else
                ResetSession();
            Certificate = CryptographyManager.FindCertificateByTumbprint(AccountSystem.Thumbprint);
            if(Client.BaseAddress != Api)
                Client.BaseAddress = Api;
            if (ValidSession)
            {
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", Session.Token);
            }
            else
            {
                // Аутентификация пользователя
                var authRequest = new Model.AuthRequest
                {
                    client_id = AccountSystem.ClientId,
                    client_secret = AccountSystem.ClientSecret,
                    user_id = AccountSystem.Thumbprint,
                    auth_type = "SIGNED_CODE"
                };
                var authResponse = await PostJsonAsync<Model.AuthResponse, Model.AuthRequest>("auth", authRequest);
                AssertApiErrorIsNotEmpty(
                    "Ошибка получения кода авторизации",
                    authResponse.error_code,
                    authResponse.error_description,
                    authResponse.code);
                // Получение ключа сессии 
                var tokenRequest = new Model.TokenRequest
                {
                    code = authResponse.code,
                    signature = CryptographyManager.Encrypt(authResponse.code, Certificate)
                };
                var tokenResponse = await PostJsonAsync<Model.TokenResponse, Model.TokenRequest>("token", tokenRequest);
                AssertApiErrorIsNotEmpty(
                    "Ошибка получения ключа сессии",
                    tokenResponse.error_code,
                    tokenResponse.error_description,
                    tokenResponse.token);
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", tokenResponse.token);
                Session.Token = tokenResponse.token;
                if (tokenResponse.life_time != string.Empty)
                    Session.LifeTime = DateTime.Now.AddMinutes(Convert.ToInt32(tokenResponse.life_time));
                SaveSession();
            }
        }
        public async Task LogoutAsync()
        {
            if (ValidSession)
            {
                await GetAsync("auth/logout");
                ResetSession();
            }
        }
        #endregion
        #region Documents
        public async Task<string> DocumentSendAsync(
            XDocument doc,
            string request_id)
        {
            var msg = Encoding.UTF8.GetBytes(doc.ToString());
            var sendDocumentRequest = new Model.DocumentSendRequest
            {
                document = Convert.ToBase64String(msg),
                sign = CryptographyManager.Encrypt(msg, Certificate),
                request_id = request_id
            };
            var sendDocumentResponse = await PostJsonAsync<Model.DocumentSendResponse, Model.DocumentSendRequest>(
                "documents/send", sendDocumentRequest);
            AssertApiErrorIsNotEmpty(
                "Ошибка отправки документа",
                sendDocumentResponse.error_code,
                sendDocumentResponse.error_description,
                sendDocumentResponse.document_id);
            return sendDocumentResponse.document_id;
        }
        public async Task<string> DocumentDownloadLinkAsync(string document_id)
        {
            var response = await GetJsonAsync<Model.DocumentDownloadLinkResponse>($"documents/download/{document_id}");
            AssertApiErrorIsNotEmpty(
                $"Ошибка получения ссылки для скачивания документа {document_id}", 
                response.error_code,
                response.error_description,
                response.link);
            return response.link;
        }
        public async Task<XDocument> DocumentDownloadByIdAsync(string document_id)
        {
            return await XDocumentDownloadAsync(await DocumentDownloadLinkAsync(document_id));
        }
        public async Task<XDocument> TicketDownloadByIdAsync(string document_id)
        {
            return await XDocumentDownloadAsync(await TicketDownloadLinkAsync(document_id));
        }
        public async Task<XDocument> DocumentDownloadByLinkAsync(string link)
        {
            return await XDocumentDownloadAsync(link);
        }
        public async Task<XDocument> TicketDownloadByLinkAsync(string link)
        {
            return await XDocumentDownloadAsync(link);
        }
        protected async Task<XDocument> XDocumentDownloadAsync(string link)
        {
            var documentText = await GetAsync(link);
            var textReader = new StringReader(documentText);
            return XDocument.Load(textReader, LoadOptions.None);
        }
        public async Task<string> TicketDownloadLinkAsync(string document_id)
        {
            var response = await GetJsonAsync<Model.TicketDownloadLinkResponse>($"documents/{document_id}/ticket");
            AssertApiErrorIsNotEmpty(
                $"Ошибка получения ссылки на квитанцию документа {document_id}",
                response.error_code,
                response.error_description,
                response.link);
            return response.link;
        }
        public async Task<int> MaximumDocumentSizeAsync()
        {
            var response = await GetJsonAsync<Model.MaximumDocumentSizeResponse>("documents/doc_size");
            AssertApiError(
                $"Ошибка получения ограничения на размер документа",
                response.error_code,
                response.error_description,
                response.doc_size.HasValue);
            return response.doc_size.Value;

        }
        public async Task DocumentCancelAsync(string document_id, string request_id)
        {
            await PostJsonNoResultAsync(
                "documents/cancel", 
                new Model.DocumentCancelRequest
                {
                    document_id = document_id,
                    request_id = request_id
                });
        }
        public async Task<Model.Document[]> DocumentsRequestAsync(string request_id)
        {
            var result = await GetJsonAsync<Model.DocumentsRequestResponse>(
                $"documents/request/{request_id}");
            AssertApiError("Ошибка получения списка документов по идентификатору запроса", 
                result.error_code, 
                result.error_description);
            return result.documents;
        }
        public async Task<PagedResponse<Model.OutcomeDocument>> DocumentsOutcomeAsync(Model.DocFilter filter, long start_from, int count)
        {
            var response = await PostJsonAsync<Model.DocumentsOutcomeResponse, Model.DocumentsOutcomeRequest>(
                @"documents/outcome",
                new Model.DocumentsOutcomeRequest
                {
                    filter = filter,
                    start_from = start_from,
                    count = count
                }
                );
            AssertApiError(
                @"Ошибка получения исходящих документов",
                response.error_code,
                response.error_description);
            return new PagedResponse<Model.OutcomeDocument>
            {
                Count = response.total ?? response.documents.Length,
                Records = response.documents
            };
        }
        public async Task<PagedResponse<Model.IncomeDocument>> DocumentsIncomeAsync(Model.DocFilter filter, long start_from, int count)
        {
            var response = await PostJsonAsync<Model.DocumentsIncomeResponse, Model.DocumentsIncomeRequest>(
                @"documents/income",
                new Model.DocumentsIncomeRequest
                {
                    filter = filter,
                    start_from = start_from,
                    count = count
                }
                );
            AssertApiError(
                @"Ошибка получения входящих документов",
                response.error_code,
                response.error_description);
            return new PagedResponse<Model.IncomeDocument>
            {
                Count = response.total ?? response.documents.Length,
                Records = response.documents
            };
        }
        public async Task ForEachDocumentsOutcomeAsync(
            Model.DocFilter filter, 
            Func<Model.OutcomeDocument, Task> process, 
            CancellationToken token)
        {
            await ForEachFilterEntriesApiCall(DocumentsOutcomeAsync, filter, process, token);
        }
        public async Task ForEachDocumentsIncomeAsync(
            Model.DocFilter filter,
            Func<Model.IncomeDocument, Task> process,
            CancellationToken token)
        {
            await ForEachFilterEntriesApiCall(DocumentsIncomeAsync, filter, process, token);
        }
        public async Task ForEachDocumentsOutcomeAsync(
            Model.DocFilter filter,
            Func<Model.OutcomeDocument, Task> process)
        {
            await ForEachFilterEntriesApiCall(DocumentsOutcomeAsync, filter, process);
        }
        public async Task ForEachDocumentsIncomeAsync(
            Model.DocFilter filter,
            Func<Model.IncomeDocument, Task> process)
        {
            await ForEachFilterEntriesApiCall(DocumentsIncomeAsync, filter, process);
        }
        #endregion

        #region Участиники/Адреса/Склады
        public async Task<Model.AddressEntry[]> AddressAllAsync()
        {
            var response = await GetJsonAsync<Model.AddressAllResponse>(@"reestr/address/all");
            AssertApiError(
                @"Ошибка получения информации о всех местах осуществления деятельности участника",
                response.error_code,
                response.error_description);
            return response.entries;
        }
        public async Task<PagedResponse<Model.BranchEntry>> BranchesReestr(Model.BranchFilter filter, long start_from, int count)
        {
            var response =  await FilterEntriesApiCall<Model.BranchEntry, Model.BranchFilter>(
                @"reestr/branches/filter",
                filter,
                start_from,
                count,
                @"Ошибка поиска информации о местах осуществления деятельности по фильтру");
            return new PagedResponse<Model.BranchEntry>
            {
                Count = response.Length,
                Records = response
            };
        }
        public async Task ForEachBranchesReestr(
            Model.BranchFilter filter, 
            Func<Model.BranchEntry, Task> process,
            CancellationToken token)
        {
            await ForEachFilterEntriesApiCall(BranchesReestr, filter, process, token);
        }
        public async Task ForEachBranchesReestr(
            Model.BranchFilter filter,
            Func<Model.BranchEntry, Task> process)
        {
            await ForEachFilterEntriesApiCall(BranchesReestr, filter, process
);
        }
        public async Task<Model.BranchEntry> BranchReestr(string branch_id)
        {
            var response = await GetJsonAsync<Model.BranchReestrResponse>($"/reestr/branches/{branch_id}");
            AssertApiError (
                @"Ошибка получения информации о конкретном месте осуществления деятельности",
                response.error_code,
                response.error_description);
            return response as Model.BranchEntry;
        }
        public async Task<PagedResponse<Model.RegistrationEntry>> PartnersReestr(Model.PartnersFilter filter, long start_from, int count)
        {
            var response = await PostJsonAsync<Model.PartnersReestrResponse, Model.PartnersReestrRequest>(
                @"reestr_partners/filter",
                new Model.PartnersReestrRequest
                {
                    filter = filter,
                    start_from = start_from,
                    count = count

                });
            return new PagedResponse<Model.RegistrationEntry>
            {
                Count = response.filtered_records_count,
                Records = response.filtered_records
            };
        }
        public async Task ForEachPartnersReestr(
            Model.PartnersFilter filter,
            Func<Model.RegistrationEntry, Task> process,
            CancellationToken token)
        {
            await ForEachFilterEntriesApiCall(PartnersReestr, filter, process, token);
        }
        public async Task ForEachPartnersReestr(
            Model.PartnersFilter filter,
            Func<Model.RegistrationEntry, Task> process)
        {
            await ForEachFilterEntriesApiCall(PartnersReestr, filter, process);
        }
        #endregion
        #region SSCC
        public async Task<Model.SsccHierarchyResponse> SsccHierarchyAsync(string sscc)
        {
            var response = await GetJsonAsync<Model.SsccHierarchyResponse>($"reestr/sscc/{sscc}/hierarchy");
            var errorDescription = "Ошибка получения информации об иерархии вложенности третичной упаковки";
            AssertApiError(
                errorDescription,
                response.error_code,
                response.error_desc);
            AssertApiError(
                errorDescription,
                response.error_code,
                response.error_description);
            return response;
        }
        #endregion
        #region SGTIN
        public async Task<PagedResponse<Model.SgtinExtended>> SgtinReestrAsync(Model.SgtinReestrFilter filter, long start_from, int count)
        {
            var response = await PostJsonAsync<Model.SgtinReestrReponse, Model.SgtinReestrRequest>(
                @"reestr/sgtin/filter",
                new Model.SgtinReestrRequest
                {
                    filter = filter,
                    start_from = start_from,
                    count = count
                });
            AssertApiError(
                @"Ошибка поиска по реестру КИЗ",
                response.error_code,
                response.error_description);
            return new PagedResponse<Model.SgtinExtended>
            {
                Count = response.total ?? response.entries.Length,
                Records = response.entries
            };
        }
        public async Task ForEachSgtinReestrAsync(
                Model.SgtinReestrFilter filter,
                Func<Model.SgtinExtended, Task> process,
                CancellationToken token
            )
        {
            await ForEachFilterEntriesApiCall(SgtinReestrAsync, filter, process, token);
        }
        public async Task ForEachSgtinReestrAsync(
                Model.SgtinReestrFilter filter,
                Func<Model.SgtinExtended, Task> process
            )
        {
            await ForEachFilterEntriesApiCall(SgtinReestrAsync, filter, process);
        }
        public async Task<Model.SgtinsByListResponse> SgtinsByListAsync(string[] sgtins)
        {
            if (sgtins == null)
                throw new ArgumentNullException("sgtins");
            if (sgtins.Length < 1 || sgtins.Length > 500)
                throw new ArgumentOutOfRangeException("sgtin", "Количество должно быть в диапазоне 1..500");
            var response = await PostJsonAsync<Model.SgtinsByListResponse, Model.SgtinsByListRequest>(
                @"reestr/sgtin/sgtins-by-list", 
                new Model.SgtinsByListRequest
                { 
                    filter = new Model.SgtinsFilter
                    {
                        sgtins = sgtins
                    }
                });
            return response;
        }
        public async Task<Model.PublicSgtinsByListResponse> PublicSgtinsByListAsync(string[] sgtins)
        {
            if (sgtins == null)
                throw new ArgumentNullException("sgtins");
            if (sgtins.Length < 1 || sgtins.Length > 500)
                throw new ArgumentOutOfRangeException("sgtin", "Количество должно быть в диапазоне 1..500");
            var response = await PostJsonAsync<Model.PublicSgtinsByListResponse, Model.PublicSgtinsByListRequest>(
                @"reestr/sgtin/public/sgtins-by-list",
                new Model.PublicSgtinsByListRequest
                {
                    filter = new Model.SgtinsFilter
                    {
                        sgtins = sgtins
                    }
                });
            return response;
        }
        public async Task ForEachSgtinsByListAsync(
            string[] sgtins, 
            Func<Model.Sgtin, Task> success,
            Func<Model.FailedSgtin, Task> fail,
            CancellationToken token)
        {
            int start = 0;
            int size = 500;
            while (start < sgtins.Length && !token.IsCancellationRequested)
            {
                Model.SgtinsByListResponse response = null;
                try
                {
                    await LoginIfNeededAsync();
                    response = await SgtinsByListAsync(sgtins
                        .Skip(start)
                        .Take(size)
                        .ToArray());
                }
                catch(Exception e)
                {
                    Log.Error(e);
                    if (e is ApiCallException)
                        if ((e as ApiCallException).Trace.StatusCode == HttpStatusCode.Unauthorized)
                            Session.Token = null;
                    if (!SkipApiErrorsInLoops)
                        throw;
                    await Task.Delay(SkipApiErrorsDelay, token);
                    continue;
                }
                if (response.entries != null && success != null)
                    foreach (var entry in response.entries)
                    {
                        if (token.IsCancellationRequested)
                            break;
                        await success(entry);
                    }
                if (response.failed_entries != null && fail != null)
                    foreach (var failed_entry in response.failed_entries)
                    {
                        if (token.IsCancellationRequested)
                            break;
                        await fail(failed_entry);
                    }
                start += size;
            }
        }
        public async Task ForEachPublicSgtinsByListAsync(
            string[] sgtins,
            Func<Model.PublicSgtin, Task> success,
            Func<Model.FailedSgtin, Task> fail,
            CancellationToken token)
        {
            int start = 0;
            int size = 500;
            while (start < sgtins.Length && !token.IsCancellationRequested)
            {
                Model.PublicSgtinsByListResponse response = null;
                try
                {
                    await LoginIfNeededAsync();
                    response = await PublicSgtinsByListAsync(sgtins
                        .Skip(start)
                        .Take(size)
                        .ToArray());
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    if (e is ApiCallException)
                        if ((e as ApiCallException).Trace.StatusCode == HttpStatusCode.Unauthorized)
                            Session.Token = null;
                    if (!SkipApiErrorsInLoops)
                        throw;
                    await Task.Delay(SkipApiErrorsDelay, token);
                    continue;
                }
                if (response.entries != null && success != null)
                    foreach (var entry in response.entries)
                    {
                        if (token.IsCancellationRequested)
                            break;
                        await success(entry);
                    }
                if (response.failed_entries != null && fail != null)
                    foreach (var failed_entry in response.failed_entries)
                    {
                        if (token.IsCancellationRequested)
                            break;
                        await fail(failed_entry);
                    }
                start += size;
            }
        }
        public async Task ForEachSgtinsByListAsync(
            string[] sgtins,
            Func<Model.Sgtin, Task> success,
            Func<Model.FailedSgtin, Task> fail)
        {
            await ForEachSgtinsByListAsync(sgtins, success, fail, new CancellationToken(false));
        }
        public async Task ForEachPublicSgtinsByListAsync(
            string[] sgtins,
            Func<Model.PublicSgtin, Task> success,
            Func<Model.FailedSgtin, Task> fail)
        {
            await ForEachPublicSgtinsByListAsync(sgtins, success, fail, new CancellationToken(false));
        }
        #endregion
        #region Helper functions
        public async Task<TEntry[]> FilterEntriesApiCall<TEntry, TFilter>(
            string requestUri,
            TFilter filter, 
            long start_from, 
            int count,
            string errorDescription)
            where TFilter: class, new()
            where TEntry : class, new()
        {
            var response = await PostJsonAsync<Model.BaseEntriesResponse<TEntry>, Model.BaseFilterRequest<TFilter>>(
                requestUri, 
                new Model.BaseFilterRequest<TFilter>()
                {
                    filter = filter,
                    start_from = start_from,
                    count = count
                });
            AssertApiError(
                errorDescription,
                response.error_code,
                response.error_description);
            return response.entries;
        }
        protected async Task ForEachFilterEntriesApiCall<TFilter, TResponse>(
            Func<TFilter, long, int, Task<PagedResponse<TResponse>>> filterEntries,
            TFilter filter,
            Func<TResponse, Task> process,
            CancellationToken token)
        {
            int start_from = 0;
            int count = 100;
            while (!token.IsCancellationRequested)
            {
                PagedResponse<TResponse> result = null;
                try
                {
                    await LoginIfNeededAsync();
                    result = await filterEntries(filter, start_from, count);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    if (e is ApiCallException)
                        if ((e as ApiCallException).Trace.StatusCode == HttpStatusCode.Unauthorized)
                            Session.Token = null;
                    if (!SkipApiErrorsInLoops)
                        throw;
                    await Task.Delay(SkipApiErrorsDelay, token);
                    continue;
                }
                if (result == null || result.Count == 0 || result.Records == null || result.Records.Length == 0)
                    break;
                foreach (var r in result.Records)
                {
                    if (token.IsCancellationRequested)
                        break;
                    await process(r);
                }
                if (result.Count < (start_from + count))
                    break;
                start_from += result.Records.Length;
            }
        }
        protected async Task ForEachFilterEntriesApiCall<TFilter, TResponse>(
            Func<TFilter, long, int, Task<PagedResponse<TResponse>>> filterEntries,
            TFilter filter,
            Func<TResponse, Task> process)
        {
            int start_from = 0;
            int count = 100;
            while (true)
            {
                PagedResponse<TResponse> result = null;
                try
                {
                    await LoginIfNeededAsync();
                    result = await filterEntries(filter, start_from, count);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    if (e is ApiCallException)
                        if ((e as ApiCallException).Trace.StatusCode == HttpStatusCode.Unauthorized)
                            Session.Token = null;
                    if (!SkipApiErrorsInLoops)
                        throw;
                    await Task.Delay(SkipApiErrorsDelay);
                    continue;
                }
                if (result == null || result.Count == 0 || result.Records == null || result.Records.Length == 0)
                    break;
                foreach (var r in result.Records)
                    await process(r);
                if (result.Count < (start_from + count))
                    break;
                start_from += result.Records.Length;
            }
        }
        #endregion
        #region GET/POST
        protected async Task<string> GetAsync(string requestUri)
        {
            _HttpRequestSemaphore.WaitOne();
            var httpResponse = await _Client.GetAsync(requestUri);
            var response = await httpResponse.Content.ReadAsStringAsync();
            _HttpRequestSemaphore.Release();
            var trace = new TraceApiCall("GET", requestUri, httpResponse.StatusCode, httpResponse.Headers, null, response);
            if (httpResponse.StatusCode != HttpStatusCode.OK)
                throw new ApiCallException(trace);
            if (TraceApiCalls)
                Log.Debug(trace);
            return response;
        }
        protected async Task<TResponse> GetJsonAsync<TResponse>(string requestUri)
        {
            var response = await GetAsync(requestUri);
            return JsonConvert.DeserializeObject<TResponse>(response);
        }
        protected async Task<string> PostAsync(string requestUri, string request)
        {
            var content = new StringContent(request, Encoding.UTF8, "application/json");
            _HttpRequestSemaphore.WaitOne();
            var httpResponse = await Client.PostAsync(requestUri, content);
            var response = await httpResponse.Content.ReadAsStringAsync();
            _HttpRequestSemaphore.Release();
            var trace = new TraceApiCall(
                "POST", 
                requestUri, 
                httpResponse.StatusCode,
                httpResponse.Headers,
                request, 
                response);
            if (httpResponse.StatusCode != HttpStatusCode.OK)
                throw new ApiCallException(trace);
            if (TraceApiCalls)
                Log.Debug(trace);
            return response;
        }
        protected async Task PostJsonNoResultAsync<TRequest>(string requestUri, TRequest data)
        {
            var jsonRequest = JsonConvert.SerializeObject(data);
            await PostAsync(requestUri, jsonRequest);
        }
        protected async Task<TResponse> PostJsonAsync<TResponse, TRequest>(string requestUri, TRequest data)
        {
            var jsonRequest = JsonConvert.SerializeObject(data);
            var jsonResponse = await PostAsync(requestUri, jsonRequest);
            return JsonConvert.DeserializeObject<TResponse>(jsonResponse);
        }
        #endregion
        #region Asserts/Checks
        protected bool IsNotEmpty(string value)
        {
            return value != null && value != string.Empty;
        }
        protected bool IsEmpty(string value)
        {
            return value == null || value == string.Empty;
        }
        protected void AssertApiErrorIsNotEmpty(string message, string errorCode, string errorDescription, string value)
        {
            AssertApiError(message, errorCode, errorDescription, IsNotEmpty(value));
        }
        protected void AssertApiErrorIsNotEmpty(string message, string errorCode, string errorDescription, IEnumerable<string> values)
        {
            bool valid = true;
            foreach (var value in values)
                if (IsEmpty(value))
                {
                    valid = false;
                    break;
                }
            AssertApiError(message, errorCode, errorDescription, valid);
        }
        protected void AssertApiError(string message, string errorCode, string errorDescription)
        {
            AssertApiError(message, errorCode, errorDescription, true);
        }
        protected void AssertApiError(string message, string errorCode, string errorDescription, bool valid)
        {
            if(IsNotEmpty(errorCode) || IsNotEmpty(errorDescription) || !valid)
                throw new ApiException(message, errorCode, errorDescription);
        }
        #endregion
        #region Session
        protected string SessionFileName
        {
            get
            {
                return $"session_{AccountSystem.ClientId}.json";
            }
        }

        protected bool ValidateSession(Session session)
        {
            if (session == null)
                return false;
            if (session.Token == null || session.Token == string.Empty)
                return false;
            if (session.LifeTime.HasValue && session.LifeTime.Value < DateTime.Now)
                return false;
            return true;
        }
        protected bool ValidSession
        {
            get
            {
                return ValidateSession(Session);
            }
        }
        protected void SaveSession()
        {
            Cache.Save(SessionFileName, Session);
        }
        protected void LoadSession()
        {
            if (Cache.Exists(SessionFileName))
            {
                var session = Cache.Load<Session>(SessionFileName);
                if (ValidateSession(session))
                    Session = session;
                else
                    ResetSession();
            }
            else
                ResetSession();
        }
        protected void ResetSession()
        {
            Session = new Session();
            SaveSession();
        }
        #endregion

        protected async Task LoginIfNeededAsync()
        {
            if (Session.Token == null || Session.Token == string.Empty)
                await LoginAsync(false);
        }
    }
}
