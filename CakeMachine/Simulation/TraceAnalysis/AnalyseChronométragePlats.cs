using System.Text;

namespace CakeMachine.Simulation.TraceAnalysis;

internal class AnalyseChronométragePlats
{
    private readonly ChronométragePlat[] _chronométrages;

    public AnalyseChronométragePlats(IEnumerable<ChronométragePlat> chronométrages)
    {
        _chronométrages = chronométrages.ToArray();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var builder = new StringBuilder();

        var propriétésTemps = typeof(ChronométragePlat)
            .GetProperties()
            .Where(prop => prop.PropertyType == typeof(TimeSpan?));

        foreach (var propriété in propriétésTemps)
        {
            var getter = propriété.GetMethod!;

            var sommeEtDécompteNonNuls = _chronométrages.Aggregate((Time:TimeSpan.Zero, NotNullCount:0), (sum, chrono) =>
            {
                var valeurPropriété = getter.Invoke(chrono, Array.Empty<object>());
                return valeurPropriété is null 
                    ? sum 
                    : (sum.Time + (TimeSpan) valeurPropriété, sum.NotNullCount + 1);
            });

            builder.AppendLine();

            var somme = sommeEtDécompteNonNuls.Time;
            builder.AppendLine($"La somme de {propriété.Name} est {somme.TotalMilliseconds:F}ms.");

            if (somme == TimeSpan.Zero) continue;
            var moyenne = somme / sommeEtDécompteNonNuls.NotNullCount;
            builder.AppendLine($"La moyenne de {propriété.Name} est {moyenne.TotalMilliseconds:F}ms.");
        }

        return builder.ToString();
    }
}