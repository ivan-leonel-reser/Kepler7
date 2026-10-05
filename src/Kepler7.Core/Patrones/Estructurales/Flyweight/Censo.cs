using Kepler7.Services;

namespace Kepler7.Core.Patrones.Estructurales.Flyweight;

/// <summary>El padrón de la estación: 10.000 tripulantes de solo 5 especies.</summary>
public class Censo
{
    public const int TotalTripulantes = 10_000;

    private static readonly string[] NombresDePila =
    [
        "Ana", "Bruno", "Carla", "Darío", "Elena", "Fabio", "Gala", "Hugo", "Irene", "Joel", "Kira", "Leo", "Mara",
        "Nico", "Olga", "Pablo", "Rita", "Saúl", "Tania", "Uma", "Vera", "Walter", "Ximena", "Yago", "Zoe",
    ];

    private static readonly string[] Apellidos =
    [
        "Acosta", "Bravo", "Castro", "Díaz", "Esquivel", "Ferrer", "Godoy", "Herrera", "Ibarra", "Juárez", "Lagos",
        "Molina", "Navarro", "Ortiz", "Paredes", "Quiroga", "Ríos", "Sosa", "Toledo", "Urrutia", "Vidal", "Zárate",
    ];

    private static readonly string[] Rangos = ["Cadete", "Técnico", "Especialista", "Sargento", "Teniente"];

    // De cada 20 tripulantes: 8 humanos, 5 keplerianos, 3 litoides, 2 aerianos y 2 abisales.
    private static readonly string[] Reparto =
    [
        "Humano", "Kepleriano", "Humano", "Litoide", "Humano", "Kepleriano", "Aeriano", "Humano", "Abisal", "Kepleriano",
        "Humano", "Litoide", "Humano", "Kepleriano", "Humano", "Abisal", "Litoide", "Kepleriano", "Humano", "Aeriano",
    ];

    private readonly List<Tripulante> _tripulantes = [];

    public FabricaEspecies Fabrica { get; } = new();

    public IReadOnlyList<Tripulante> Tripulantes => _tripulantes;

    public bool Generado => _tripulantes.Count > 0;

    /// <summary>Queda cargada al generar el padrón.</summary>
    public ComparacionMemoria? Comparacion { get; private set; }

    /// <summary>Arma el padrón una sola vez. El nombre y el rango de cada uno salen al azar.</summary>
    public void Generar(IAleatorio aleatorio, int cantidad = TotalTripulantes)
    {
        if (Generado)
            return;
        ArgumentOutOfRangeException.ThrowIfLessThan(cantidad, 1);

        // Primero lo propio de cada tripulante. Después se arma el padrón de las dos formas para poder comparar.
        var datos = new List<(int Id, string Nombre, string Rango, string Especie)>(cantidad);
        for (var i = 0; i < cantidad; i++)
        {
            var nombre = $"{Elegir(NombresDePila, aleatorio)} {Elegir(Apellidos, aleatorio)}";
            datos.Add((i + 1, nombre, Elegir(Rangos, aleatorio), Reparto[i % Reparto.Length]));
        }

        _tripulantes.Capacity = cantidad;
        var antes = GC.GetAllocatedBytesForCurrentThread();
        foreach (var d in datos)
            _tripulantes.Add(new Tripulante(d.Id, d.Nombre, d.Rango, Fabrica.Obtener(d.Especie)));
        var conFlyweight = GC.GetAllocatedBytesForCurrentThread() - antes;

        // Lo mismo sin compartir: cada tripulante con su propio objeto de especie.
        var sinCompartir = new List<Tripulante>(cantidad);
        antes = GC.GetAllocatedBytesForCurrentThread();
        foreach (var d in datos)
            sinCompartir.Add(new Tripulante(d.Id, d.Nombre, d.Rango, FabricaEspecies.Crear(d.Especie)));
        var sinFlyweight = GC.GetAllocatedBytesForCurrentThread() - antes;

        Comparacion = new ComparacionMemoria(cantidad, Fabrica.EspeciesCreadas, sinCompartir.Count, conFlyweight, sinFlyweight);
    }

    /// <summary>Cuántos tripulantes comparten ese objeto de especie.</summary>
    public int CantidadDe(IEspecie especie) => _tripulantes.Count(t => ReferenceEquals(t.Especie, especie));

    private static string Elegir(string[] opciones, IAleatorio aleatorio) =>
        opciones[aleatorio.Siguiente(0, opciones.Length)];
}
