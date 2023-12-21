using Allo;

namespace AlloTest;

/// <summary>
/// Permet de definir le delai retourné en foncion du nombre de tentative
/// Tentative 0 : delai 0
/// Tentative 1 : delai 1
///
/// Une fois utilisé le dernier politiqueDeTemporisation fournit servira pour chaque nouvelle demande
/// </summary>
/// <param name="delais"></param>
public class PolitiqueDeTemporisation(params TimeSpan[] delais) : IPolitiqueDeTemporisation
{
    public TimeSpan GetTemporisation(int nbTentatives) =>
        delais.Length > nbTentatives
            ? delais[nbTentatives]
            : delais.Last();

    public TimeSpan TotalDelay =>
        delais.Aggregate(
            TimeSpan.Zero,
            (sumSoFar, next) => sumSoFar + next);
}