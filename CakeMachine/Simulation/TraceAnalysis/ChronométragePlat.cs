using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Tracing;

namespace CakeMachine.Simulation.TraceAnalysis;

internal class ChronométragePlat
{
    // Attention les getters sont appelés par réflection
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public TimeSpan? TempsAvantPréparation { get; }
    public TimeSpan? TempsPréparation { get; }
    public TimeSpan? TempsEntreFinPréparationEtDébutCuisson { get; }
    public TimeSpan? TempsCuisson { get; }
    public TimeSpan? TempsEntreFinCuissonEtDébutEmballage { get; }
    public TimeSpan? TempsEmballage { get; }
    public TimeSpan? TempsEntreFinEmballageEtLivraison { get; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore MemberCanBePrivate.Global

    public ChronométragePlat(IGrouping<Plat, ProductionTraceStep> steps)
    {
        if(steps.Any(step => step.Objet.PlatSousJacent != steps.Key))
            throw new InvalidOperationException(
                "Le chronométrage ne peut se faire qu'avec les étapes successives d'un même plat !");
        
        var temps = steps.ToDictionary(step => step.Step, step => step.Horodatage);

        if(!temps.ContainsKey(EtapeProduction.DebutPréparation)) return;
        TempsAvantPréparation = temps[EtapeProduction.DebutPréparation] - temps[EtapeProduction.ProductionPlat];

        if (!temps.ContainsKey(EtapeProduction.FinPréparation)) return;
        TempsPréparation = temps[EtapeProduction.FinPréparation] - temps[EtapeProduction.DebutPréparation];

        if (!temps.ContainsKey(EtapeProduction.DébutCuisson)) return;
        TempsEntreFinPréparationEtDébutCuisson = temps[EtapeProduction.DébutCuisson] - temps[EtapeProduction.FinPréparation];

        if (!temps.ContainsKey(EtapeProduction.FinCuisson)) return;
        TempsCuisson = temps[EtapeProduction.FinCuisson] - temps[EtapeProduction.DébutCuisson];

        if (!temps.ContainsKey(EtapeProduction.DébutEmballage)) return;
        TempsEntreFinCuissonEtDébutEmballage = temps[EtapeProduction.DébutEmballage] - temps[EtapeProduction.FinCuisson];

        if (!temps.ContainsKey(EtapeProduction.FinEmballage)) return;
        TempsEmballage = temps[EtapeProduction.FinEmballage] - temps[EtapeProduction.DébutEmballage];

        if (!temps.ContainsKey(EtapeProduction.Livraison)) return;
        TempsEntreFinEmballageEtLivraison = temps[EtapeProduction.Livraison] - temps[EtapeProduction.FinEmballage];
    }
}