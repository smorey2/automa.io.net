using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Automa.IO
{
    public interface IHttp
    {
        Task<T> ExecuteAsync<T>(HttpRequestMessage requestMessage, CancellationToken? cancellationToken = null);
    }

    class Http : IHttp
    {
        readonly HttpClient _client;
        readonly JsonSerializerOptions _jsonOptions;
        public static string LastErrorContent;

        public Http(HttpClient client, JsonSerializerOptions jsonOptions)
        {
            _client = client;
            _jsonOptions = jsonOptions;
        }

        public async Task<T> ExecuteAsync<T>(HttpRequestMessage requestMessage, CancellationToken? cancellationToken = null)
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
            await JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false), _jsonOptions);
    }
}
