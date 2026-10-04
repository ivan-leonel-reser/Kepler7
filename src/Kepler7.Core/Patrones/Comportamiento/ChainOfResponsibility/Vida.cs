using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;

/// <summary>El último eslabón: lo que llega hasta acá, la unidad lo pierde de vida.</summary>
public class Vida : ManejadorDanio
{
    private readonly Unidad _unidad;

    public Vida(Unidad unidad)
    {
        _unidad = unidad;
    }

    public override string Nombre => "Vida";

    protected override string Absorber(Danio danio)
    {
        danio.VidaPerdida = _unidad.PerderVida(danio.Restante);
        danio.Restante = 0;
        return $"pierde {danio.VidaPerdida} (queda en {_unidad.Vida}/{_unidad.VidaMaxima})";
    }
}
