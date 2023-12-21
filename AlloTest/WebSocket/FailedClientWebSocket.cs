using System.Net.WebSockets;
using Allo;

namespace AlloTest.WebSocket;

public sealed class FailedClientWebSocket : IClientWebSocket
{
    public void Dispose() {}

    public Task ConnecterAsync(CancellationToken cancellationToken) => throw new WebSocketException();

    public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken) => throw new NotImplementedException();

    public Task CloseOutputAsync(WebSocketCloseStatus status, string message, CancellationToken cancellationToken) => throw new NotImplementedException();

    public WebSocketState State => WebSocketState.Closed;
}