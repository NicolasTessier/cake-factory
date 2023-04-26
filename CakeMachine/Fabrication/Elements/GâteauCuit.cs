namespace CakeMachine.Fabrication.Elements;

internal class GâteauCuit : IConforme, IEquatable<GâteauCuit>
{
    private readonly GâteauCru _gâteauCru;

    public GâteauCuit(GâteauCru gâteauCru, bool estBienCuit)
    {
        _gâteauCru = gâteauCru;
        EstConforme = gâteauCru.EstConforme && estBienCuit;
        gâteauCru.Lock(this);
    }

    /// <inheritdoc />
    public bool EstConforme { get; }

    /// <inheritdoc />
    public Plat PlatSousJacent => _gâteauCru.PlatSousJacent;

    private bool _hasSuccessor;

    public void Lock(GâteauEmballé _)
    {
        if (_hasSuccessor)
            throw new InvalidOperationException("Ce gâteau a déjà été emballé ! " +
                                                "N'essayez pas de frauder.");
        _hasSuccessor = true;
    }

    /// <inheritdoc />
    public bool Equals(GâteauCuit? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _gâteauCru.Equals(other._gâteauCru);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((GâteauCuit)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => _gâteauCru.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => $"Gâteau Cuit [{_gâteauCru}]";
}