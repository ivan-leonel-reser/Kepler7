using Kepler7.BE;
using Kepler7.Core.Patrones.Comportamiento.Strategy;

namespace Kepler7.Core.Combate;

/// <summary>Arma los enemigos de cada oleada: cada una trae más y más fuertes.</summary>
public static class GeneradorOleadas
{
    // Z = Zángano (cuerpo a cuerpo), E = Escupidor (a distancia), C = Coloso (aguanta mucho), R = Reina.
    // Cada tipo llega con su IA: agresiva, aleatoria, defensiva y calculadora, en ese orden.
    // El Escupidor envenena con cada golpe que saca vida; el Coloso y la Reina aturden.
    private static readonly string[] Composicion = ["ZZ", "ZZE", "ZCE", "ZZCE", "ZCCEE", "RZZEE"];

    public static IReadOnlyList<Enemigo> Crear(int numeroOleada)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(numeroOleada, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(numeroOleada, ProgresoJugador.OleadasTotales);

        var enemigos = new List<Enemigo>();
        var cantidadPorTipo = new Dictionary<char, int>();
        foreach (var tipo in Composicion[numeroOleada - 1])
        {
            cantidadPorTipo[tipo] = cantidadPorTipo.GetValueOrDefault(tipo) + 1;
            enemigos.Add(CrearEnemigo(tipo, cantidadPorTipo[tipo], numeroOleada));
        }
        return enemigos;
    }

    /// <summary>El zángano de más que llega cuando el Enjambre interceptó un mensaje a la Tierra.</summary>
    public static Enemigo CrearInterceptor(int numeroOleada) =>
        new("Zángano interceptor", VidaZangano(numeroOleada), AtaqueComun(numeroOleada), estrategia: new IAAgresiva());

    // Los números salen de simular partidas enteras: las primeras oleadas se ganan sin preparar nada,
    // de la cuarta en adelante hacen falta los bonus de los módulos.
    private static int VidaZangano(int oleada) => 16 + 5 * oleada;

    // Sube de a uno cada dos oleadas: 5, 5, 6, 6, 7, 7.
    private static int AtaqueComun(int oleada) => 4 + (oleada + 1) / 2;

    private static Enemigo CrearEnemigo(char tipo, int numero, int oleada) => tipo switch
    {
        'Z' => new Enemigo($"Zángano {numero}", VidaZangano(oleada), AtaqueComun(oleada), estrategia: new IAAgresiva()),
        'E' => new Enemigo($"Escupidor {numero}", vidaMaxima: 12 + 3 * oleada, AtaqueComun(oleada), atacaADistancia: true, estrategia: new IAAleatoria())
        {
            EfectoAlGolpear = EfectoGolpe.Veneno,
        },
        'C' => new Enemigo($"Coloso {numero}", vidaMaxima: 30 + 6 * oleada, ataque: AtaqueComun(oleada) + 2, estrategia: new IADefensiva())
        {
            EfectoAlGolpear = EfectoGolpe.Aturdimiento,
        },
        'R' => new Enemigo("Reina del Enjambre", vidaMaxima: 100, ataque: 12, estrategia: new IACalculadora())
        {
            EfectoAlGolpear = EfectoGolpe.Aturdimiento,
        },
        _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
    };
}
