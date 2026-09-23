namespace Kepler7.Core.Combate;

public class UnidadEscuadron : Unidad
{
    public UnidadEscuadron(string nombre, ClaseUnidad clase, int vidaMaxima, int ataque, bool atacaADistancia)
        : base(nombre, Bando.Escuadron, vidaMaxima, ataque, atacaADistancia)
    {
        Clase = clase;
    }

    public ClaseUnidad Clase { get; }

    /// <summary>Crea una unidad con los valores base de su clase.</summary>
    public static UnidadEscuadron Crear(string nombre, ClaseUnidad clase) => clase switch
    {
        ClaseUnidad.Soldado => new(nombre, clase, vidaMaxima: 30, ataque: 8, atacaADistancia: false),
        ClaseUnidad.Medico => new(nombre, clase, vidaMaxima: 22, ataque: 4, atacaADistancia: true),
        ClaseUnidad.Francotirador => new(nombre, clase, vidaMaxima: 18, ataque: 10, atacaADistancia: true),
        ClaseUnidad.Comandante => new(nombre, clase, vidaMaxima: 35, ataque: 7, atacaADistancia: false),
        _ => throw new ArgumentOutOfRangeException(nameof(clase)),
    };
}
