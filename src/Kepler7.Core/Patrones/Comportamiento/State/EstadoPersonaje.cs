using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.State;

/// <summary>Lo que hace una unidad cuando el estado no cambia nada. Cada estado redefine solo lo suyo.</summary>
public abstract class EstadoPersonaje : IEstadoPersonaje
{
    public abstract string Nombre { get; }

    public abstract string Color { get; }

    public abstract string Efecto { get; }

    public virtual bool PuedeActuar => true;

    public virtual int Atacar(Unidad unidad, int danio) => danio;

    public virtual bool Mover(Unidad unidad) => true;

    public virtual int RecibirDanio(Unidad unidad, Danio danio)
    {
        unidad.Defensas.Manejar(danio);
        return danio.VidaPerdida;
    }

    public virtual string? TerminarTurno(Unidad unidad) => null;
}
