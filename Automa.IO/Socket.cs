using Automa.IO.Proxy;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Automa.IO
{
    public interface ISocket
    {
        Task<(WebSocket socket, JsonSerializerOptions jsonOptions)> ConnectAsync(string apiMethod, CancellationToken? cancellationToken = null);
        Task<(WebSocket socket, JsonSerializerOptions jsonOptions)> AcceptAsync(WebSocketManager webSockets);
    }

    class Socket : ISocket
    {
        readonly Func<ClientWebSocket> _clientFactory;
        readonly IProxyOptions _proxyOptions;
        readonly JsonSerializerOptions _jsonOptions;

        public Socket(Func<ClientWebSocket> clientFactory, IProxyOptions proxyOptions, JsonSerializerOptions jsonOptions)
        {
            _clientFactory = clientFactory;
            _proxyOptions = proxyOptions;
            _jsonOptions = jsonOptions;
        }

        public async Task<(WebSocket socket, JsonSerializerOptions jsonOptions)> ConnectAsync(string apiMethod, CancellationToken? cancellationToken = null)
        {
            var socket = _clientFactory();
            await socket.ConnectAsync(Url(apiMethod), cancellationToken ?? CancellationToken.None);
            return (socket, _jsonOptions);
        }

        public async Task<(WebSocket socket, JsonSerializerOptions jsonOptions)> AcceptAsync(WebSocketManager webSockets)
        {
            var webSocket = await webSockets.AcceptWebSocketAsync();
            if (webSocket == null || webSocket.State != WebSocketState.Open)
                throw new InvalidOperationException("Unable to accept");
            return (webSocket, _jsonOptions);
        }

        Uri Url(string apiMethod) => new Uri($"{_proxyOptions.ProxyUri}/Automa/{apiMethod}");
    }
}
