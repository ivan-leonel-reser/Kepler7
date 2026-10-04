using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;

/// <summary>Le saca siempre un poco a cada golpe. No se gasta.</summary>
public class Armadura : ManejadorDanio
{
    public const int Reduccion = 1;

    public override string Nombre => "Armadura";

    protected override string Absorber(Danio danio)
    {
        var frena = Math.Min(Reduccion, danio.Restante);
        danio.Restante -= frena;
        return $"frena {frena}";
    }
}
