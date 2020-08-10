using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Automa.IO
{
    /// <summary>
    /// ICustom
    /// </summary>
    public interface ICustom
    {
        /// <summary>
        /// Executes the asynchronous.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<object> ExecuteAsync(AutomaClient client, object param = null, object tag = null, CancellationToken? cancellationToken = null);
    }

    /// <summary>
    /// ICustomWithTransfer
    /// </summary>
    /// <seealso cref="Automa.IO.ICustom" />
    public interface ICustomWithTransfer : ICustom
    {
        /// <summary>
        /// Sends the asynchronous.
        /// </summary>
        /// <param name="ws">The ws.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task SendAsync((WebSocket socket, JsonSerializerOptions jsonOptions) ws, object param = null, CancellationToken? cancellationToken = null);
        /// <summary>
        /// Receives the asynchronous.
        /// </summary>
        /// <param name="ws">The ws.</param>
        /// <param name="param">The parameter.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task ReceiveAsync((WebSocket socket, JsonSerializerOptions jsonOptions) ws, object param = null, CancellationToken? cancellationToken = null);
    }
}
