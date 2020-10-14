using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Automa.IO
{
    public static class WebSocketExtensions
    {
        /// <summary>
        /// BarrierMethod
        /// </summary>
        public enum BarrierMethod
        {
            Barrier = -1,
            EndOfMessage = -2,
            Exception = -3,
        }

        /// <summary>
        /// Sends the barrier.
        /// </summary>
        /// <param name="ws">The ws.</param>
        /// <param name="endOfMessage">if set to <c>true</c> [end of message].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static ValueTask SendBarrier(this (WebSocket socket, JsonSerializerOptions jsonOptions) ws, bool endOfMessage, CancellationToken? cancellationToken = null) =>
            ws.SendAsync(endOfMessage ? BarrierMethod.EndOfMessage : BarrierMethod.Barrier, cancellationToken);

        /// <summary>
        /// Sends the exception.
        /// </summary>
        /// <param name="ws">The ws.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async ValueTask SendExceptionAsync(this (WebSocket socket, JsonSerializerOptions jsonOptions) ws, Exception exception, CancellationToken? cancellationToken = null)
        {
            await ws.SendAsync(BarrierMethod.Exception, cancellationToken).ConfigureAwait(false);
            await ws.SendObjectAsync(new WebSocketErrorResponse(exception), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Receives the barrier.
        /// </summary>
        /// <param name="ws">The ws.</param>
        /// <param name="endOfMessage">if set to <c>true</c> [end of message].</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="WebSocketException"></exception>
        /// <exception cref="ArgumentOutOfRangeException">method</exception>
        public static async ValueTask ReceiveBarrier(this (WebSocket socket, JsonSerializerOptions jsonOptions) ws, bool endOfMessage, CancellationToken? cancellationToken = null)
        {
            var method = await ws.ReceiveAsync<BarrierMethod>(cancellationToken).ConfigureAwait(false);
            switch (method)
            {
                case BarrierMethod.Barrier when !endOfMessage: return;
                case BarrierMethod.EndOfMessage when endOfMessage: return;
                case BarrierMethod.Exception: throw new WebSocketException(await ws.ReceiveObjectAsync<WebSocketErrorResponse>().ConfigureAwait(false));
                default: throw new ArgumentOutOfRangeException(nameof(method), method.ToString());
            }
        }

        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ws">The ws.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentOutOfRangeException">T</exception>
        public static async ValueTask SendAsync<T>(this (WebSocket socket, JsonSerializerOptions jsonOptions) ws, T value, CancellationToken? cancellationToken = null)
        {
            var messageType = WebSocketMessageType.Binary;
            byte[] bytes;
            if (value == null) bytes = new byte[0];
            else if (value is Enum enumValue) bytes = BitConverter.GetBytes((int)Convert.ChangeType(enumValue, enumValue.GetTypeCode()));
            else if (value is Type typeValue) { bytes = Encoding.UTF8.GetBytes(typeValue.AssemblyQualifiedName); messageType = WebSocketMessageType.Text; }
            else if (value is string stringValue) { bytes = Encoding.UTF8.GetBytes(stringValue); messageType = WebSocketMessageType.Text; }
            else if (value is byte[] bytesValue) bytes = bytesValue;
            else if (value is int intValue) bytes = BitConverter.GetBytes(intValue);
            else if (value is bool boolValue) bytes = BitConverter.GetBytes(boolValue);
            else if (value is byte byteValue) bytes = new[] { byteValue };
            else throw new ArgumentOutOfRangeException(nameof(T), typeof(T).ToString());
            await ws.socket.SendAsync(new ArraySegment<byte>(bytes), messageType, true, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Receives the asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ws">The ws.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="chunkSize">Size of the chunk.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException">T</exception>
        public static async ValueTask<T> ReceiveAsync<T>(this (WebSocket socket, JsonSerializerOptions jsonOptions) ws, CancellationToken? cancellationToken = null, int chunkSize = 2048)
        {
            var type = typeof(T);
            Func<byte[], object> convert = null;
            Func<MemoryStream, object> streamConvert = null;
            if (type.IsEnum) convert = s => Enum.ToObject(type, BitConverter.ToInt32(s, 0));
            else if (type == typeof(Type)) streamConvert = s => { using (var reader = new StreamReader(s, Encoding.UTF8)) return Type.GetType(reader.ReadToEnd(), true); };
            else if (type == typeof(string)) streamConvert = s => { using (var reader = new StreamReader(s, Encoding.UTF8)) return reader.ReadToEnd(); };
            else if (type == typeof(byte[])) streamConvert = s => s.ToArray();
            else if (type == typeof(int)) convert = s => BitConverter.ToInt32(s, 0);
            else if (type == typeof(bool)) convert = s => BitConverter.ToBoolean(s, 0);
            else if (type == typeof(byte)) convert = s => s[0];
            else throw new ArgumentOutOfRangeException(nameof(T), typeof(T).ToString());

            // receive
            WebSocketReceiveResult result;
            if (convert != null)
            {
                var buf = new ArraySegment<byte>(new byte[8]);
                result = await ws.socket.ReceiveAsync(buf, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
                return buf.Count > 0 ? (T)convert(buf.Array) : default;
            }
            var buffer = new ArraySegment<byte>(new byte[chunkSize]);
            do
            {
                using (var s = new MemoryStream())
                {
                    do
                    {
                        result = await ws.socket.ReceiveAsync(buffer, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
                        s.Write(buffer.Array, buffer.Offset, result.Count);
                    } while (!result.EndOfMessage);

                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    s.Seek(0, SeekOrigin.Begin);
                    return s.Length > 0 ? (T)streamConvert(s) : default;
                }
            } while (true);
            return default;
        }

        /// <summary>
        /// Sends the object asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ws">The ws.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async ValueTask SendObjectAsync<T>(this (WebSocket socket, JsonSerializerOptions jsonOptions) ws, T value, CancellationToken? cancellationToken = null) =>
            await ws.socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, ws.jsonOptions))), WebSocketMessageType.Text, true, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);

        public static async ValueTask<T> ReceiveObjectAsync<T>(this (WebSocket socket, JsonSerializerOptions jsonOptions) ws, CancellationToken? cancellationToken = null) =>
            JsonSerializer.Deserialize<T>(await ws.ReceiveAsync<string>(cancellationToken).ConfigureAwait(false), ws.jsonOptions);

        /// <summary>
        /// Sends the typed asynchronous.
        /// </summary>
        /// <param name="ws">The ws.</param>
        /// <param name="value">The value.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async ValueTask SendTypedAsync(this (WebSocket socket, JsonSerializerOptions jsonOptions) ws, object value, CancellationToken? cancellationToken = null) =>
            await ws.socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize((value, value?.GetType().AssemblyQualifiedName), ws.jsonOptions))), WebSocketMessageType.Text, true, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);

        /// <summary>
        /// Receives the typed asynchronous.
        /// </summary>
        /// <param name="ws">The ws.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public static async ValueTask<object> ReceiveTypedAsync(this (WebSocket socket, JsonSerializerOptions jsonOptions) ws, CancellationToken? cancellationToken = null)
        {
            var (obj, typeName) = await ws.ReceiveObjectAsync<(JsonElement obj, string typeName)>();
            if (string.IsNullOrEmpty(typeName))
                return null;
            var type = Type.GetType(typeName);
            return obj.GetObject(type);
        }
    }
}