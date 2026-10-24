namespace Kepler7.Core.Patrones.Creacionales.Builder;

/// <summary>
/// Nave de apoyo que se arma en el Hangar. Casco y motor van siempre; el resto es opcional.
/// Lo que lleva montado define cómo ayuda al escuadrón en la oleada.
/// </summary>
public class Nave
{
    private readonly List<PiezaNave> _piezas = [];

    public Nave(string modelo)
    {
        Modelo = modelo;
    }

    public string Modelo { get; }
    public string Casco { get; internal set; } = "";
    public string Motor { get; internal set; } = "";

    /// <summary>Puntos de escudo. 0 = sin escudo.</summary>
    public int Escudo { get; internal set; }

    /// <summary>Potencia de fuego. 0 = desarmada.</summary>
    public int Armas { get; internal set; }

    public bool Radar { get; internal set; }

    /// <summary>IA de a bordo. null = se pilotea a mano.</summary>
    public string? IA { get; internal set; }

    public int Tanques { get; internal set; }

    /// <summary>Toneladas de carga. 0 = sin bodega.</summary>
    public int Bodega { get; internal set; }

    /// <summary>Las piezas en el orden en que se fueron montando.</summary>
    public IReadOnlyList<PiezaNave> Piezas => _piezas;

    public bool RevelaIA => Radar;

    public int DanioDeApoyo => Armas;

    /// <summary>Porcentaje extra de créditos al ganar la oleada.</summary>
    public int BonusCreditos => Bodega > 0 ? 50 : 0;

    /// <summary>Cómo ayuda en la oleada, según lo que lleva montado.</summary>
    public string Apoyo
    {
        get
        {
            var ayudas = new List<string>();
            if (RevelaIA)
                ayudas.Add("revela la IA de los enemigos");
            if (DanioDeApoyo > 0)
                ayudas.Add($"dispara {DanioDeApoyo} de daño a la vanguardia enemiga");
            if (Escudo > 0)
                ayudas.Add($"cubre al escuadrón con {Escudo} de escudo");
            if (BonusCreditos > 0)
                ayudas.Add($"trae {BonusCreditos}% más de créditos");
            return ayudas.Count switch
            {
                0 => "no da apoyo",
                1 => ayudas[0],
                _ => string.Join(", ", ayudas[..^1]) + " y " + ayudas[^1],
            };
        }
    }

    internal void Montar(string componente, string detalle) => _piezas.Add(new PiezaNave(componente, detalle));
}
