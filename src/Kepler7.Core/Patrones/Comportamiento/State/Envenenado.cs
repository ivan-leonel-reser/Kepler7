using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.State;

/// <summary>Pierde vida al terminar cada turno y pega más flojo. Se le pasa solo después de unos turnos.</summary>
public class Envenenado : EstadoPersonaje
{
    public const int DanioPorTurno = 2;
    public const int Duracion = 3;
    public const int AtaquePerdido = 1;

    public int TurnosRestantes { get; private set; } = Duracion;

    public override string Nombre => "Envenenado";

    public override string Color => "#5EE278";

    public override string Efecto => $"pierde {DanioPorTurno} por turno";

    public override int Atacar(Unidad unidad, int danio) => Math.Max(0, danio - AtaquePerdido);

    public override string? TerminarTurno(Unidad unidad)
    {
        // El veneno va directo a la vida: las defensas no lo frenan.
        var perdida = unidad.PerderVida(DanioPorTurno);
        TurnosRestantes--;

        if (!unidad.EstaViva)
            return $"pierde {perdida} por el veneno y cae";
        if (TurnosRestantes > 0)
            return $"pierde {perdida} por el veneno (le quedan {TurnosRestantes} turnos)";

        unidad.CambiarEstado(new Normal());
        return $"pierde {perdida} por el veneno y se le pasa";
    }
}
