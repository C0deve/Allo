using Allo;

namespace SampleConsoleApp;

public class ZeroPolitiqueDeTemporisation : IPolitiqueDeTemporisation
{
    public TimeSpan GetTemporisation(int nbTentatives) => TimeSpan.FromSeconds(1);
}