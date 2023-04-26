namespace CakeMachine.Utils;

internal class SingleToBatchEnumerable<T> : IAsyncEnumerable<T[]>
{
    private readonly IAsyncEnumerable<T> _producer;
    private readonly int _batchSize;

    public SingleToBatchEnumerable(IAsyncEnumerable<T> producer, int batchSize)
    {
        _producer = producer;
        _batchSize = batchSize;
    }

    /// <inheritdoc />
    public IAsyncEnumerator<T[]> GetAsyncEnumerator(
        CancellationToken cancellationToken = new())
        => new Enumerator(_producer.GetAsyncEnumerator(cancellationToken), _batchSize, cancellationToken);

    private class Enumerator : IAsyncEnumerator<T[]>
    {
        private readonly IAsyncEnumerator<T> _producer;
        private readonly int _batchSize;
        private readonly CancellationToken _token;

        public Enumerator(IAsyncEnumerator<T> producer, int batchSize, CancellationToken token)
        {
            _producer = producer;
            _batchSize = batchSize;
            _token = token;
            Current = default!;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() => await _producer.DisposeAsync().ConfigureAwait(false);

        /// <inheritdoc />
        public async ValueTask<bool> MoveNextAsync()
        {
            var batchBuffer = new List<T>();

            for (var i = 0; i < _batchSize; i++)
            {
                _token.ThrowIfCancellationRequested();
                var producerHasElement = await _producer.MoveNextAsync().ConfigureAwait(false);
                if(!producerHasElement) break;

                batchBuffer.Add(_producer.Current);
            }

            Current = batchBuffer.ToArray();
            return batchBuffer.Any();
        }

        /// <inheritdoc />
        public T[] Current { get; private set; }
    }
}

internal static class SingleToBatchExtensions
{
    public static IAsyncEnumerable<TProduction[]> RassemblerParBains<TProduction>(
        this IAsyncEnumerable<TProduction> inputProducer, int tailleBains)
        => new SingleToBatchEnumerable<TProduction>(inputProducer, tailleBains);
}