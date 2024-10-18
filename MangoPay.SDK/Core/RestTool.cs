using Common.Logging;
using MangoPay.SDK.Entities;
using RestSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MangoPay.SDK.Core
{
    public class RestSharpDto
    {
        private static readonly Dictionary<string, RestSharpDto> _instances = new Dictionary<string, RestSharpDto>();

        private static readonly object _lock = new object();

        private readonly RestClientOptions _options;

        public RestClient Client { get; }

        private RestSharpDto(string url, int timeout)
        {
            _options = new RestClientOptions(url)
            {
                ThrowOnAnyError = false,
                Timeout = timeout
            };

            Client = new RestClient(_options);
            Client.UseSerializer<MangoPaySerializer>();
        }

        public static RestSharpDto GetInstance(string url, int timeout)
        {
            if (!_instances.ContainsKey(url))
            {
                lock (_lock) // now I can claim some form of thread safety...
                {
                    if (!_instances.ContainsKey(url))
                    {
                        _instances[url] = new RestSharpDto(url, timeout);
                    }
                }
            }

            return _instances[url];
        }
    }

    /// <summary>Class used to build HTTP request, call the request and handle response.</summary>
    internal class RestTool
    {
        // root/parent instance that holds the OAuthToken and Configuration instance
        private readonly MangoPayApi _root;

        // enable/disable log debugging
        //private bool _debugMode;

        // variable to flag that in request authentication data are required
        private readonly bool _authRequired;

        // array with HTTP header to send with request
        private Dictionary<string, string> _requestHttpHeaders;

        // request type for current request
        private string _requestType;

        /// <summary>Whether to include ClientId in the API url or not</summary>
        private bool _includeClientId;

        // key-value collection pass to the request
        private Dictionary<string, string> _requestData;

        // code get from response
        private int _responseCode;

        // pagination object
        private Pagination _pagination;

        // logger object
        private readonly ILog _log;

        private readonly int _timeout = 15000;

        private readonly RestSharpDto _dto;

        private readonly UrlTool _urlTool;

        /// <summary>Instantiates new RestTool object.</summary>
        /// <param name="root">Root/parent instance that holds the OAuthToken and Configuration instance.</param>
        /// <param name="authRequired">Defines whether request authentication is required.</param>
        public RestTool(MangoPayApi root, bool authRequired)
        {
            this._root = root;
            this._authRequired = authRequired;
            LogManager.Adapter = this._root.Config.LoggerFactoryAdapter;
            this._log = LogManager.GetLogger(this._root.Config.LoggerFactoryAdapter.GetType());
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            _urlTool = new UrlTool(_root);
            _dto = RestSharpDto.GetInstance(_urlTool.GetBaseUrl(), _root.Config.Timeout > 0 ? _root.Config.Timeout : _timeout);
        }

        /// <summary>Adds HTTP headers as name/value pairs into the request.</summary>
        /// <param name="httpHeader">Collection of headers name/value pairs.</param>
        internal void AddRequestHttpHeader(Dictionary<string, string> httpHeader)
        {
            if (this._requestHttpHeaders == null)
                this._requestHttpHeaders = new Dictionary<string, string>();

            foreach (var item in httpHeader)
            {
                this._requestHttpHeaders.Add(item.Key, item.Value);
            }
        }

        /// <summary>Adds HTTP header into the request.</summary>
        /// <param name="key">Header name.</param>
        /// <param name="value">Header value.</param>
        internal void AddRequestHttpHeader(string key, string value)
        {
            AddRequestHttpHeader(new Dictionary<string, string> { { key, value } });
        }

        /// <summary>Checks the HTTP response and if it's neither 200 nor 204 throws a ResponseException.</summary>
        /// <param name="restResponse">Rest response object</param>
        private void CheckResponseCode(RestResponse restResponse)
        {
            var responseCode = (int)restResponse.StatusCode;

            switch (responseCode)
            {
                case 200:
                case 204:
                    return;
                case 401:
                    throw new UnauthorizedAccessException(restResponse.Content);
            }
            
            if (restResponse.ResponseStatus == ResponseStatus.TimedOut)
                throw new TimeoutException(restResponse.ErrorMessage);

            if (restResponse.ErrorException is System.Net.ProtocolViolationException)
                throw restResponse.ErrorException;

            throw new ResponseException(restResponse.Content, responseCode);
        }

        /// <summary>Makes a call to the MangoPay API.
        /// This generic method handles calls targeting single 
        /// DTO instances. In order to process collections of objects, 
        /// use <code>RequestList</code> method instead.
        /// </summary>
        /// <typeparam name="U">Return type.</typeparam>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="endPoint">An instance of <see cref="ApiEndPoint"/> that specifies API url and method to call</param>
        /// <param name="requestData">Collection of key-value pairs of request parameters.</param>
        /// <param name="pagination">Pagination object.</param>
        /// <param name="entity">Instance of DTO class that is going to be sent in case of PUTting or POSTing.</param>
        /// <param name="idempotentKey">Idempotent key for this request.</param>
        /// <returns>The DTO instance returned from API.</returns>
        public async Task<U> RequestAsync<U, T>(ApiEndPoint endPoint, Dictionary<string, string> requestData, 
            T entity = default, Pagination pagination = null, Dictionary<string, string> additionalUrlParams = null, string idempotentKey = null)
            where U : new()
        {
            this._requestType = endPoint.RequestType;
            this._includeClientId = endPoint.IncludeClientId;
            this._requestData = requestData;

            var responseResult = await this.DoRequestAsync<U, T>(endPoint.GetUrl(), entity, pagination, additionalUrlParams, idempotentKey);

            return responseResult;
        }

        /// <summary>Makes a call to the MangoPay API. 
        /// This generic method handles calls targeting collections of 
        /// DTO instances. In order to process single objects, 
        /// use <code>Request</code> method instead.
        /// </summary>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="endPoint">An instance of <see cref="ApiEndPoint"/> that specifies API url and method to call</param>
        /// <param name="requestData">Collection of key-value pairs of request parameters.</param>
        /// <param name="pagination">Pagination object.</param>
        /// <param name="additionalUrlParams"></param>
        /// <returns>Collection of DTO instances returned from API.</returns>
        public async Task<ListPaginated<T>> RequestListAsync<T>(ApiEndPoint endPoint, Dictionary<string, string> requestData, 
            Dictionary<string, string> additionalUrlParams, Pagination pagination = null, string idempotentKey = null)
            where T : new()
        {
            this._requestType = endPoint.RequestType;
            this._includeClientId = endPoint.IncludeClientId;
            this._requestData = requestData;

            var responseResult = await this.DoRequestListAsync<T>(endPoint.GetUrl(), additionalUrlParams, pagination, idempotentKey);

            return responseResult;
        }

        private async Task<U> DoRequestAsync<U, T>(string urlMethod, T entity = default, Pagination pagination = null, Dictionary<string, string> additionalUrlParams = null, string idempotentKey = null)
            where U : new()
        {
            var restUrl = _urlTool.GetRestUrl(urlMethod, this._authRequired && this._includeClientId, pagination, additionalUrlParams, _root.Config.ApiVersion);
            
            _log.Debug("FullUrl: " + _urlTool.GetFullUrl(restUrl));

            var restRequest = new RestRequest(restUrl)
            {
                RequestFormat = DataFormat.Json,
                Method = (Method)Enum.Parse(typeof(Method), this._requestType, true)
            };

            var headers = await this.GetHttpHeadersAsync(restUrl);
            foreach (var h in headers)
            {
                if (!(h.Key == Constants.CONTENT_TYPE && this._requestType == RequestType.GET))
                    restRequest.AddHeader(h.Key, h.Value);

                if (h.Key != Constants.AUTHORIZATION)
                    _log.Debug("HTTP Header: " + h.Key + ": " + h.Value);
            }

            if (!string.IsNullOrWhiteSpace(idempotentKey))
                restRequest.AddHeader(Constants.IDEMPOTENCY_KEY, idempotentKey);

            if (pagination != null) this._pagination = pagination;

            _log.Debug("RequestType: " + this._requestType);

            if (this._requestData != null || entity != null)
            {
                if (entity != null)
                {
                    restRequest.AddBody(entity, "application/json");
                }
                if (this._requestData != null)
                {
                    foreach (var entry in this._requestData)
                    {
                        restRequest.AddParameter(entry.Key, entry.Value);
                    }
                }

                var body = restRequest.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
                var parameters = restRequest.Parameters.Where(p => p.Type == ParameterType.GetOrPost);
                foreach (var p in parameters)
                {
                    _log.Debug(p.Name + ": " + p.Value);
                }

                if (body != null)
                {
                    _log.Debug("CurrentBody: " + body.Value);
                }
                else
                {
                    _log.Debug("CurrentBody: /body is null/");
                }
            }

            var restResponse = await _dto.Client.ExecuteAsync<U>(restRequest);
            var responseObject = restResponse.Data;

            this._responseCode = (int)restResponse.StatusCode;

            if (restResponse.StatusCode == HttpStatusCode.OK || restResponse.StatusCode == HttpStatusCode.NoContent)
            {
                _log.Debug("Response OK: " + restResponse.Content);
            }
            else
            {
                _log.Debug("Response ERROR: " + restResponse.Content);
            }

            if (this._responseCode == 200)
            {
                _log.Debug("Response object: " + responseObject);
            }

            SetLastRequestInfo(restRequest, restResponse);

            this.CheckResponseCode(restResponse);

            return responseObject;
        }

        private void SetLastRequestInfo(RestRequest request, RestResponse response)
        {
            _root.LastRequestInfo = new LastRequestInfo() { Request = request, Response = response };
            
            string GetHeaderValue(string key)
            {
                return response?.Headers?
                    .FirstOrDefault(h => string.Equals(h.Name, key, StringComparison.OrdinalIgnoreCase))
                    ?.Value
                    ?.ToString();
            }

            _root.LastRequestInfo.RateLimitingCallsAllowed = GetHeaderValue("X-RateLimit-Limit");
            _root.LastRequestInfo.RateLimitingCallsRemaining = GetHeaderValue("X-RateLimit-Remaining");
            _root.LastRequestInfo.RateLimitingTimeTillReset = GetHeaderValue("X-RateLimit-Reset");
        }

        private async Task<ListPaginated<T>> DoRequestListAsync<T>(string urlMethod, Dictionary<string, string> additionalUrlParams = null,
            Pagination pagination = null, string idempotentKey = null)
        {
            ListPaginated<T> responseObject = null;

            var restUrl = _urlTool.GetRestUrl(urlMethod, this._authRequired && this._includeClientId, pagination, additionalUrlParams, _root.Config.ApiVersion);

            if (this._requestData != null)
            {
                var parameters = string.Empty;
                foreach (var entry in this._requestData)
                {
                    parameters += $"&{Uri.EscapeDataString(entry.Key)}={Uri.EscapeDataString(entry.Value)}";
                }

                if (pagination == null)
                    parameters = parameters.Remove(0, 1).Insert(0, Constants.URI_QUERY_SEPARATOR);

                restUrl += parameters;
            }

            _log.Debug("FullUrl: " + _urlTool.GetFullUrl(restUrl));

            var restRequest = new RestRequest(restUrl)
            {
                RequestFormat = DataFormat.Json,
                Method = (Method)Enum.Parse(typeof(Method), this._requestType, true)
            };

            if (!string.IsNullOrWhiteSpace(idempotentKey))
                restRequest.AddHeader(Constants.IDEMPOTENCY_KEY, idempotentKey);

            var headers = await this.GetHttpHeadersAsync(restUrl);
            foreach (var h in headers)
            {
                if (!(h.Key == Constants.CONTENT_TYPE && this._requestType == RequestType.GET))
                    restRequest.AddHeader(h.Key, h.Value);

                if (h.Key != Constants.AUTHORIZATION)
                    _log.Debug("HTTP Header: " + h.Key + ": " + h.Value);
            }

            _log.Debug("RequestType: " + this._requestType);

            var restResponse = await _dto.Client.ExecuteAsync<List<T>>(restRequest);

            responseObject = new ListPaginated<T>(restResponse.Data);

            this._responseCode = (int)restResponse.StatusCode;

            if (restResponse.StatusCode == HttpStatusCode.OK || restResponse.StatusCode == HttpStatusCode.NoContent)
            {
                _log.Debug("Response OK: " + restResponse.Content);
            }
            else
            {
                _log.Debug("Response ERROR: " + restResponse.Content);
            }

            if (this._responseCode == 200)
            {
                responseObject = this.ReadResponseHeaders<T>(restResponse, responseObject);

                _log.Debug("Response object: " + responseObject.ToString());
            }

            SetLastRequestInfo(restRequest, restResponse);

            this.CheckResponseCode(restResponse);

            return responseObject;
        }

        /// <summary>Reads and parses response headers (pagination etc.)</summary>
        /// <param name="response">The original response</param>
        /// <param name="listPaginated">The list</param>
        private ListPaginated<T> ReadResponseHeaders<T>(RestResponse response, ListPaginated<T> listPaginated)
        {
            var headers = response.Headers.Where(x => x.Name != null).ToList();
            foreach (var header in headers)
            {
                var value = header?.Value?.ToString();

                if (header.Name.ToLower().Contains(Constants.X_NUMBER_OF_PAGES.ToLower()))
                {
                    listPaginated.TotalPages = int.Parse(value);
                    continue;
                }

                if (header.Name.ToLower().Contains(Constants.X_NUMBER_OF_ITEMS.ToLower()))
                {
                    listPaginated.TotalItems = int.Parse(value);
                    continue;
                }

                if (header.Name.ToLower().Contains(Constants.LINK.ToLower()))
                {
                    var links = CustomSplit(value, ',');

                    if (links.Count <= 0) continue;

                    SetLinksForList(listPaginated, links);
                }
            }

            return listPaginated;
        }

        private List<string> CustomSplit(string input, char delim)
        {
            var list = new List<string>();
            var pos = new List<int> {0};

            for (var i = 0; i < input.Length; i++)
            {
                if (input[i] == delim)
                {
                    pos.Add(i + 1);
                }
            }

            pos.Add(input.Length + 1);

            for (var i = 1; i < pos.Count; i++)
            {
                var length = pos[i] - pos[i - 1] - 1;
                var charArray = new char[length];
                var count = 0;
                for (var j = pos[i - 1]; j < pos[i] - 1; j++)
                {
                    charArray[count++] = input[j];
                }

                list.Add(new string(charArray));
            }

            return list;
        }

        private string SubstractFromRel(string rel, char delim)
        {
            var pos = new List<int>();

            for (var i = 0; i < rel.Length; i++)
            {
                if (rel[i] == delim)
                {
                    pos.Add(i + 1);
                }
            }

            var length = pos[1] - pos[0] - 1;
            var charArr = new char[length];
            var count = 0;
            for (var i = pos[0]; i < pos[1] - 1; i++)
            {
                charArr[count++] = rel[i];
            }

            return new string(charArr);
        }

        private void SetLinksForList<T>(ListPaginated<T> listPaginated, List<string> links)
        {
            foreach (var l in links)
            {
                var oneLink = CustomSplit(l, ';');
                oneLink[1] = SubstractFromRel(oneLink[1], '"');

                if (oneLink[0] != null && oneLink[1] != null)
                {
                    if (oneLink[1] == Constants.LINKS_FIRST_ITEM) listPaginated.Links[0] = oneLink[0];
                    if (oneLink[1] == Constants.LINKS_PREVIOUS_ITEM) listPaginated.Links[1] = oneLink[0];
                    if (oneLink[1] == Constants.LINKS_NEXT_ITEM) listPaginated.Links[2] = oneLink[0];
                    if (oneLink[1] == Constants.LINKS_LAST_ITEM) listPaginated.Links[3] = oneLink[0];
                }
            }
        }

        /// <summary>Reads and parses response headers (pagination etc.)</summary>
        /// <param name="conn">Response object.</param>
        private ListPaginated<T> ReadResponseHeadersOld<T>(RestResponse restResponse, ListPaginated<T> listPaginated = null)
        {
            foreach (var k in restResponse.Headers)
            {
                string v = (string)k.Value;
                _log.Debug("Response header: " + k.Name + ":" + v);

                if (k.Name == null) continue;

                if (k.Name.Equals(Constants.X_NUMBER_OF_PAGES))
                {
                    listPaginated.TotalPages = Int32.Parse(v);
                }
                if (k.Name.Equals(Constants.X_NUMBER_OF_ITEMS))
                {
                    listPaginated.TotalItems = Int32.Parse(v);
                }
                if (k.Name.Equals(Constants.LINK))
                {
                    string linkValue = v;
                    string[] links = linkValue.Split(',');

                    if (links != null && links.Length > 0)
                    {
                        foreach (string l in links)
                        {
                            string link = l;
                            link = link.Replace("<\"", "");
                            link = link.Replace("\">", "");
                            link = link.Replace(" rel=\"", "");
                            link = link.Replace("\"", "");

                            string[] oneLink = link.Split(';');

                            if (oneLink != null && oneLink.Length > 1)
                            {
                                if (oneLink[0] != null && oneLink[1] != null)
                                {
                                    if (oneLink[1] == Constants.LINKS_FIRST_ITEM) listPaginated.Links[0] = oneLink[0];
                                    if (oneLink[1] == Constants.LINKS_PREVIOUS_ITEM) listPaginated.Links[1] = oneLink[0];
                                    if (oneLink[1] == Constants.LINKS_NEXT_ITEM) listPaginated.Links[2] = oneLink[0];
                                    if (oneLink[1] == Constants.LINKS_LAST_ITEM) listPaginated.Links[3] = oneLink[0];
                                }
                            }
                        }
                    }
                }
            }

            return listPaginated;
        }

        /// <summary>Gets HTTP header to use in request.</summary>
        /// <param name="restUrl">The REST API URL.</param>
        /// <returns>Collection of headers name-value pairs.</returns>
        private async Task<Dictionary<string, string>> GetHttpHeadersAsync(string restUrl)
        {
            // return if already created...
            if (this._requestHttpHeaders != null)
                return this._requestHttpHeaders;

            // ...or initialize with default headers
            var httpHeaders = new Dictionary<string, string>
            {
                // content type
                { Constants.CONTENT_TYPE, Constants.APPLICATION_JSON },

                // User agent header
                { Constants.USER_AGENT, $"MangoPay V2 SDK .NET {_root.GetVersion()}" }
            };

            if (_root.Config.UKHeaderFlag)
            {
                httpHeaders.Add(Constants.TENANT_ID, "uk");
            }

            // AuthenticationHelper http header
            if (!this._authRequired) return httpHeaders;

            var authHlp = new AuthenticationHelper(_root);
            var httpHelper = await authHlp.GetHttpHeaderKeyAsync();
            foreach (var item in httpHelper)
            {
                httpHeaders.Add(item.Key, item.Value);
            }

            return httpHeaders;
        }
    }
}
