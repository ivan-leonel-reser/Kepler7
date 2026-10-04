namespace Kepler7.Core.Patrones.Estructurales.Flyweight;

/// <summary>Los datos de una especie. No se puede modificar: si cambiara, cambiaría para todos los que la comparten.</summary>
public class Especie : IEspecie
{
    // Solo se crea desde la fábrica, que es la que garantiza una instancia por especie.
    internal Especie(string nombre, string color, string planetaOrigen, string dieta, int bonusVida, int bonusAtaque)
    {
        Nombre = nombre;
        Color = color;
        PlanetaOrigen = planetaOrigen;
        Dieta = dieta;
        BonusVida = bonusVida;
        BonusAtaque = bonusAtaque;
    }

    public string Nombre { get; }
    public string Color { get; }
    public string PlanetaOrigen { get; }
    public string Dieta { get; }
    public int BonusVida { get; }
    public int BonusAtaque { get; }
}
