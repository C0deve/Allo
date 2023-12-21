using Allo;

namespace AlloTest;

public class ExecuteurShould
{

    [Fact]
    public void AjouterDesActionsAFaire()
    {
        var executeur = new Executeur()
            .AjouterUneActionAFaire<string>(Console.WriteLine)
            .AjouterUneActionAFaire<int>(Console.WriteLine);
        
        executeur.ActionsAFaireCount.ShouldBe(2);
    }
    
    [Fact]
    public void ExecuterUneActionSiLeParametreFourniCorrespondAuParametreAttenduParUneDesActions()
    {
        var list = new List<string>();

        new Executeur()
            .AjouterUneActionAFaire<string>(AddToList)
            .Execute("coucou")
            .Execute("toi");
        
        list.ShouldBe(new List<string>{"coucou", "toi"});

        return;

        void AddToList(string s) => list.Add(s);
    }
    
    [Fact]
    public void NeRienFaireSiLeParametreNeCorrespondAAucuneAction()
    {
        var list = new List<string>();

        new Executeur()
            .AjouterUneActionAFaire<string>(AddToList)
            .Execute(1);
        
        list.ShouldBeEmpty();

        return;

        void AddToList(string s) => list.Add(s);
    }
}