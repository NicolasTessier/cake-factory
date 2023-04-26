using CakeMachine.Fabrication.Opérations;

namespace CakeMachine.Utils;

internal class MachineEnumerable<TInput, TOuput> : IAsyncEnumerable<TOuput>
{
    private readonly IMachine<TInput, TOuput> _machine;
    private readonly IAsyncEnumerable<TInput> _inputGenerator;

    public MachineEnumerable(
        IMachine<TInput, TOuput> machine, 
        IAsyncEnumerable<TInput> inputGenerator)
    {
        _machine = machine;
        _inputGenerator = inputGenerator;
    }

    /// <inheritdoc />
    public IAsyncEnumerator<TOuput> GetAsyncEnumerator(
        CancellationToken cancellationToken = new ())
        => new Enumerator(_machine, _inputGenerator.GetAsyncEnumerator(cancellationToken), cancellationToken);

    private class Enumerator : IAsyncEnumerator<TOuput>
    {
        private readonly IMachine<TInput, TOuput> _machine;
        private readonly IAsyncEnumerator<TInput> _inputEnumerator;
        private readonly CancellationToken _token;

        public Enumerator(
            IMachine<TInput, TOuput> machine, 
            IAsyncEnumerator<TInput> inputEnumerator, 
            CancellationToken token)
        {
            _machine = machine;
            _inputEnumerator = inputEnumerator;
            _token = token;
            Current = default!;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync() 
            => await _inputEnumerator.DisposeAsync().ConfigureAwait(false);

        /// <inheritdoc />
        public async ValueTask<bool> MoveNextAsync()
        {
            _token.ThrowIfCancellationRequested();
            
            var inputResult = await _inputEnumerator.MoveNextAsync().ConfigureAwait(false);
            if (!inputResult) return false;

            var input = _inputEnumerator.Current;
            
            Current = await _machine.ProduireAsync(input, _token).ConfigureAwait(false);
            return true;
        }

        /// <inheritdoc />
        public TOuput Current { get; private set; }
    }
}