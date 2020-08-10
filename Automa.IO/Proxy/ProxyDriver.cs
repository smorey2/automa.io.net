using Automa.IO.Drivers;
using OpenQA.Selenium;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyDriver" /> class.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="bearerToken">The bearer token.</param>
        public ProxyDriver(AutomaClient client, string bearerToken = null) : base(null)
        {
            _client = client;
            _socket = Default.Socket(_client.ProxyOptions);
            _ws = OpenAsync(bearerToken).Result;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (_ws.socket.State == WebSocketState.Open)
                _ws.SendAsync(ProxyMethod.Dispose).AsTask().Wait();
            _ws.socket.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Opens the asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.Net.WebSockets.WebSocketException">Closed</exception>
        public async Task<(WebSocket socket, JsonSerializerOptions jsonOptions)> OpenAsync(string bearerToken = null, CancellationToken? cancellationToken = null)
        {
            var ws = await _socket.ConnectAsync("Open", cancellationToken).ConfigureAwait(false);
            if (ws.socket.State != WebSocketState.Open)
                throw new System.Net.WebSockets.WebSocketException("Closed");
            await ws.SendAsync(bearerToken, cancellationToken).ConfigureAwait(false);
            await ws.SendAsync(ProxyMethod.Open, cancellationToken).ConfigureAwait(false);
            await ws.SendObjectAsync(_client.GetClientArgs(), cancellationToken).ConfigureAwait(false);
            await ws.ReceiveBarrier(true, cancellationToken).ConfigureAwait(false);
            return ws;
        }

        /// <summary>
        /// Gets the cookies.
        /// </summary>
        /// <value>
        /// The cookies.
        /// </value>
        /// <exception cref="NotSupportedException"></exception>
        public override CookieCollection Cookies
        {
            get => Task.Run(async () =>
            {
                var ws = _ws;
                if (ws.socket.State != WebSocketState.Open)
                    throw new System.Net.WebSockets.WebSocketException("Closed");
                await ws.SendAsync(ProxyMethod.GetCookies).ConfigureAwait(false);
                await ws.ReceiveBarrier(false).ConfigureAwait(false);
                var obj = await ws.ReceiveObjectAsync<CookieCollection>().ConfigureAwait(false);
                await ws.ReceiveBarrier(true).ConfigureAwait(false);
                return obj;
            }).Result;
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Customs the asynchronous.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="custom">The custom.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.Net.WebSockets.WebSocketException">Closed</exception>
        public async Task<object> CustomAsync(Type registration, ICustom custom, object param = null, object tag = null, CancellationToken? cancellationToken = null)
        {
            var ws = _ws;
            if (ws.socket.State != WebSocketState.Open)
                throw new System.Net.WebSockets.WebSocketException("Closed");
            await ws.SendAsync(ProxyMethod.Custom, cancellationToken).ConfigureAwait(false);
            await ws.SendAsync(registration, cancellationToken).ConfigureAwait(false);
            await ws.SendTypedAsync(param, cancellationToken).ConfigureAwait(false);
            await ws.SendTypedAsync(tag, cancellationToken).ConfigureAwait(false);
            await ws.ReceiveBarrier(false).ConfigureAwait(false);
            if (custom is ICustomWithTransfer customWithTransfer)
            {
                await customWithTransfer.ReceiveAsync(ws, param, cancellationToken).ConfigureAwait(false);
                await ws.ReceiveBarrier(false).ConfigureAwait(false);
            }
            var obj = await ws.ReceiveTypedAsync().ConfigureAwait(false);
            await ws.ReceiveBarrier(true).ConfigureAwait(false);
            return obj;
        }

        /// <summary>
        /// Logins the asynchronous.
        /// </summary>
        /// <param name="cookies">The cookies.</param>
        /// <param name="credential">The credential.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.Net.WebSockets.WebSocketException">Closed</exception>
        public async Task LoginAsync(Func<CookieCollection, Task<CookieCollection>> cookies, NetworkCredential credential, object tag = null, CancellationToken? cancellationToken = null)
        {
            var ws = _ws;
            if (ws.socket.State != WebSocketState.Open)
                throw new System.Net.WebSockets.WebSocketException("Closed");
            await ws.SendAsync(ProxyMethod.Login, cancellationToken).ConfigureAwait(false);
            await ws.SendObjectAsync(credential, cancellationToken).ConfigureAwait(false);
            await ws.SendTypedAsync(tag, cancellationToken).ConfigureAwait(false);
            await ws.ReceiveBarrier(true).ConfigureAwait(false);
        }

        /// <summary>
        /// Selects the application asynchronous.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Closed</exception>
        public async Task<object> SelectApplicationAsync(string application, object tag = null, CancellationToken? cancellationToken = null)
        {
            var ws = _ws;
            if (ws.socket.State != WebSocketState.Open)
                throw new InvalidOperationException("Closed");
            await ws.SendAsync(ProxyMethod.SelectApplication, cancellationToken).ConfigureAwait(false);
            await ws.SendAsync(application, cancellationToken).ConfigureAwait(false);
            await ws.SendTypedAsync(tag, cancellationToken);
            await ws.ReceiveBarrier(false).ConfigureAwait(false);
            var obj = await ws.ReceiveTypedAsync().ConfigureAwait(false);
            await ws.ReceiveBarrier(true).ConfigureAwait(false);
            return obj;
        }

        /// <summary>
        /// Sets the device access token asynchronous.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="userCode">The user code.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="System.Net.WebSockets.WebSocketException">Closed</exception>
        public async Task SetDeviceAccessTokenAsync(string url, string userCode, object tag = null, CancellationToken? cancellationToken = null)
        {
            var ws = _ws;
            if (ws.socket.State != WebSocketState.Open)
                throw new System.Net.WebSockets.WebSocketException("Closed");
            await ws.SendAsync(ProxyMethod.SetDeviceAccessToken, cancellationToken).ConfigureAwait(false);
            await ws.SendAsync(url, cancellationToken).ConfigureAwait(false);
            await ws.SendAsync(userCode, cancellationToken).ConfigureAwait(false);
            await ws.SendTypedAsync(tag, cancellationToken).ConfigureAwait(false);
            await ws.ReceiveBarrier(true).ConfigureAwait(false);
        }
    }
}