using System.Net.WebSockets;
using SampleConsoleApp;
using Allo;

var count = 0;
var random = new Random();
var semaphore = new SemaphoreSlim(3);
var executeur = new Executeur()
    .AjouterUneActionAFaire<string>(message => LongRunningTask(message, count++));

while (true)
{
    Console.WriteLine("[E]xit or [S]tart");
    var message = Console.ReadLine();

    if (message == "e")
    {
        Console.WriteLine("bye bye");
        break;
    }

    if (message == "s")
    {
        await Start();
    }
}

return;

async Task LongRunningTask(string s, int id)
{
    Console.WriteLine($"n°{id} wait for starting");

    int semaphoreCount;
    await semaphore.WaitAsync();
    try
    {
        var seconds = random.Next(4, 15);
        Console.WriteLine($"started {s} n°{id} for {seconds}s");
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        Console.WriteLine($"Finished {s} {id}");
    }
    finally
    {
        semaphoreCount = semaphore.Release();
    }

    Console.WriteLine("Task {0} releases the semaphore; previous count: {1}.",
        Task.CurrentId, semaphoreCount);
}

async Task Start()
{
    await using var notifieur = new Allo.EcouteurPermanent(
        () => new ClientWebSocketWrapper(new ClientWebSocket()),
        new ZeroPolitiqueDeTemporisation(), 
        executeur);

    Console.WriteLine("Connecting to server");

    Console.WriteLine("[D]econnexion or [R]econnexion");

    while (true)
    {
        var message = Console.ReadLine();

        if (message == "d")
        {
            Console.WriteLine("DisConnected to server");
            break;
        }

        if (message == "r")
        {
            await notifieur.RelancerLEcouteAsync();
        }
    }
}