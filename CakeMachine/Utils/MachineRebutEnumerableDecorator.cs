using CakeMachine.Fabrication.Elements;

namespace CakeMachine.Utils;

internal class FiltrerRebutEnumerable<T> : IAsyncEnumerable<T> 
    where T : IConforme
{
    private readonly IAsyncEnumerable<T> _input;
    private readonly Action<IConforme> _miseAuRebut;

    public FiltrerRebutEnumerable(IAsyncEnumerable<T> input, Action<IConforme> miseAuRebut)
    {
        _input = input;
        _miseAuRebut = miseAuRebut;
    }

    private class Enumerator : IAsyncEnumerator<T>
    {
        private readonly IAsyncEnumerator<T> _input;
        private readonly CancellationToken _token;
        private readonly Action<IConforme> _miseAuRebut;

        public Enumerator(IAsyncEnumerator<T> input, Action<IConforme> miseAuRebut, CancellationToken token)
        {
            _input = input;
            _token = token;
            _miseAuRebut = miseAuRebut;
            Current = default!;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() => await _input.DisposeAsync().ConfigureAwait(false);

        /// <inheritdoc />
        public async ValueTask<bool> MoveNextAsync()
        {
            _token.ThrowIfCancellationRequested();
            var inputHasElements = await _input.MoveNextAsync().ConfigureAwait(false);
            if (!inputHasElements) return false;

            var currentInput = _input.Current;

            if (!currentInput.EstConforme)
            {
                _miseAuRebut(currentInput);
                return await MoveNextAsync().ConfigureAwait(false);
            }

            Current = currentInput;
            return true;
        }

        /// <inheritdoc />
        public T Current { get; private set; }
    }

    /// <inheritdoc />
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new ())
        => new Enumerator(_input.GetAsyncEnumerator(cancellationToken), _miseAuRebut, cancellationToken);
}

internal static class FiltrerRebutExtensions
{
    public static IAsyncEnumerable<T> FiltrerRebut<T>(this IAsyncEnumerable<T> input, Action<IConforme[]> miseAuRebut)
        where T : IConforme
        => new FiltrerRebutEnumerable<T>(input, rebut => miseAuRebut(new []{ rebut }));
}