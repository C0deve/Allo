using System.Net.WebSockets;

namespace Allo;

/// <summary>
/// Etablie une connexion durable avec un serveur et
/// execute les méthodes fournies (callback) à la reception des notifications emises par le serveur.
/// </summary>
/// Gere la reconnexion automatique au serveur avec une politique d'attente entre deux tentatives
/// Garantie la deconnexion et la liberation des ressources
public sealed class EcouteurPermanent : IAsyncDisposable
{
    private readonly Func<IClientWebSocket> _clientWebSocketFactory;
    private readonly IPolitiqueDeTemporisation _politiqueDeTemporisation;
    private CancellationTokenSource? _temporisationCancellationTokenSource;
    private readonly Executeur _executeur;
    private readonly CancellationTokenSource _mainCancellationTokenSource = new();
    private readonly Task _taskBouclePrincipale;
    private Ecouteur? _ecouteur;
    public EcouteurState State => _ecouteur?.State == WebSocketState.Open ? EcouteurState.Connecte : EcouteurState.Deconnecte;

    public EcouteurPermanent(Func<IClientWebSocket> clientWebSocketFactory, IPolitiqueDeTemporisation politiqueDeTemporisation, Executeur executeur)
    {
        _clientWebSocketFactory = clientWebSocketFactory;
        _politiqueDeTemporisation = politiqueDeTemporisation;
        _executeur = executeur;
        _taskBouclePrincipale = MainAsync(_mainCancellationTokenSource.Token);
    }

    private async Task MainAsync(CancellationToken mainCancellationToken)
    {
        var nbTentatives = 0;
        while (!mainCancellationToken.IsCancellationRequested)
        {
            using var temporisationCancellationTokenSource = new CancellationTokenSource();
            _temporisationCancellationTokenSource = temporisationCancellationTokenSource;

            using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(temporisationCancellationTokenSource.Token, mainCancellationToken);
            await TemporiseEntreDeuxTentativesAsync(nbTentatives, linkedTokenSource.Token);

            await using var ecouteur = new Ecouteur(_clientWebSocketFactory(), _executeur);
            _ecouteur = ecouteur;
            nbTentatives = await ecouteur.EcouterAsync()
                ? 0
                : nbTentatives + 1;
        }
    }

    private async Task TemporiseEntreDeuxTentativesAsync(int nbTentatives, CancellationToken linkedToken)
    {
        try
        {
            await Task.Delay(_politiqueDeTemporisation.GetTemporisation(nbTentatives), linkedToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async Task RelancerLEcouteAsync()
    {
        if (_temporisationCancellationTokenSource != null)
            await _temporisationCancellationTokenSource.CancelAsync();

        if (_ecouteur != null)
            await _ecouteur.DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (!_mainCancellationTokenSource.IsCancellationRequested)
            await _mainCancellationTokenSource.CancelAsync().ConfigureAwait(false);

        if (_ecouteur != null)
            await _ecouteur.DisposeAsync().ConfigureAwait(false);

        await _taskBouclePrincipale.ConfigureAwait(false);
        _mainCancellationTokenSource.Dispose();
        _temporisationCancellationTokenSource?.Dispose();
    }
}