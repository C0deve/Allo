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
    private CancellationTokenSource? _delayCancellationTokenSource;
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
            using var delayCancellationTokenSource = new CancellationTokenSource();
            _delayCancellationTokenSource = delayCancellationTokenSource;

            using var createLinkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(delayCancellationTokenSource.Token, mainCancellationToken);
            await TemporiseEntreDeuxTentativesAsync(nbTentatives, createLinkedTokenSource);

            await using var ecouteur = new Ecouteur(_clientWebSocketFactory(), _executeur);
            _ecouteur = ecouteur;
            nbTentatives = await ecouteur.EcouterAsync()
                ? 0
                : nbTentatives + 1;
        }
    }

    private async Task TemporiseEntreDeuxTentativesAsync(int nbTentatives, CancellationTokenSource linkedTokenSource)
    {
        try
        {
            await Task.Delay(_politiqueDeTemporisation.GetTemporisation(nbTentatives), linkedTokenSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public async Task RelancerLEcouteAsync()
    {
        if (_delayCancellationTokenSource != null)
            await _delayCancellationTokenSource.CancelAsync();

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
        _delayCancellationTokenSource?.Dispose();
    }
}