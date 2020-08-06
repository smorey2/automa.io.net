using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Args = System.Collections.Generic.Dictionary<string, object>;

namespace Automa.IO.Proxy
{
    public class WebApiClient
    {
        static readonly JsonSerializerOptions _jsonOptions = Default.JsonOptions;
        static readonly IHttp _http = Default.Http(_jsonOptions);
        readonly IProxyOptions _options;
        readonly string _token;

        public WebApiClient(IProxyOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _token = options.Token ?? "TOKEN";
        }

        public Task Get(string apiMethod, Args args, CancellationToken? cancellationToken) =>
            Get<object>(apiMethod, args, cancellationToken);

        public async Task<T> Get<T>(string apiMethod, Args args, CancellationToken? cancellationToken) where T : class
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, Url(apiMethod, args));
            return Deserialize<T>(await _http.Execute<WebApiResponse>(requestMessage, cancellationToken ?? CancellationToken.None).ConfigureAwait(false));
        }

        public Task Post(string apiMethod, Args args, CancellationToken? cancellationToken) =>
            Post<object>(apiMethod, args, cancellationToken);

        public Task<T> Post<T>(string apiMethod, Args args, CancellationToken? cancellationToken) where T : class =>
            Post<T>(Url(apiMethod, args), (object)StripNullArgs(args), cancellationToken);

        public Task Post(string apiMethod, Args args, HttpContent content, CancellationToken? cancellationToken) =>
            Post<object>(apiMethod, args, content, cancellationToken);

        public async Task<T> Post<T>(string apiMethod, Args args, HttpContent content, CancellationToken? cancellationToken) where T : class
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, Url(apiMethod, args)) { Content = content };
            return Deserialize<T>(await _http.Execute<WebApiResponse>(requestMessage, cancellationToken ?? CancellationToken.None).ConfigureAwait(false));
        }

        async Task<T> Post<T>(string requestUri, object body, CancellationToken? cancellationToken) where T : class
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json");

            var response = await _http.Execute<WebApiResponse>(requestMessage, cancellationToken ?? CancellationToken.None).ConfigureAwait(false)
                ?? new WebApiResponse { ok = true };
            return Deserialize<T>(response);
        }

        string Url(string apiMethod, Args args) =>
            $"{_options.ProxyUri}/Automa/{apiMethod}";

        T Deserialize<T>(WebApiResponse response) where T : class =>
           response.ok
               ? JsonSerializer.Deserialize<T>(response.data?["data"].GetRawText(), _jsonOptions)
               : throw new ProxyException(JsonSerializer.Deserialize<ErrorResponse>(response.data?["data"].GetRawText(), _jsonOptions));

        static Args StripNullArgs(Args args) =>
            args.Where(kv => kv.Value != null)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

    }
}
