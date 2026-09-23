namespace Kepler7.Core.Combate;

/// <summary>Un golpe que recibe una unidad. Las capas de defensa van descontando <see cref="Restante"/>.</summary>
public class Danio
{
    private int _restante;

    public Danio(int cantidad, TipoDanio tipo = TipoDanio.Fisico, Unidad? atacante = null)
    {
        if (cantidad < 0)
            throw new ArgumentOutOfRangeException(nameof(cantidad));

        Original = cantidad;
        _restante = cantidad;
        Tipo = tipo;
        Atacante = atacante;
    }

    public int Original { get; }
    public TipoDanio Tipo { get; }
    public Unidad? Atacante { get; }

    public int Restante
    {
        get => _restante;
        set => _restante = Math.Max(0, value);
    }

    public bool FueAbsorbido => Restante == 0;
}
