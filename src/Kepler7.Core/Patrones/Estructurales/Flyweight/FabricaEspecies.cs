namespace Kepler7.Core.Patrones.Estructurales.Flyweight;

/// <summary>
/// Entrega siempre el mismo objeto para cada especie: lo crea la primera vez que se lo piden
/// y después devuelve el que ya tiene guardado.
/// </summary>
public class FabricaEspecies
{
    private readonly Dictionary<string, IEspecie> _especies = [];

    /// <summary>Las cinco especies que viven en Kepler-7.</summary>
    public static IReadOnlyList<string> Nombres { get; } = ["Humano", "Kepleriano", "Litoide", "Aeriano", "Abisal"];

    /// <summary>Cuántos objetos de especie se crearon de verdad.</summary>
    public int EspeciesCreadas => _especies.Count;

    /// <summary>Cuántas veces le pidieron una especie.</summary>
    public int Pedidos { get; private set; }

    public IEspecie Obtener(string nombre)
    {
        Pedidos++;
        if (!_especies.TryGetValue(nombre, out var especie))
        {
            especie = Crear(nombre);
            _especies[nombre] = especie;
        }
        return especie;
    }

    // Crea un objeto nuevo cada vez. La fábrica lo usa una sola vez por especie;
    // el censo lo usa además para medir cuánto ocuparía todo sin compartir.
    internal static Especie Crear(string nombre) => nombre switch
    {
        "Humano" => new Especie(nombre, "#4FA3FF", "Tierra", "Omnívora", bonusVida: 3, bonusAtaque: 1),
        "Kepleriano" => new Especie(nombre, "#33FF99", "Kepler-7b", "Fotosintética", bonusVida: 6, bonusAtaque: 0),
        "Litoide" => new Especie(nombre, "#FFBA40", "Crisol", "Mineral", bonusVida: 0, bonusAtaque: 2),
        "Aeriano" => new Especie(nombre, "#B48CFF", "Zefir", "Filtradora", bonusVida: 4, bonusAtaque: 1),
        "Abisal" => new Especie(nombre, "#FF6B6B", "Tetis", "Carnívora", bonusVida: 0, bonusAtaque: 3),
        _ => throw new ArgumentException($"No hay una especie llamada {nombre}.", nameof(nombre)),
    };
}
