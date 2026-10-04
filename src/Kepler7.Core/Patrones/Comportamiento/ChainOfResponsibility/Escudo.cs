using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;

/// <summary>Tiene puntos propios que absorben daño hasta agotarse. Se recarga en cada oleada.</summary>
public class Escudo : ManejadorDanio
{
    public const int PuntosPorOleada = 6;

    public override string Nombre => "Escudo";

    public int Puntos { get; private set; } = PuntosPorOleada;

    protected override string Absorber(Danio danio)
    {
        if (Puntos == 0)
            return "está agotado";

        var absorbe = Math.Min(Puntos, danio.Restante);
        Puntos -= absorbe;
        danio.Restante -= absorbe;
        return $"absorbe {absorbe} (le quedan {Puntos} puntos)";
    }
}
