using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Net;
using System.Threading;
using TidyHQ.Resources;

namespace TidyHQ.Client
{
    class ConnectClient
    {
        private Uri _tokenBaseUri;
        private Uri _restApiBaseUri;
        private string _domainPrefix;
        private string _clientID;
        private string _clientSecret;
        private string _username;
        private string _password;
        private string _token;
        private string _authorizationHeaderValue;
        private JsonSerializer _jsonSerializer;

        public ConnectClient(Uri tokenRequestUri, Uri restApiUri, string domainPrefix, string clientId, string clientSecret, string username, string password)
        {
            if (tokenRequestUri == null) throw new ArgumentNullException();
            if (!tokenRequestUri.IsAbsoluteUri) throw new ArgumentException();
            if (!"http".Equals(tokenRequestUri.Scheme, StringComparison.OrdinalIgnoreCase)
                && !"https".Equals(tokenRequestUri.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException();
            }
            if (!string.IsNullOrEmpty(tokenRequestUri.Query))
            {
                throw new ArgumentException();
            }
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException();
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException();
            }
            _tokenBaseUri = tokenRequestUri;
            _restApiBaseUri = restApiUri;
            _domainPrefix = domainPrefix;
            _clientID = clientId;
            _clientSecret = clientSecret;
            _username = username;
            _password = password;
            _jsonSerializer = JsonSerializer.Create();
        }

        private List<T> GetCommon<T>( string accessResourceWithToken, int start = -1, int limit = -1,
            List<KeyValuePair<string, string>> extraQueryStringParameters = null)
        {
            var uri = new Uri(_restApiBaseUri.ToString());
            var uriStrBuilder = new StringBuilder();
            uriStrBuilder.Append(uri);
            uriStrBuilder.Append(accessResourceWithToken);
            if (0 <= start || 0 <= limit ||
                (extraQueryStringParameters != null && 0 < extraQueryStringParameters.Count))
            {
                uriStrBuilder.Append('?');
                int keyValuePairCount = 0;
                if (0 <= start)
                {
                    uriStrBuilder.Append("start=");
                    uriStrBuilder.Append(start);
                    keyValuePairCount += 1;
                }
                if (0 <= limit)
                {
                    if (0 < keyValuePairCount) uriStrBuilder.Append('&');
                    uriStrBuilder.Append("limit=");
                    uriStrBuilder.Append(limit);
                    keyValuePairCount += 1;
                }
                if (extraQueryStringParameters != null)
                {
                    foreach (var keyValuePair in extraQueryStringParameters)
                    {
                        if (0 < keyValuePairCount) uriStrBuilder.Append('&');
                        uriStrBuilder.Append(WebUtility.UrlEncode(keyValuePair.Key));
                        uriStrBuilder.Append('=');
                        uriStrBuilder.Append(WebUtility.UrlEncode(keyValuePair.Value));
                        keyValuePairCount += 1;
                    }
                }
            }
            var uriStr = uriStrBuilder.ToString();
            int attemptNo = 0;
            while (true)
            {
                var getRequest = WebRequest.Create(uriStr) as HttpWebRequest;
                WebResponse response = null;
                try
                {
                    WebException ex1 = null;
                    JsonReaderException ex2 = null;
                    try
                    {
                        response = getRequest.GetResponse();
                    }
                    catch (WebException ex3)
                    {
                        ex1 = ex3;
                        response = ex1.Response;
                    }
                    WebExceptionStatus status = ex1?.Status ?? WebExceptionStatus.Success;
                    if (response == null)
                    {
                        goto retry;
                    }
                    using (var stream = response.GetResponseStream())
                    using (var streamReader = new StreamReader(stream))
                    using (var jsonReader = new JsonTextReader(streamReader))
                    {
                        if (status == WebExceptionStatus.Success)
                        {
                            return _jsonSerializer.Deserialize<List<T>>(jsonReader);
                        }
                        ErrorList errorList;
                        try
                        {
                            errorList = _jsonSerializer.Deserialize<ErrorList>(jsonReader);
                        }
                        catch (JsonReaderException ex3)
                        {
                            ex2 = ex3;
                            goto retry;
                        }
                        var error = errorList.Errors[0];
                        throw new ClientException(error.Message);
                    }
                    retry:
                    attemptNo += 1;
                    if (3 < attemptNo)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    Debug.Assert(ex1 != null);
                    throw (ex2 == null) ? ((Exception) ex1) : new AggregateException(ex1, ex2);
                }
                finally
                {
                    response?.Dispose();
                }
            }
        }

        public List<ContactsStructure> Contacts(string accessToken, int start = -1, int limit = -1)
        { 
            return GetCommon<ContactsStructure>($"contacts?access_token={accessToken}", start: start, limit: limit);
        }

        public List<GroupStructure> Groups(string accessToken, int contactId, int start = -1, int limit = -1)
        {
            return GetCommon<GroupStructure>($"contacts/{contactId}/groups?access_token={accessToken}", start: start, limit: limit);
        }


        public TokenResponse TokenRequest()
        {
            return GetToken<TokenResponse>();
        }

        public T GetToken<T>()
        {
            var uri = new Uri(_tokenBaseUri.ToString());
            var uriStrBuilder = new StringBuilder();
            uriStrBuilder.Append(uri);
            uriStrBuilder.Append($"?domain_prefix={_domainPrefix}&client_id={_clientID}&client_secret={_clientSecret}&username={_username}&password={_password}&grant_type=password");
            var uriStr = uriStrBuilder.ToString();
            int attemptNo = 0;
            while (true)
            {
                var getRequest = WebRequest.Create(uriStr) as HttpWebRequest;
                if (getRequest != null)
                {
                    getRequest.Method = "POST";
                    WebResponse response = null;
                    try
                    {
                        WebException ex1 = null;
                        JsonReaderException ex2 = null;
                        try
                        {
                            response = getRequest.GetResponse();
                        }
                        catch (WebException ex3)
                        {
                            ex1 = ex3;
                            response = ex1.Response;
                        }
                        WebExceptionStatus status = ex1?.Status ?? WebExceptionStatus.Success;
                        if (response == null)
                        {
                            goto retry;
                        }
                        using (var stream = response.GetResponseStream())
                        using (var streamReader = new StreamReader(stream))
                        using (var jsonReader = new JsonTextReader(streamReader))
                        {
                            if (status == WebExceptionStatus.Success)
                            {
                                return _jsonSerializer.Deserialize<T>(jsonReader);
                            }
                            ErrorList errorList;
                            try
                            {
                                errorList = _jsonSerializer.Deserialize<ErrorList>(jsonReader);
                            }
                            catch (JsonReaderException ex3)
                            {
                                ex2 = ex3;
                                goto retry;
                            }
                            var error = errorList.Errors[0];                            
                            throw new ClientException(error.Message);
                        }
                        retry:
                        attemptNo += 1;
                        if (3 < attemptNo)
                        {
                            Thread.Sleep(1000);
                            continue;
                        }
                        Debug.Assert(ex1 != null);
                        throw (ex2 == null) ? ((Exception)ex1) : new AggregateException(ex1, ex2);
                    }
                    finally
                    {
                        response?.Dispose();
                    }
                }
            }
        } 
    }

    public class Error
    {
        public string ExceptionName { get; set; }
        public string Message { get; set; }

    }

    public class ClientException : Exception
    {
        public ClientException(string message)
            : base(message)
        {

        }
    }
    
    public class ErrorList
    {
        public List<Error> Errors { get; set; }
    }

}

