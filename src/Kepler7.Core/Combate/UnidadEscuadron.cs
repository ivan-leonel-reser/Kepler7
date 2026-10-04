namespace Kepler7.Core.Combate;

public class UnidadEscuadron : Unidad
{
    public UnidadEscuadron(string nombre, ClaseUnidad clase, int vidaMaxima, int ataque, bool atacaADistancia)
        : base(nombre, Bando.Escuadron, vidaMaxima, ataque, atacaADistancia)
    {
        Clase = clase;
    }

    public ClaseUnidad Clase { get; }

    /// <summary>Crea una unidad con los valores base de su clase, más el bonus que traiga (por ejemplo, el de su especie).</summary>
    public static UnidadEscuadron Crear(string nombre, ClaseUnidad clase, int bonusVida = 0, int bonusAtaque = 0) => clase switch
    {
        ClaseUnidad.Soldado => new(nombre, clase, vidaMaxima: 30 + bonusVida, ataque: 8 + bonusAtaque, atacaADistancia: false),
        ClaseUnidad.Medico => new(nombre, clase, vidaMaxima: 22 + bonusVida, ataque: 4 + bonusAtaque, atacaADistancia: true),
        ClaseUnidad.Francotirador => new(nombre, clase, vidaMaxima: 18 + bonusVida, ataque: 10 + bonusAtaque, atacaADistancia: true),
        ClaseUnidad.Comandante => new(nombre, clase, vidaMaxima: 35 + bonusVida, ataque: 7 + bonusAtaque, atacaADistancia: false),
        _ => throw new ArgumentOutOfRangeException(nameof(clase)),
    };
}
