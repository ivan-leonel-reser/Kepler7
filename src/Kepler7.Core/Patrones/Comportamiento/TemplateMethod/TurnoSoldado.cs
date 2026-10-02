using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.TemplateMethod;

/// <summary>Pelea cuerpo a cuerpo: cuando ataca, carga y pega más fuerte.</summary>
public class TurnoSoldado : TurnoUnidad
{
    public const int BonusCarga = 2;

    public TurnoSoldado(UnidadEscuadron unidad, CampoBatalla campo) : base(unidad, campo)
    {
    }

    protected override string Iniciar() =>
        Unidad.Fila == Fila.Vanguardia ? "está adelante, listo para cargar" : "está atrás, sin nadie a su alcance";

    protected override string Decidir(OrdenTurno orden)
    {
        if (orden.Tipo != TipoOrden.Atacar)
            return Describir(orden);

        Bonus = BonusCarga;
        return $"carga contra {orden.Objetivo?.Nombre} (+{BonusCarga} de daño)";
    }
}
