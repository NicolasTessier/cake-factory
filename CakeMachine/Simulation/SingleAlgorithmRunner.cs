using System.Diagnostics;
using CakeMachine.Fabrication.ContexteProduction;
using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Tracing;
using CakeMachine.Simulation.Algorithmes;

namespace CakeMachine.Simulation;

internal class SingleAlgorithmRunner
{
    private readonly Algorithme _algorithme;

    private SingleAlgorithmRunner(Algorithme algorithme)
    {
        _algorithme = algorithme;
    }

    public SingleAlgorithmRunner(Type type) 
        : this((Algorithme) Activator.CreateInstance(type)!)
    {
    }

    private bool AlgorithmSupports(bool sync) => sync ? _algorithme.SupportsSync : _algorithme.SupportsAsync;

    public async Task<RésultatSimulation?> ProduirePendantAsync(TimeSpan timeSpan, bool syncAlgorithm)
        => AlgorithmSupports(syncAlgorithm) 
            ? await Produire(tuple => tuple.TempsÉcoulé >= timeSpan, syncAlgorithm).ConfigureAwait(false)
            : null;

    public async Task<RésultatSimulation?> ProduireNGâteauxAsync(uint nombreGâteaux, bool syncAlgorithm)
        => AlgorithmSupports(syncAlgorithm) 
            ? await Produire(tuple => tuple.GâteauxValidesProduits >= nombreGâteaux, syncAlgorithm).ConfigureAwait(false)
            : null;

    private async Task<RésultatSimulation> Produire(
        Predicate<(TimeSpan TempsÉcoulé, uint GâteauxValidesProduits)> conditionSortie, bool sync)
    {
        var stopWatch = new Stopwatch();

        var traceSink = new BagSink();
        var builder = new UsineBuilder
        {
            TraceSink = traceSink
        };

        _algorithme.ConfigurerUsine(builder);
        var usine = builder.Build();

        uint gâteauxConformes = 0;
        var gâteauxProduits = new List<GâteauEmballé>();

        var tokenSource = new CancellationTokenSource();

        bool PeutContinuer() => !conditionSortie((stopWatch.Elapsed, gâteauxConformes));

        if(sync)
        {
            using var producteur = _algorithme.Produire(usine, tokenSource.Token).GetEnumerator();

            stopWatch.Start();
                
            while (PeutContinuer())
            {
                if (!producteur.MoveNext())
                    throw new InvalidOperationException(
                        $"L'algorithme {_algorithme} n'a pas été capable de produire suffisamment de gâteaux.");

                Debug.Assert(producteur.Current != null, "producteur.Current != null");
                var gâteau = producteur.Current;

                if (!PeutContinuer()) break;
                traceSink.RecordTrace(new ProductionTraceStep(gâteau, EtapeProduction.Livraison, DateTime.Now));
                gâteauxProduits.Add(producteur.Current);
                if (gâteau.EstConforme) gâteauxConformes++;
            }
        } 
        else
        {
            await using var producteur = _algorithme
                .ProduireAsync(usine, tokenSource.Token)
                .GetAsyncEnumerator(tokenSource.Token);

            stopWatch.Start();

            while (PeutContinuer())
            {
                if (!await producteur.MoveNextAsync().ConfigureAwait(false))
                    throw new InvalidOperationException(
                        $"L'algorithme {_algorithme} n'a pas été capable de produire suffisamment de gâteaux.");

                var gâteau = producteur.Current;

                if (!PeutContinuer()) break;
                traceSink.RecordTrace(new ProductionTraceStep(gâteau, EtapeProduction.Livraison, DateTime.Now));
                gâteauxProduits.Add(gâteau);
                if (gâteau.EstConforme) gâteauxConformes++;
            }
        }

        stopWatch.Stop();
        tokenSource.Cancel();

        var destinationPlats = usine.DestinationPlats(gâteauxProduits);

        if (destinationPlats[DestinationPlat.RebutMaisConforme] != 0)
            throw new InvalidOperationException("Vous avez mis au rebut un produit conforme.");

        if (destinationPlats[DestinationPlat.Inconnu] != 0)
            throw new InvalidOperationException("Vous avez tenté de créer un plat. Vous devez utiliser les plats du stock de l'usine.");

        if (destinationPlats[DestinationPlat.RéutiliséFrauduleusement] != 0)
            throw new InvalidOperationException("Vous avez tenté de réutiliser un plat, repréparer un gâteau raté, " +
                                                "recuire un gâteau mal cuit ou réemballer un gâteau mal emballé. C'est interdit.");

        return new RésultatSimulation(_algorithme, true, stopWatch.Elapsed, destinationPlats, traceSink);
    }
}