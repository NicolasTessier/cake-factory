using CakeMachine.Fabrication.Opérations;

namespace CakeMachine.Utils;

internal class MachinePool<TInput, TOuput> : IMachine<TInput, TOuput>
{
    private readonly SemaphoreSlim _machineAllocationSemaphore;
    private readonly IEnumerable<IMachine<TInput, TOuput>> _pooledMachines;
    private readonly Ring<IMachine<TInput, TOuput>> _machineToUseIfNoAlternative;

    public MachinePool(IEnumerable<IMachine<TInput, TOuput>> pooledMachines)
    {
        _pooledMachines = pooledMachines;
        _machineAllocationSemaphore = new SemaphoreSlim(1);
        _machineToUseIfNoAlternative = new Ring<IMachine<TInput, TOuput>>(_pooledMachines);
    }

    /// <inheritdoc />
    public async Task<TOuput> ProduireAsync(TInput input, CancellationToken token)
    {
        await _machineAllocationSemaphore.WaitAsync(token).ConfigureAwait(false);

        Task<TOuput> tâcheProduction;

        try
        {
            var machineSélectionnée = 
                _pooledMachines.FirstOrDefault(machine => machine.PlacesRestantes > 1)
                    ?? _machineToUseIfNoAlternative.Next;

            tâcheProduction = machineSélectionnée.ProduireAsync(input, token);
        }
        finally
        {
            _machineAllocationSemaphore.Release();
        }

        return await tâcheProduction.ConfigureAwait(false);
    }

    /// <inheritdoc />
    public TOuput Produire(TInput input) => throw new NotSupportedException();

    /// <inheritdoc />
    public int PlacesRestantes => _pooledMachines.Sum(machine => machine.PlacesRestantes);
}

internal static class MachinePoolExtensions
{
    public static IMachine<TInput, TOuput> PoolTogether<TInput, TOuput>(
        this IEnumerable<IMachine<TInput, TOuput>> machines)
    {
        return new MachinePool<TInput, TOuput>(machines);
    }
}