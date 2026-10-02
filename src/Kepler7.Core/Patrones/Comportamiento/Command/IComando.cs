using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.Command;

/// <summary>Una acción del jugador guardada como objeto, para poder deshacerla y rehacerla.</summary>
public interface IComando
{
    /// <summary>La unidad que hace la acción (cada una actúa una vez por turno).</summary>
    Unidad Actor { get; }

    /// <summary>Qué pasó, para el historial. Se completa al ejecutar.</summary>
    string Descripcion { get; }

    void Ejecutar();

    void Deshacer();
}
