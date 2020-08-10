using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Args = System.Collections.Generic.Dictionary<string, object>;

// https://stackoverflow.com/questions/35554128/opening-a-websocket-channel-inside-mvc-controller
namespace Automa.IO.Proxy
{
    /// <summary>
    /// WebHost
    /// </summary>
    public class ProxyHost
    {
        readonly ISocket _socket = Default.Socket(null);

        public async Task<int> OpenAsync(HttpContext context, CancellationToken? cancellationToken = null)
        {
            var webSockets = context.WebSockets;
            if (!webSockets.IsWebSocketRequest)
                return 405;
            var ws = await _socket.AcceptAsync(webSockets).ConfigureAwait(false);
            AutomaClient client = null;
            try
            {
                var opened = true;
                while (opened && !context.RequestAborted.IsCancellationRequested)
                {
                    var method = await ws.ReceiveAsync<ProxyMethod>(cancellationToken).ConfigureAwait(false);
                    switch (method)
                    {
                        case ProxyMethod.Open:
                            {
                                var args = await ws.ReceiveObjectAsync<Args>(cancellationToken).ConfigureAwait(false);
                                client = AutomaClient.Parse(args);
                                break;
                            }
                        case ProxyMethod.Custom:
                            {
                                var registration = await ws.ReceiveAsync<Type>(cancellationToken).ConfigureAwait(false);
                                var param = await ws.ReceiveTypedAsync(cancellationToken).ConfigureAwait(false);
                                var tag = await ws.ReceiveTypedAsync(cancellationToken).ConfigureAwait(false);
                                var (obj, custom) = await client.Automa.CustomAsync(registration, param, tag).ConfigureAwait(false);
                                await ws.SendBarrier(false, cancellationToken).ConfigureAwait(false);
                                if (custom is ICustomWithTransfer customWithTransfer)
                                {
                                    await customWithTransfer.SendAsync(ws, param).ConfigureAwait(false);
                                    await ws.SendBarrier(false, cancellationToken).ConfigureAwait(false);
                                }
                                await ws.SendTypedAsync(obj, cancellationToken).ConfigureAwait(false);
                                break;
                            }
                        case ProxyMethod.Login:
                            {
                                var credential = await ws.ReceiveObjectAsync<NetworkCredential>(cancellationToken).ConfigureAwait(false);
                                client.ServiceLogin = credential.UserName;
                                client.ServicePassword = credential.Password;
                                var tag = await ws.ReceiveTypedAsync(cancellationToken).ConfigureAwait(false);
                                await client.Automa.LoginAsync(tag).ConfigureAwait(false);
                                break;
                            }
                        case ProxyMethod.SelectApplication:
                            {
                                var application = await ws.ReceiveAsync<string>(cancellationToken).ConfigureAwait(false);
                                var tag = await ws.ReceiveTypedAsync(cancellationToken).ConfigureAwait(false);
                                var obj = await client.Automa.SelectApplicationAsync(application, tag).ConfigureAwait(false);
                                await ws.SendBarrier(false, cancellationToken).ConfigureAwait(false);
                                await ws.SendTypedAsync(obj, cancellationToken).ConfigureAwait(false);
                                break;
                            }
                        case ProxyMethod.SetDeviceAccessToken:
                            {
                                var url = await ws.ReceiveAsync<string>(cancellationToken).ConfigureAwait(false);
                                var code = await ws.ReceiveAsync<string>(cancellationToken).ConfigureAwait(false);
                                var tag = await ws.ReceiveTypedAsync(cancellationToken).ConfigureAwait(false);
                                await client.Automa.SetDeviceAccessTokenAsync(url, code, tag).ConfigureAwait(false);
                                break;
                            }
                        case ProxyMethod.GetCookies:
                            {
                                await ws.SendBarrier(false, cancellationToken).ConfigureAwait(false);
                                await ws.SendObjectAsync(client.Automa.Cookies, cancellationToken).ConfigureAwait(false);
                                break;
                            }
                        case ProxyMethod.Dispose:
                            {
                                opened = false;
                                break;
                            }
                        default: throw new ArgumentOutOfRangeException(nameof(method), method.ToString());
                    }
                    await ws.SendBarrier(true, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                await ws.SendExceptionAsync(e, cancellationToken);
            }
            finally
            {
                client?.Dispose();
            }
            return 101;
        }
    }
}
