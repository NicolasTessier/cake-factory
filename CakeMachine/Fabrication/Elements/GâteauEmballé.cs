namespace CakeMachine.Fabrication.Elements;

internal class GâteauEmballé : IConforme, IEquatable<GâteauEmballé>
{
    private readonly GâteauCuit _gâteau;

    public GâteauEmballé(GâteauCuit gâteau, bool estCorrectementEmballé)
    {
        _gâteau = gâteau;
        EstConforme = gâteau.EstConforme && estCorrectementEmballé;
        gâteau.Lock(this);
    }

    /// <inheritdoc />
    public bool EstConforme { get; }

    /// <inheritdoc />
    public Plat PlatSousJacent => _gâteau.PlatSousJacent;

    /// <inheritdoc />
    public bool Equals(GâteauEmballé? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _gâteau.Equals(other._gâteau);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((GâteauEmballé)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode() => _gâteau.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => $"Gâteau Emballé [{_gâteau}]";
}