using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.State;

/// <summary>
/// El estado en que está una unidad. La unidad le delega cómo ataca, si se mueve y cómo recibe
/// un golpe; cuando corresponde, es el propio estado el que la pasa al siguiente.
/// </summary>
public interface IEstadoPersonaje
{
    string Nombre { get; }

    /// <summary>Color en hexadecimal con el que la interfaz marca a la unidad.</summary>
    string Color { get; }

    /// <summary>Qué le pasa a la unidad mientras está así, en pocas palabras.</summary>
    string Efecto { get; }

    /// <summary>Si en este estado la unidad puede usar su turno.</summary>
    bool PuedeActuar { get; }

    /// <summary>Con cuánto daño termina pegando la unidad.</summary>
    int Atacar(Unidad unidad, int danio);

    /// <summary>Si la unidad puede cambiar de fila.</summary>
    bool Mover(Unidad unidad);

    /// <summary>Recibe el golpe y lo pasa por las defensas de la unidad.</summary>
    /// <returns>La vida que perdió.</returns>
    int RecibirDanio(Unidad unidad, Danio danio);

    /// <summary>Lo que pasa cuando termina el turno de su bando.</summary>
    /// <returns>El aviso para la bitácora, o null si no pasó nada.</returns>
    string? TerminarTurno(Unidad unidad);
}
