namespace CakeMachine.Fabrication.Opérations;

internal interface IMachine<in TInput, TOuput>
{
    Task<TOuput> ProduireAsync(TInput input, CancellationToken token);
    TOuput Produire(TInput input);

    int PlacesRestantes { get; }
}