namespace Allo;

public interface IPolitiqueDeTemporisation
{
    TimeSpan GetTemporisation(int nbTentatives);
}