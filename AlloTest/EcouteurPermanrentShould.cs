using AlloTest.WebSocket;
using Allo;

namespace AlloTest;

public class EcouteurPermanentShould
{
    [Fact]
    public async Task Connect()
    {
        await using var sut = new EcouteurPermanent(() => new ClientWebSocket(), new NoPolitiqueDeTemporisation(), new Executeur());

        sut.State.ShouldBe(EcouteurState.Connecte);
    }

    [Fact]
    public async Task RepeatWithDelayUntilSuccess()
    {
        var temporisation = new PolitiqueDeTemporisation(TimeSpan.FromMilliseconds(5), TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(15));
        var tryConnexion = 2;

        await using var sut = new EcouteurPermanent(ClientFactory, temporisation, new Executeur());

        await Task.Delay(temporisation.TotalDelay);
        await Task.Delay(TimeSpan.FromMilliseconds(10));
        sut.State.ShouldBe(EcouteurState.Connecte);
        return;

        IClientWebSocket ClientFactory()
        {
            var client = (IClientWebSocket)(tryConnexion == 0 ? new ClientWebSocket() : new FailedClientWebSocket());
            tryConnexion--;
            return client;
        }
    }

    [Fact]
    public async Task Relancer()
    {
        await using var sut = new EcouteurPermanent(
            () => new ClientWebSocket(),
            new NoPolitiqueDeTemporisation(),
            new Executeur());
        await Task.Delay(TimeSpan.FromTicks(10));

        await sut.RelancerLEcouteAsync();
        await Task.Delay(TimeSpan.FromMilliseconds(1));

        sut.State.ShouldBe(EcouteurState.Connecte);
    }

    [Fact]
    public async Task RelancerPendantLaTemporisation()
    {
        var temporisation = new PolitiqueDeTemporisationParGroupeDeTentatives(TimeSpan.FromSeconds(20), TimeSpan.Zero);
        await using var sut = new EcouteurPermanent(() => new ClientWebSocket(), temporisation, new Executeur());
        await Task.Delay(TimeSpan.FromTicks(10));
        
        await sut.RelancerLEcouteAsync();
        
        await Task.Delay(TimeSpan.FromMilliseconds(10));
        sut.State.ShouldBe(EcouteurState.Connecte);
    }
}