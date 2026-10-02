using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.Strategy;

/// <summary>Una forma de decidir el turno de un enemigo. Se le puede cambiar en plena oleada.</summary>
public interface IEstrategiaIA
{
    string Nombre { get; }

    string Descripcion { get; }

    /// <summary>Solo decide: no toca el campo. La oleada es la que ejecuta la decisión.</summary>
    DecisionIA DecidirAccion(Enemigo enemigo, Contexto contexto);
}
