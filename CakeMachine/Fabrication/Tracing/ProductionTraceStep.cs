using CakeMachine.Fabrication.Elements;

namespace CakeMachine.Fabrication.Tracing;

internal record ProductionTraceStep(IConforme Objet, EtapeProduction Step, DateTime Horodatage);