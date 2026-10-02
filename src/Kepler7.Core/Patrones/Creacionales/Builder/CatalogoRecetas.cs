namespace Kepler7.Core.Patrones.Creacionales.Builder;

/// <summary>Las recetas que ofrece el Hangar.</summary>
public static class CatalogoRecetas
{
    public static IReadOnlyList<INaveBuilder> Crear() =>
    [
        new RecetaExploradora(),
        new RecetaCombate(),
        new RecetaCarga(),
    ];
}
