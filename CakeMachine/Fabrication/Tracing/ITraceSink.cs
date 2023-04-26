namespace CakeMachine.Fabrication.Tracing;

internal interface ITraceSink
{
    void RecordTrace(ProductionTraceStep step);
}