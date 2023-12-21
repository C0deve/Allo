using System.Net.WebSockets;
using System.Text;

namespace Allo;

/// <summary>
/// Ecoute les notifications emises par un serveur au travers d'une connexion webSocket et
/// execute les methodes fournies
/// </summary>
/// Garanti la deconnexion et la liberation des ressources
/// <param name="clientWebSocket"></param>
/// <param name="executeur"></param>
internal sealed class Ecouteur(IClientWebSocket clientWebSocket, Executeur executeur) : IAsyncDisposable
{
    public WebSocketState State => clientWebSocket.State;

    public async Task<bool> EcouterAsync()
    {
        var success = false;
        try
        {
            await clientWebSocket.ConnecterAsync(CancellationToken.None).ConfigureAwait(false);
            success = true;
            await EcouterTousLesMessagesAsync(clientWebSocket, executeur).ConfigureAwait(false);
        }
        catch (WebSocketException e)
        {
            Console.WriteLine(e.Message);
        }

        return success;
    }

    private static async Task EcouterTousLesMessagesAsync(IClientWebSocket clientWebSocket1, Executeur executeur)
    {
        while (clientWebSocket1.State == WebSocketState.Open)
        {
            var message = await EcouterUnMessage(clientWebSocket1);
            executeur.Execute(message);
        }
    }

    private static async Task<string> EcouterUnMessage(IClientWebSocket clientWebSocket1)
    {
        var buffer = new ArraySegment<byte>(new byte[8192]);
        WebSocketReceiveResult? result;
        using var ms = new MemoryStream();

        do
        {
            result = await clientWebSocket1.ReceiveAsync(buffer, CancellationToken.None);
            if (buffer.Array != null)
                ms.Write(buffer.Array, buffer.Offset, result.Count);
        } while (!result.EndOfMessage);

        ms.Seek(0, SeekOrigin.Begin);

        if (result.MessageType != WebSocketMessageType.Text) return "";

        using var reader = new StreamReader(ms, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if(clientWebSocket.State == WebSocketState.Open)
            await clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        
        clientWebSocket.Dispose();
    }
}