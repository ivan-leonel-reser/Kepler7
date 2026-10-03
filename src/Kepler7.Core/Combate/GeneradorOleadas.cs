using Kepler7.BE;
using Kepler7.Core.Patrones.Comportamiento.Strategy;

namespace Kepler7.Core.Combate;

/// <summary>Arma los enemigos de cada oleada: cada una trae más y más fuertes.</summary>
public static class GeneradorOleadas
{
    // Z = Zángano (cuerpo a cuerpo), E = Escupidor (a distancia), C = Coloso (aguanta mucho), R = Reina.
    // Cada tipo llega con su IA: agresiva, aleatoria, defensiva y calculadora, en ese orden.
    private static readonly string[] Composicion = ["ZZ", "ZZE", "ZCE", "ZZCE", "ZCCEE", "RZEE"];

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

    private static Enemigo CrearEnemigo(char tipo, int numero, int oleada) => tipo switch
    {
        'Z' => new Enemigo($"Zángano {numero}", vidaMaxima: 10 + 2 * oleada, ataque: 2 + oleada / 2, estrategia: new IAAgresiva()),
        'E' => new Enemigo($"Escupidor {numero}", vidaMaxima: 8 + oleada, ataque: 2 + oleada / 2, atacaADistancia: true, estrategia: new IAAleatoria()),
        'C' => new Enemigo($"Coloso {numero}", vidaMaxima: 20 + 3 * oleada, ataque: 3 + oleada / 2, estrategia: new IADefensiva()),
        'R' => new Enemigo("Reina del Enjambre", vidaMaxima: 55, ataque: 8, estrategia: new IACalculadora()),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
    };
}
