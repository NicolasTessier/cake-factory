using System.Collections.Concurrent;
using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Opérations;
using CakeMachine.Fabrication.Paramètres;
using CakeMachine.Fabrication.Tracing;
using CakeMachine.Utils;

namespace CakeMachine.Fabrication.ContexteProduction;

internal class Usine : IUsine
{
    private readonly ITraceSink _traceSink;
    private readonly ConcurrentBag<Plat> _platsCréés = new();
    private readonly ConcurrentBag<IConforme> _rebut = new();

    public ParamètresUsine OrganisationUsine { get; }
    public const ushort TailleMaxUsine = 35;

    public IEnumerable<Préparation> Préparateurs { get; }
    public IEnumerable<Cuisson> Fours { get; }
    public IEnumerable<Emballage> Emballeuses { get; }

    public Usine(ThreadSafeRandomNumberGenerator rng, ParamètresUsine organisationUsine, ITraceSink traceSink)
    {
        if (organisationUsine.NombrePréparateurs + organisationUsine.NombreEmballeuses + organisationUsine.NombreFours > TailleMaxUsine)
            throw new InvalidOperationException(
                $"L'usine est pleine, elle peut compter au maximum {TailleMaxUsine} machines");
        _traceSink = traceSink;
        OrganisationUsine = organisationUsine;

        Préparateurs = Enumerable
            .Range(0, organisationUsine.NombrePréparateurs)
            .Select(_ => new Préparation(rng, organisationUsine.ParamètresPréparation, traceSink));

        Fours = Enumerable
            .Range(0, organisationUsine.NombreFours)
            .Select(_ => new Cuisson(rng, organisationUsine.ParamètresCuisson, traceSink));

        Emballeuses = Enumerable
            .Range(0, organisationUsine.NombreEmballeuses)
            .Select(_ => new Emballage(rng, organisationUsine.ParamètresEmballage, traceSink));
    }

    public IEnumerable<Plat> StockInfiniPlats 
    { 
        get 
        {
            while (true)
            {
                var plat = new Plat();
                _platsCréés.Add(plat);
                _traceSink.RecordTrace(new ProductionTraceStep(plat, EtapeProduction.ProductionPlat, DateTime.Now));
                yield return plat;
            }
            // ReSharper disable once IteratorNeverReturns
        }
    }

    public IReadOnlyDictionary<DestinationPlat, uint> DestinationPlats(IEnumerable<GâteauEmballé> gâteauxEmballésReçus)
    {
        var dictionary = new Dictionary<DestinationPlat, uint>
        {
            { DestinationPlat.Inconnu, 0 },
            { DestinationPlat.LivréConforme, 0 },
            { DestinationPlat.LivréNonConforme, 0 },
            { DestinationPlat.Rebut, 0 }, 
            { DestinationPlat.RebutMaisConforme, 0 }, 
            { DestinationPlat.Perdu, 0 }, 
            { DestinationPlat.RéutiliséFrauduleusement, 0 }
        };

        var platsCréés = new HashSet<Plat>(_platsCréés);
        var rebut = new HashSet<IConforme>(_rebut);

        var gâteauxEmballésArray = gâteauxEmballésReçus.ToArray();
        var platsArrivésEnBoutDeChaîne = new HashSet<Plat>(gâteauxEmballésArray.Select(gâteau => gâteau.PlatSousJacent));

        foreach (var gâteauEmballé in gâteauxEmballésArray)
        {
            if(!platsCréés.Contains(gâteauEmballé.PlatSousJacent)) 
                dictionary[DestinationPlat.Inconnu] ++;
            else if (!gâteauEmballé.EstConforme)
                dictionary[DestinationPlat.LivréNonConforme]++;
            else if(rebut.Contains(gâteauEmballé))
                dictionary[DestinationPlat.RéutiliséFrauduleusement]++;
            else dictionary[DestinationPlat.LivréConforme]++;
        }

        foreach (var platCréé in platsCréés.Where(platCréé => !platsArrivésEnBoutDeChaîne.Contains(platCréé)))
        {
            var élémentTrouvéAuRebut = rebut.SingleOrDefault(element => element.PlatSousJacent == platCréé.PlatSousJacent);
            if (élémentTrouvéAuRebut != null)
            {
                if (élémentTrouvéAuRebut.EstConforme) dictionary[DestinationPlat.RebutMaisConforme]++;
                else dictionary[DestinationPlat.Rebut]++;
            }
            else dictionary[DestinationPlat.Perdu]++;
        }

        return dictionary;
    }

    /// <inheritdoc />
    public void MettreAuRebut(params IConforme[] conformes)
    {
        foreach (var conforme in conformes)
            _rebut.Add(conforme);
    }
}