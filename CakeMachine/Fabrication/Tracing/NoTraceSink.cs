namespace CakeMachine.Fabrication.Tracing;

internal class NoTraceSink : ITraceSink
{
    /// <inheritdoc />
    public void RecordTrace(ProductionTraceStep step) { }
}