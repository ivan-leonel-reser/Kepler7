using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.TemplateMethod;

/// <summary>Dispara de lejos: desde atrás tiene tiempo de apuntar y pega más fuerte.</summary>
public class TurnoFrancotirador : TurnoUnidad
{
    public const int BonusApuntado = 3;

    public TurnoFrancotirador(UnidadEscuadron unidad, CampoBatalla campo) : base(unidad, campo)
    {
    }

    protected override string Iniciar() =>
        Unidad.Fila == Fila.Retaguardia ? "está a cubierto atrás y puede apuntar" : "está adelante, sin tiempo para apuntar";

    protected override string Decidir(OrdenTurno orden)
    {
        if (orden.Tipo != TipoOrden.Atacar)
            return Describir(orden);
        if (Unidad.Fila != Fila.Retaguardia)
            return $"dispara a {orden.Objetivo?.Nombre} sin apuntar";

        Bonus = BonusApuntado;
        return $"apunta a {orden.Objetivo?.Nombre} desde atrás (+{BonusApuntado} de daño)";
    }

    protected override string Finalizar() => "recarga el rifle";
}
