using Allo;

namespace AlloTest;

public class NoPolitiqueDeTemporisation : IPolitiqueDeTemporisation
{
    public TimeSpan GetTemporisation(int nbTentatives) => TimeSpan.Zero;
}