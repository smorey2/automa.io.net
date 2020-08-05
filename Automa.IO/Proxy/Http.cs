using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Automa.IO.Proxy
{
    public interface IHttp
    {
        Task<T> Execute<T>(HttpRequestMessage requestMessage, CancellationToken? cancellationToken = null);
    }

    class Http : IHttp
    {
        readonly HttpClient _client;
        readonly JsonSerializerOptions _serializerOptions;
        public static string LastErrorContent;

        public Http(HttpClient client, JsonSerializerOptions serializerOptions)
        {
            _client = client;
            _serializerOptions = serializerOptions;
        }

        public async Task<T> Execute<T>(HttpRequestMessage requestMessage, CancellationToken? cancellationToken = null)
        {
            try
            {
                var response = await _client.SendAsync(requestMessage, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    LastErrorContent = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return await DeserializeAsync<T>(response).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        async ValueTask<T> DeserializeAsync<T>(HttpResponseMessage response) =>
            await JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false), _serializerOptions);
    }
}
