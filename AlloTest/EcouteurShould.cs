using System.Net.WebSockets;
using Allo;

namespace AlloTest;

using ClientWebSocket = WebSocket.ClientWebSocket;

public class EcouteurShould
{
    [Fact]
    public async Task SeConnecter()
    {
        await using var sut = new Ecouteur(new ClientWebSocket(), new Executeur());

        _ = sut.EcouterAsync();

        sut.State.ShouldBe(WebSocketState.Open);
    }

    [Fact]
    public async Task ExecuterLesMethodesALaReceptionDUnMessage()
    {
        string? message = null;
        var webSocket = new ClientWebSocket();
        var executeur = new Executeur().AjouterUneActionAFaire<string>(s => message = s);
        await using var sut = new Ecouteur(webSocket, executeur);
        _ = sut.EcouterAsync();
        await Task.Delay(TimeSpan.FromSeconds(1));

        await webSocket.FakeReceiveAsync("ok");
        await Task.Delay(TimeSpan.FromMilliseconds(10));

        message.ShouldBe("ok");
    }
}