namespace Kepler7.Core.Patrones.Comportamiento.Strategy;

/// <summary>Las IA que se le pueden cargar a un enemigo. No guardan estado, así que se comparten.</summary>
public static class CatalogoEstrategias
{
    public static IReadOnlyList<IEstrategiaIA> Todas { get; } =
    [
        new IAAgresiva(),
        new IADefensiva(),
        new IAAleatoria(),
        new IACalculadora(),
    ];
}
