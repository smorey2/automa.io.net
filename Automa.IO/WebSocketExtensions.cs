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
        public static async Task SendAsync<T>(this (WebSocket socket, JsonSerializerOptions jsonOptions) ws, T value, CancellationToken? cancellationToken = null)
        {
            var messageType = WebSocketMessageType.Binary;
            byte[] bytes;
            if (value == null) bytes = new byte[0];
            else if (value is Enum enumValue) bytes = BitConverter.GetBytes((int)Convert.ChangeType(enumValue, enumValue.GetTypeCode()));
            else if (value is string stringValue) { bytes = Encoding.UTF8.GetBytes(stringValue); messageType = WebSocketMessageType.Text; }
            else if (value is byte[] bytesValue) bytes = bytesValue;
            else if (value is int intValue) bytes = BitConverter.GetBytes(intValue);
            else if (value is byte byteValue) bytes = new[] { byteValue };
            else throw new ArgumentOutOfRangeException(nameof(T), typeof(T).ToString());
            await ws.socket.SendAsync(new ArraySegment<byte>(bytes), messageType, true, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        }

        public static async ValueTask<T> ReceiveAsync<T>(this (WebSocket socket, JsonSerializerOptions jsonOptions) ws, CancellationToken? cancellationToken = null, int chunkSize = 2048)
        {
            var type = typeof(T);
            Func<byte[], object> convert = null;
            Func<MemoryStream, object> streamConvert = null;
            if (type.IsEnum) convert = s => Enum.ToObject(type, BitConverter.ToInt32(s, 0));
            else if (type == typeof(string)) streamConvert = s => { using (var reader = new StreamReader(s, Encoding.UTF8)) return reader.ReadToEnd(); };
            else if (type == typeof(byte[])) streamConvert = s => s.ToArray();
            else if (type == typeof(int)) convert = s => BitConverter.ToInt32(s, 0);
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

        public static async Task SendObjectAsync<T>(this (WebSocket socket, JsonSerializerOptions jsonOptions) ws, T value, CancellationToken? cancellationToken = null) =>
            await ws.socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, ws.jsonOptions))), WebSocketMessageType.Text, true, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);

        public static async ValueTask<T> ReceiveObjectAsync<T>(this (WebSocket socket, JsonSerializerOptions jsonOptions) ws, CancellationToken? cancellationToken = null) =>
            JsonSerializer.Deserialize<T>(await ws.ReceiveAsync<string>(cancellationToken).ConfigureAwait(false), ws.jsonOptions);

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