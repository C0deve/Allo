using System.Net.WebSockets;

namespace Allo;

public interface IClientWebSocket : IDisposable
{
    Task ConnecterAsync(CancellationToken cancellationToken);
    Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);
    Task CloseOutputAsync(WebSocketCloseStatus status, string message, CancellationToken cancellationToken);
    WebSocketState State { get; }
}