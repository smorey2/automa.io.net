using Automa.IO.Drivers;
using OpenQA.Selenium;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Automa.IO.Proxy
{
    /// <summary>
    /// ProxyDriver
    /// </summary>
    public class ProxyDriver : AbstractDriver
    {
        readonly AutomaClient _client;
        readonly ISocket _socket;
        readonly (WebSocket socket, JsonSerializerOptions jsonOptions) _ws;

        public ProxyDriver(AutomaClient client, Action<DriverOptions> driverOptions) : base(null)
        {
            _client = client;
            _socket = Default.Socket(_client.ProxyOptions);
            _ws = OpenAsync().Result;
        }

        protected override void Dispose(bool disposing)
        {
            if (_ws.socket.State == WebSocketState.Open)
                _ws.SendAsync(ProxyMethod.Dispose).Wait();
            _ws.socket.Dispose();
            base.Dispose(disposing);
        }

        public async Task<(WebSocket socket, JsonSerializerOptions jsonOptions)> OpenAsync(CancellationToken? cancellationToken = null)
        {
            var ws = await _socket.ConnectAsync("Open", cancellationToken).ConfigureAwait(false);
            if (ws.socket.State != WebSocketState.Open)
                throw new WebSocketException();
            await ws.SendAsync(ProxyMethod.Open, cancellationToken).ConfigureAwait(false);
            await ws.SendObjectAsync(_client.GetClientArgs(), cancellationToken).ConfigureAwait(false);
            return ws;
        }

        public async Task LoginAsync(Func<CookieCollection, Task<CookieCollection>> cookies, NetworkCredential credential, object tag = null, CancellationToken? cancellationToken = null)
        {
            if (_ws.socket.State != WebSocketState.Open)
                throw new WebSocketException();
            await _ws.SendAsync(ProxyMethod.Login, cancellationToken).ConfigureAwait(false);
            await _ws.SendObjectAsync(credential, cancellationToken).ConfigureAwait(false);
            await _ws.SendObjectAsync((tag, tag?.GetType().AssemblyQualifiedName), cancellationToken).ConfigureAwait(false);
        }

        public async Task<object> SelectApplicationAsync(string application, object tag = null, CancellationToken? cancellationToken = null)
        {
            if (_ws.socket.State != WebSocketState.Open)
                throw new InvalidOperationException();
            await _ws.SendAsync(ProxyMethod.SelectApplication, cancellationToken).ConfigureAwait(false);
            await _ws.SendAsync(application, cancellationToken).ConfigureAwait(false);
            await _ws.SendObjectAsync((tag, tag?.GetType().AssemblyQualifiedName), cancellationToken);
            if (await _ws.ReceiveAsync<ProxyMethod>().ConfigureAwait(false) != ProxyMethod.Ready)
                throw new InvalidOperationException();
            return await _ws.ReceiveTypedAsync().ConfigureAwait(false);
        }

        public async Task SetDeviceAccessTokenAsync(string url, string userCode, object tag = null, CancellationToken? cancellationToken = null)
        {
            if (_ws.socket.State != WebSocketState.Open)
                throw new WebSocketException();
            await _ws.SendAsync(ProxyMethod.SetDeviceAccessToken, cancellationToken).ConfigureAwait(false);
            await _ws.SendAsync(url, cancellationToken).ConfigureAwait(false);
            await _ws.SendAsync(userCode, cancellationToken).ConfigureAwait(false);
            await _ws.SendObjectAsync((tag, tag?.GetType().AssemblyQualifiedName), cancellationToken).ConfigureAwait(false);
            throw new NotSupportedException();
        }

        public override CookieCollection Cookies
        {
            get => Task.Run(async () =>
            {
                if (_ws.socket.State != WebSocketState.Open)
                    throw new WebSocketException();
                await _ws.SendAsync(ProxyMethod.GetCookies).ConfigureAwait(false);
                if (await _ws.ReceiveAsync<ProxyMethod>().ConfigureAwait(false) != ProxyMethod.Ready)
                    throw new InvalidOperationException();
                return await _ws.ReceiveObjectAsync<CookieCollection>().ConfigureAwait(false);
            }).Result;
            set => throw new NotSupportedException();
        }
    }
}