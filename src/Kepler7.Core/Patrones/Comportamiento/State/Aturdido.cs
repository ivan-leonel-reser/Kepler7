using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.State;

/// <summary>Pierde su próximo turno: no ataca ni se mueve. Después vuelve a estar normal.</summary>
public class Aturdido : EstadoPersonaje
{
    public override string Nombre => "Aturdido";

    public override string Color => "#FFBA40";

    public override string Efecto => "pierde el turno";

    public override bool PuedeActuar => false;

    public override int Atacar(Unidad unidad, int danio) => 0;

    public override bool Mover(Unidad unidad) => false;

    public override string? TerminarTurno(Unidad unidad)
    {
        // Sale en guardia, para que no la dejen aturdida un turno atrás de otro.
        unidad.CambiarEstado(new Normal(recienRecuperada: true));
        return "se recupera del aturdimiento";
    }
}
