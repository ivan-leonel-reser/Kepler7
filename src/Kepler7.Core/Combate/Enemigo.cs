using Kepler7.Core.Patrones.Comportamiento.Strategy;

namespace Kepler7.Core.Combate;

/// <summary>Unidad del Enjambre. Su turno lo decide la estrategia que tenga asignada.</summary>
public class Enemigo : Unidad
{
    public Enemigo(string nombre, int vidaMaxima, int ataque, bool atacaADistancia = false, IEstrategiaIA? estrategia = null)
        : base(nombre, Bando.Enjambre, vidaMaxima, ataque, atacaADistancia)
    {
        Estrategia = estrategia ?? new IAAgresiva();
    }

    /// <summary>La IA que decide su turno. Se puede cambiar durante la oleada.</summary>
    public IEstrategiaIA Estrategia { get; set; }

    public DecisionIA DecidirAccion(Contexto contexto) => Estrategia.DecidirAccion(this, contexto);
}
