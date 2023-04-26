namespace CakeMachine.Utils;

internal class BatchToSingleEnumerable<T> : IAsyncEnumerable<T>
{
    private readonly IAsyncEnumerable<T[]> _batchProducer;

    public BatchToSingleEnumerable(IAsyncEnumerable<T[]> batchProducer)
    {
        _batchProducer = batchProducer;
    }

    /// <inheritdoc />
    public IAsyncEnumerator<T> GetAsyncEnumerator(
        CancellationToken cancellationToken = new())
        => new Enumerator(_batchProducer.GetAsyncEnumerator(cancellationToken), cancellationToken);

    private class Enumerator : IAsyncEnumerator<T>
    {
        private readonly IAsyncEnumerator<T[]> _producer;
        private readonly CancellationToken _token;
        private Queue<T> _queueOfCurrentBatch;

        public Enumerator(IAsyncEnumerator<T[]> producer, CancellationToken token)
        {
            _producer = producer;
            _token = token;
            _queueOfCurrentBatch = new Queue<T>();
            Current = default!;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() => await _producer.DisposeAsync();

        /// <inheritdoc />
        public async ValueTask<bool> MoveNextAsync()
        {
            _token.ThrowIfCancellationRequested();

            if (!_queueOfCurrentBatch.Any())
            {
                var producerHasElements = await _producer.MoveNextAsync().ConfigureAwait(false);
                if (!producerHasElements) return false;

                _queueOfCurrentBatch = new Queue<T>(_producer.Current);
                if (!_queueOfCurrentBatch.Any()) return false;
            }
            
            Current = _queueOfCurrentBatch.Dequeue();
            return true;
        }

        /// <inheritdoc />
        public T Current { get; private set; }
    }
}

internal static class BatchToSingleExtensions
{
    public static IAsyncEnumerable<TProduction> EclaterBains<TProduction>(this IAsyncEnumerable<TProduction[]> producer)
        => new BatchToSingleEnumerable<TProduction>(producer);
}