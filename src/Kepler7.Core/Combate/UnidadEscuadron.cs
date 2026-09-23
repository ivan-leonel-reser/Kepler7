namespace Kepler7.Core.Combate;

public class UnidadEscuadron : Unidad
{
    public UnidadEscuadron(string nombre, ClaseUnidad clase, int vidaMaxima, int ataque, int movimiento, int alcance)
        : base(nombre, Bando.Escuadron, vidaMaxima, ataque, movimiento, alcance)
    {
        Clase = clase;
    }

    public ClaseUnidad Clase { get; }

    /// <summary>Crea una unidad con los valores base de su clase.</summary>
    public static UnidadEscuadron Crear(string nombre, ClaseUnidad clase) => clase switch
    {
        ClaseUnidad.Soldado => new(nombre, clase, vidaMaxima: 30, ataque: 8, movimiento: 3, alcance: 1),
        ClaseUnidad.Medico => new(nombre, clase, vidaMaxima: 22, ataque: 4, movimiento: 3, alcance: 1),
        ClaseUnidad.Francotirador => new(nombre, clase, vidaMaxima: 18, ataque: 10, movimiento: 2, alcance: 4),
        ClaseUnidad.Comandante => new(nombre, clase, vidaMaxima: 35, ataque: 7, movimiento: 3, alcance: 2),
        _ => throw new ArgumentOutOfRangeException(nameof(clase)),
    };
}
