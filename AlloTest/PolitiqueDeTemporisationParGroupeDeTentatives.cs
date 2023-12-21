using Allo;

namespace AlloTest;

/// <summary>
/// Permet de definir une temporisation par groupe de tentatives
/// Un groupe contient l'ensembles des tentatives necessaires à une connexion reussie
/// tentative 0 ... n :  temporisation 1
/// tentative 0 ... n :  temporisation 2
///
/// Une fois utilisée la dernière temporisation fourni servira pour chaque nouvelle demande
/// </summary>
/// <param name="temporisations"></param>
public class PolitiqueDeTemporisationParGroupeDeTentatives(params TimeSpan[] temporisations) : IPolitiqueDeTemporisation
{
    private int _indexTemporisation;
    public TimeSpan GetTemporisation(int nbTentatives)
    {
        var timeSpan = temporisations.Length > _indexTemporisation
            ? temporisations[_indexTemporisation]
            : temporisations.Last();
        
        // Si c est le début d un groupe de tentatives
        // on met à jour l index  
        if(nbTentatives == 0)
            _indexTemporisation++;
        
        return timeSpan;
    }

}