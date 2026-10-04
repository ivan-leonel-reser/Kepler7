namespace Kepler7.Core.Patrones.Estructurales.Flyweight;

/// <summary>
/// Lo que es igual para todos los tripulantes de una especie (estado intrínseco).
/// Hay un solo objeto por especie y lo comparten miles de tripulantes.
/// </summary>
public interface IEspecie
{
    string Nombre { get; }

    /// <summary>Color en hexadecimal, para que la interfaz lo pinte.</summary>
    string Color { get; }

    string PlanetaOrigen { get; }

    string Dieta { get; }

    /// <summary>Vida que la especie le suma a un recluta.</summary>
    int BonusVida { get; }

    /// <summary>Ataque que la especie le suma a un recluta.</summary>
    int BonusAtaque { get; }
}
