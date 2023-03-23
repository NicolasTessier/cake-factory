using CakeMachine.Fabrication.ContexteProduction;
using CakeMachine.Fabrication.Elements;

namespace CakeMachine.Simulation.Algorithmes;

internal class AlgoPremiersPas : Algorithme
{
    public override bool SupportsSync => true;
    
    public override IEnumerable<GâteauEmballé> Produire(Usine usine, CancellationToken token)
    {
        var postePréparation = usine.Préparateurs.Single();
        var posteCuisson = usine.Fours.Single();
        var posteEmballage = usine.Emballeuses.Single();

        while (!token.IsCancellationRequested)
        {
            var plats = usine.StockInfiniPlats.Take(2);

            var gâteauxCrus = plats
                .Select(postePréparation.Préparer)
                .AsParallel();

            var gâteauxCuits = posteCuisson.Cuire(gâteauxCrus.ToArray());

            var gâteauxEmballés = gâteauxCuits
                .Select(posteEmballage.Emballer)
                .AsParallel();

            foreach (var gâteauEmballé in gâteauxEmballés)
                yield return gâteauEmballé;
        }
    }

}