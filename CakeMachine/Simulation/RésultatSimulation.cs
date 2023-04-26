using System.Text;
using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Tracing;
using CakeMachine.Simulation.Algorithmes;
using CakeMachine.Simulation.TraceAnalysis;

namespace CakeMachine.Simulation;

internal record RésultatSimulation(
        Algorithme Algorithme,
        bool IsSync,
        TimeSpan Temps,
        IReadOnlyDictionary<DestinationPlat, uint> DestinationPlats,
        IEnumerable<ProductionTraceStep> Trace)
    : IComparable<RésultatSimulation>
{
    public int CompareTo(RésultatSimulation? other)
    {
        if (other is null) return -1;

        var livrésConformes = DestinationPlats[DestinationPlat.LivréConforme];
        var livrésConformesAutre = other.DestinationPlats[DestinationPlat.LivréConforme];

        if (livrésConformesAutre != livrésConformes) return (int) (livrésConformesAutre - livrésConformes);

        return Convert.ToInt32(Math.Round((other.Temps - Temps).TotalSeconds));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder($"L'algorithme {Algorithme} a produit ");
        builder.Append($"{DestinationPlats[DestinationPlat.LivréConforme]} gâteaux livrés conformes, ");
        builder.Append($"mis {DestinationPlats[DestinationPlat.Rebut] + DestinationPlats[DestinationPlat.LivréNonConforme]} éléments au rebut, ");
        builder.Append($"perdu {DestinationPlats[DestinationPlat.Perdu]} plats lors du traitement ");
        builder.Append($"en {Temps.TotalSeconds:F}s");

        builder.AppendLine();
        var chronométrages = Trace
            .GroupBy(trace => trace.Objet.PlatSousJacent)
            .Select(groupe => new ChronométragePlat(groupe));
        builder.Append(new AnalyseChronométragePlats(chronométrages));

        return builder.ToString();
    }
}