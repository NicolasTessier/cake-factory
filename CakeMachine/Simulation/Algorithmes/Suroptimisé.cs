using System.Runtime.CompilerServices;
using CakeMachine.Fabrication.ContexteProduction;
using CakeMachine.Fabrication.Elements;

namespace CakeMachine.Simulation.Algorithmes;

internal class Suroptimisé : Algorithme
{
    /// <inheritdoc />
    public override void ConfigurerUsine(IConfigurationUsine builder)
    {
        base.ConfigurerUsine(builder);

        builder.NombrePréparateurs = 2;
    }

    /// <inheritdoc />
    public override bool SupportsAsync => true;

    /// <inheritdoc />
    public override async IAsyncEnumerable<GâteauEmballé> ProduireAsync(
        Usine usine,
        [EnumeratorCancellation] CancellationToken token)
    {
        var postesPréparation = usine.Préparateurs.ToArray();
        var posteCuisson = usine.Fours.Single();
        var posteEmballage = usine.Emballeuses.Single();

        while (!token.IsCancellationRequested)
        {
            var platsPréparateur1 = usine.StockInfiniPlats.Take(2);
            var platsPréparateur2 = usine.StockInfiniPlats.Take(2);

            var gâteauxCrusPrep1 = await Task.WhenAll(platsPréparateur1.Select(postesPréparation.First().PréparerAsync));
            var gâteauxCrusPrep2 = await Task.WhenAll(platsPréparateur2.Select(postesPréparation.Last().PréparerAsync));
            var gâteauxCrus = gâteauxCrusPrep1.Concat(gâteauxCrusPrep2);

            var gâteauxCuits = await posteCuisson.CuireAsync(gâteauxCrus.ToArray());
            var gâteauxEmballés = await Task.WhenAll(gâteauxCuits.Select(posteEmballage.EmballerAsync));

            foreach (var gâteauEmballé in gâteauxEmballés)
                yield return gâteauEmballé;
        }
    }
}