namespace Kepler7.Core.Patrones.Estructurales.Flyweight;

/// <summary>
/// Un tripulante del censo. Guarda solo lo propio (estado extrínseco) y una referencia
/// a su especie, que es un objeto compartido.
/// </summary>
public record Tripulante(int Id, string Nombre, string Rango, IEspecie Especie);
