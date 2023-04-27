using System.Reflection;
using CakeMachine.Simulation.Algorithmes;

namespace CakeMachine.Simulation;

internal class MultipleAlgorithmsRunner
{
    private readonly IEnumerable<SingleAlgorithmRunner> _runners;

    public MultipleAlgorithmsRunner()
    {
        _runners = Assembly.GetExecutingAssembly().GetTypes()
            .Where(type => type.BaseType == typeof(Algorithme))
            .Where(type => type.Name is "UsineNicoRemi" or "UsineEtalon")
            .Select(algorithm => new SingleAlgorithmRunner(algorithm));
    }

    public async Task ProduirePendant(TimeSpan timeSpan)
    {
        var résultats = new List<RésultatSimulation>();

        foreach (var runner in _runners)
        {
            var sync = await runner.ProduirePendantAsync(timeSpan, true).ConfigureAwait(false);
            var async = await runner.ProduirePendantAsync(timeSpan, false).ConfigureAwait(false);

            if (async is not null) résultats.Add(async);
            if (sync is not null) résultats.Add(sync);
        }

        foreach (var résultat in résultats.OrderByDescending(r => r)) Console.WriteLine(résultat);
    }

    public async Task ProduireNGâteaux(uint nombreGâteaux)
    {
        var résultats = new List<RésultatSimulation>();

        foreach (var runner in _runners)
        {
            var sync = await runner.ProduireNGâteauxAsync(nombreGâteaux, true).ConfigureAwait(false);
            var async = await runner.ProduireNGâteauxAsync(nombreGâteaux, false).ConfigureAwait(false);

            if (async is not null) résultats.Add(async);
            if (sync is not null) résultats.Add(sync);
        }

        foreach (var résultat in résultats.OrderByDescending(r => r)) Console.WriteLine(résultat);
    }
}