namespace Kepler7.Core.Patrones.Comportamiento.Visitor;

/// <summary>Los analizadores que ofrece el Almacén. Cada uno acumula su resultado, así que se crea uno nuevo por uso.</summary>
public static class CatalogoVisitantes
{
    public static IReadOnlyList<IVisitor> Crear() =>
    [
        new CalculadorPrecio(),
        new ContadorArmas(),
        new AplicadorVeneno(),
    ];
}
