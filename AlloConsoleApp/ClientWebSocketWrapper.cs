using System.Net.WebSockets;
using Allo;

namespace AlloConsoleApp;

internal class ClientWebSocketWrapper(ClientWebSocket clientWebSocket) : IClientWebSocket
{
    public void Dispose() => clientWebSocket.Dispose();

    public Task ConnecterAsync(CancellationToken cancellationToken) =>
        clientWebSocket.ConnectAsync(new Uri($"ws://localhost:6969/ws"), CancellationToken.None);

    public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken) => 
        clientWebSocket.ReceiveAsync(buffer, cancellationToken);

    public Task CloseAsync(WebSocketCloseStatus status, string message, CancellationToken cancellationToken) =>
        clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);

    public WebSocketState State => clientWebSocket.State;
}