using System.Collections;
using System.Collections.Concurrent;

namespace CakeMachine.Fabrication.Tracing;

internal class BagSink : IEnumerable<ProductionTraceStep>, ITraceSink
{
    private readonly ConcurrentBag<ProductionTraceStep> _storage = new ();

    /// <inheritdoc />
    public IEnumerator<ProductionTraceStep> GetEnumerator() => _storage.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public void RecordTrace(ProductionTraceStep step) => _storage.Add(step);
}