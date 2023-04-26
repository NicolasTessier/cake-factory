namespace CakeMachine.Fabrication.Elements;

internal class GâteauCru : IConforme, IEquatable<GâteauCru>
{
    public GâteauCru(Plat plat, bool estCorrectementPréparé)
    {
        PlatSousJacent = plat;
        EstConforme = plat.EstConforme && estCorrectementPréparé;
        plat.Lock(this);
    }

    /// <inheritdoc />
    public bool EstConforme { get; }

    /// <inheritdoc />
    public Plat PlatSousJacent { get; }

    private bool _hasSuccessor;

    public void Lock(GâteauCuit _)
    {
        if (_hasSuccessor)
            throw new InvalidOperationException("Ce gâteau a déjà été cuit ! " +
                                                "N'essayez pas de frauder.");

        _hasSuccessor = true;
    }

    /// <inheritdoc />
    public bool Equals(GâteauCru? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return PlatSousJacent.Equals(other.PlatSousJacent);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((GâteauCru)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => PlatSousJacent.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => $"Gâteau Cru [{PlatSousJacent}]";
}