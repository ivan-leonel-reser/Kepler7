using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;

/// <summary>Base de las capas: cada una hace lo suyo con el golpe y, si queda daño, se lo pasa a la siguiente.</summary>
public abstract class ManejadorDanio : IManejadorDanio
{
    public abstract string Nombre { get; }

    public IManejadorDanio? Siguiente { get; set; }

    public void Manejar(Danio danio)
    {
        var entra = danio.Restante;
        var detalle = Absorber(danio);
        danio.Registrar(Nombre, entra, detalle);

        // Si la capa frenó todo, el golpe termina acá y las que siguen ni se enteran.
        if (!danio.FueAbsorbido)
            Siguiente?.Manejar(danio);
    }

    /// <summary>Descuenta de <see cref="Danio.Restante"/> lo que esta capa frena y cuenta qué hizo.</summary>
    protected abstract string Absorber(Danio danio);
}
