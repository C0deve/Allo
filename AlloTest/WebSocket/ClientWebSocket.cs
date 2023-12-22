using System.Net.WebSockets;
using System.Text;
using Allo;

namespace AlloTest.WebSocket;

public sealed class ClientWebSocket : IClientWebSocket
{
    private readonly CancellationTokenSource _mainCancellationTokenSource = new();
    private CancellationTokenSource? _fakeReceiveCancellationTokenSourceSource;
    private string _value = "";

    public Task ConnecterAsync(CancellationToken cancellationToken1)
    {
        State = WebSocketState.Open;
        return Task.CompletedTask;
    }

    public Task FakeReceiveAsync(string value)
    {
        _value = value;
        return _fakeReceiveCancellationTokenSourceSource != null
            ? _fakeReceiveCancellationTokenSourceSource.CancelAsync()
            : Task.CompletedTask;
    }

    public async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        using var waitFakeReceiveCancellationTokenSourceSource = new CancellationTokenSource();
        _fakeReceiveCancellationTokenSourceSource = waitFakeReceiveCancellationTokenSourceSource;
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(_mainCancellationTokenSource.Token, waitFakeReceiveCancellationTokenSourceSource.Token);

        try
        {
            await Task.Delay(TimeSpan.FromHours(5), linked.Token);
        }
        catch (OperationCanceledException)
        {
            if (cancellationToken.IsCancellationRequested)
                return new WebSocketReceiveResult(
                    buffer.Count,
                    WebSocketMessageType.Text,
                    true,
                    WebSocketCloseStatus.NormalClosure,
                    "");
        }

        var bytes = Encoding.ASCII.GetBytes(_value);
        for (var i = 0; i < bytes.Length; i++)
        {
            buffer.Array[i] = bytes[i];
        }
        return new WebSocketReceiveResult(bytes.Length, WebSocketMessageType.Text, true);
    }

    public async Task CloseAsync(WebSocketCloseStatus status, string message, CancellationToken cancellationToken)
    {
        await _mainCancellationTokenSource.CancelAsync();
        State = WebSocketState.Closed;
    }

    public WebSocketState State { get; private set; } = WebSocketState.None;

    public void Dispose()
    {
    }
}