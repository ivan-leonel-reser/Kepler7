using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.TemplateMethod;

/// <summary>Pelea acompañado: cada aliado en su misma fila le suma daño al ataque.</summary>
public class TurnoComandante : TurnoUnidad
{
    public TurnoComandante(UnidadEscuadron unidad, CampoBatalla campo) : base(unidad, campo)
    {
    }

    public override string Habilidad => "Al atacar suma +1 de daño por cada aliado en su fila.";

    private int EnemigosEnPie => Campo.UnidadesVivasDe(Bando.Enjambre).Count();

    protected override string Iniciar() => $"evalúa el campo: {EnemigosEnPie} enemigos en pie";

    protected override string Decidir(OrdenTurno orden)
    {
        if (orden.Tipo != TipoOrden.Atacar)
            return Describir(orden);

        var aliados = Campo.EnFila(Bando.Escuadron, Unidad.Fila).OfType<UnidadEscuadron>().Count(u => u != Unidad);
        if (aliados == 0)
            return $"ataca a {orden.Objetivo?.Nombre} sin apoyo en su fila";

        Bonus = aliados;
        return $"ordena el ataque sobre {orden.Objetivo?.Nombre} con apoyo de su fila (+{aliados} de daño)";
    }

    protected override string Finalizar() => $"da el parte: quedan {EnemigosEnPie} enemigos";
}
