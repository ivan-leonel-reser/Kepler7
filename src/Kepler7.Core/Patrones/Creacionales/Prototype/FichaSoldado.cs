using Kepler7.Core.Combate;
using Kepler7.Core.Patrones.Estructurales.Flyweight;

namespace Kepler7.Core.Patrones.Creacionales.Prototype;

/// <summary>
/// La ficha de un soldado en el Laboratorio: quién es y qué sabe hacer.
/// De una ficha sale un clon, y de la ficha del clon sale la unidad que pelea.
/// </summary>
public class FichaSoldado : IPrototipo
{
    public const int MaximoHabilidades = 3;

    public FichaSoldado(string nombre, ClaseUnidad clase, IEspecie? especie = null)
    {
        Nombre = nombre;
        Clase = clase;
        Especie = especie;
        Habilidades.Add(CatalogoHabilidades.DeClase(clase));
    }

    public string Nombre { get; set; }

    public ClaseUnidad Clase { get; }

    /// <summary>Solo la tienen los reclutas del Censo.</summary>
    public IEspecie? Especie { get; }

    public List<string> Habilidades { get; } = [];

    public int BonusVida => (Especie?.BonusVida ?? 0) + Entrenadas.Sum(h => h.BonusVida);

    public int BonusAtaque => (Especie?.BonusAtaque ?? 0) + Entrenadas.Sum(h => h.BonusAtaque);

    private IEnumerable<Habilidad> Entrenadas =>
        Habilidades.Select(CatalogoHabilidades.Buscar).OfType<Habilidad>();

    /// <summary>
    /// Copia profunda: el clon recibe una lista nueva con las mismas habilidades, así que cambiarle
    /// una al clon no toca al original. La especie sí es la misma: es un objeto compartido que no cambia.
    /// </summary>
    public IPrototipo Clonar()
    {
        var clon = new FichaSoldado(Nombre, Clase, Especie);
        clon.Habilidades.Clear();
        clon.Habilidades.AddRange(Habilidades);
        return clon;
    }

    public bool PuedeAprender(string habilidad) =>
        Habilidades.Count < MaximoHabilidades
        && !Habilidades.Contains(habilidad)
        && CatalogoHabilidades.Buscar(habilidad) is not null;

    public void Aprender(string habilidad)
    {
        if (!PuedeAprender(habilidad))
            throw new InvalidOperationException($"{Nombre} no puede aprender {habilidad}.");

        Habilidades.Add(habilidad);
    }

    // La habilidad de la clase es la que usa en su turno: esa no se olvida.
    public bool PuedeOlvidar(string habilidad) =>
        Habilidades.Contains(habilidad) && habilidad != CatalogoHabilidades.DeClase(Clase);

    public void Olvidar(string habilidad)
    {
        if (!PuedeOlvidar(habilidad))
            throw new InvalidOperationException($"{Nombre} no puede olvidar {habilidad}.");

        Habilidades.Remove(habilidad);
    }

    /// <summary>La unidad que sale a pelear, con lo que le suman su especie y sus habilidades.</summary>
    public UnidadEscuadron CrearUnidad() => UnidadEscuadron.Crear(Nombre, Clase, BonusVida, BonusAtaque);
}
