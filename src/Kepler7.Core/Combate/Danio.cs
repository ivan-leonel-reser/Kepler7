using Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;

namespace Kepler7.Core.Combate;

/// <summary>Un golpe que recibe una unidad. Las capas de defensa van descontando <see cref="Restante"/>.</summary>
public class Danio
{
    private readonly List<PasoDanio> _recorrido = [];
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

    /// <summary>La vida que terminó perdiendo la unidad. La carga el último eslabón de la cadena.</summary>
    public int VidaPerdida { get; set; }

    /// <summary>Lo que hizo cada capa con este golpe, en el orden en que lo recibieron.</summary>
    public IReadOnlyList<PasoDanio> Recorrido => _recorrido;

    public void Registrar(string capa, int entra, string detalle) =>
        _recorrido.Add(new PasoDanio(capa, entra, Restante, detalle));

    /// <summary>El recorrido en una línea: cuánto daño quedaba al salir de cada capa y cómo terminó.</summary>
    public string DescribirRecorrido()
    {
        var capas = _recorrido.Where(p => p.Capa != "Vida").Select(p => $"{p.Capa} {p.Sale}");
        var final = VidaPerdida > 0 ? $"Vida: pierde {VidaPerdida}" : "no pierde vida";
        return string.Join(" › ", new[] { $"Golpe de {Original}" }.Concat(capas).Append(final));
    }
}
