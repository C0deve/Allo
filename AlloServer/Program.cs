using System.Net;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:6969");
var app = builder.Build();
app.UseWebSockets();


app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {

        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        app.Logger.LogInformation("Connexion");

        try
        {
            var emitTask = Emit(ws);
            
            var buffer = new ArraySegment<byte>(new byte[1024 * 4]);
            var receiveResult = await ws.ReceiveAsync(
                buffer, CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue && ws.State == WebSocketState.Open)
            {
                receiveResult = await ws.ReceiveAsync(buffer, CancellationToken.None);
            }

            await ws.CloseAsync(
                receiveResult.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
            app.Logger.LogInformation("Deconnexion");

            await emitTask;
        }
        catch (WebSocketException ex)
        {
            app.Logger.LogError(ex, "Socket");
        }
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

await app.RunAsync();

static async Task Emit(WebSocket ws)
{
    var bytes = Encoding.UTF8.GetBytes("ok");
    var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
    while (ws.State == WebSocketState.Open)
    {
        await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(1));
    }
}