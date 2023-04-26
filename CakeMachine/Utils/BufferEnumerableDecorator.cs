using System.Collections.Concurrent;

namespace CakeMachine.Utils;

internal class BufferEnumerableDecorator<T> : IAsyncEnumerable<T>
{
    private readonly IAsyncEnumerable<T> _input;
    private readonly int _bufferSize;

    public BufferEnumerableDecorator(IAsyncEnumerable<T> input, int bufferSize)
    {
        _input = input;
        _bufferSize = bufferSize;
    }

    /// <inheritdoc />
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new ())
        => new Enumerator(_input.GetAsyncEnumerator(cancellationToken), _bufferSize, cancellationToken);

    private class Enumerator : IAsyncEnumerator<T>
    {
        private readonly IAsyncEnumerator<T> _input;
        private readonly int _bufferSize;
        private readonly CancellationToken _token;
        private readonly ConcurrentBag<T> _buffer;
        private readonly Task _fillingTask;

        public Enumerator(IAsyncEnumerator<T> input, int bufferSize, CancellationToken token)
        {
            _input = input;
            _bufferSize = bufferSize;
            _token = token;
            _buffer = new ConcurrentBag<T>();
            Current = default!;
            _fillingTask = FillBufferUntilExhaustionOfSource();
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() => await _input.DisposeAsync().ConfigureAwait(false);

        private async Task FillBufferUntilExhaustionOfSource()
        {
            while(await _input.MoveNextAsync().ConfigureAwait(false))
            {
                _token.ThrowIfCancellationRequested();

                while (_buffer.Count >= _bufferSize)
                    await BufferOverflow().ConfigureAwait(false);

                _buffer.Add(_input.Current);
            }
        }

        /// <inheritdoc />
        public async ValueTask<bool> MoveNextAsync()
        {
            _token.ThrowIfCancellationRequested();

            T output;
            while(!_buffer.TryTake(out output!))
            {
                if (_fillingTask.IsCompleted) return false;
                _token.ThrowIfCancellationRequested();
                await BufferStarving().ConfigureAwait(false);
            }

            Current = output;
            return true;
        }

        private async Task BufferOverflow()
            => await Task.Delay(5, _token).ConfigureAwait(false);

        private async Task BufferStarving() 
            => await Task.Delay(10, _token).ConfigureAwait(false);

        /// <inheritdoc />
        public T Current { get; private set; }
    }
}

internal static class BufferDecoratorExtensions
{
    public static IAsyncEnumerable<T> WithBufferOfSize<T>(this IAsyncEnumerable<T> input, int bufferSize)
        => new BufferEnumerableDecorator<T>(input, bufferSize);
}