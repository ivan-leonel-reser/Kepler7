using Kepler7.Core.Combate;
using Kepler7.Services;

namespace Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;

/// <summary>
/// Guarda qué capas lleva el escuadrón y en qué orden. Con eso le arma la cadena a cada unidad
/// al empezar una oleada, así el escudo y los bloqueos arrancan siempre cargados.
/// </summary>
public class Armeria
{
    // El orden que pide la consigna. La vida no está en la lista porque siempre cierra la cadena.
    private readonly List<TipoCapa> _capas = [TipoCapa.Esquiva, TipoCapa.Bloqueo, TipoCapa.Armadura, TipoCapa.Escudo];

    /// <summary>Las capas puestas, en el orden en que reciben el golpe.</summary>
    public IReadOnlyList<TipoCapa> Capas => _capas;

    /// <summary>Las que están sin usar.</summary>
    public IReadOnlyList<TipoCapa> Guardadas => Enum.GetValues<TipoCapa>().Except(_capas).ToList();

    public static string Descripcion(TipoCapa capa) => capa switch
    {
        TipoCapa.Esquiva => $"{Esquiva.Probabilidad}% de evitar el golpe entero",
        TipoCapa.Bloqueo => $"para entero un golpe de hasta {Bloqueo.GolpeMaximo}, {Bloqueo.UsosPorOleada} veces por oleada",
        TipoCapa.Armadura => $"le saca {Armadura.Reduccion} a cada golpe",
        TipoCapa.Escudo => $"absorbe hasta {Escudo.PuntosPorOleada} puntos por oleada",
        _ => throw new ArgumentOutOfRangeException(nameof(capa)),
    };

    public void Agregar(TipoCapa capa)
    {
        if (_capas.Contains(capa))
            throw new InvalidOperationException($"{capa} ya está en la cadena.");

        _capas.Add(capa);
    }

    public void Quitar(TipoCapa capa)
    {
        if (!_capas.Remove(capa))
            throw new InvalidOperationException($"{capa} no está en la cadena.");
    }

    public bool PuedeSubir(TipoCapa capa) => _capas.IndexOf(capa) > 0;

    public bool PuedeBajar(TipoCapa capa) => _capas.Contains(capa) && _capas.IndexOf(capa) < _capas.Count - 1;

    /// <summary>La capa pasa a recibir el golpe antes.</summary>
    public void Subir(TipoCapa capa) => Mover(capa, -1, PuedeSubir(capa));

    /// <summary>La capa pasa a recibir el golpe después.</summary>
    public void Bajar(TipoCapa capa) => Mover(capa, +1, PuedeBajar(capa));

    /// <summary>Le arma a la unidad una cadena nueva con las capas elegidas y la vida al final.</summary>
    public void Equipar(Unidad unidad, IAleatorio aleatorio)
    {
        unidad.Defensas.Vaciar();
        foreach (var capa in _capas)
            unidad.Defensas.Agregar(Crear(capa, aleatorio));
        unidad.Defensas.Agregar(new Vida(unidad));
    }

    /// <summary>Tira un golpe contra un maniquí equipado con la cadena actual, para ver el recorrido sin entrar a una oleada.</summary>
    public Danio ProbarGolpe(int cantidad, IAleatorio aleatorio)
    {
        var maniqui = UnidadEscuadron.Crear("Maniquí", ClaseUnidad.Soldado);
        Equipar(maniqui, aleatorio);

        var golpe = new Danio(cantidad);
        maniqui.RecibirGolpe(golpe);
        return golpe;
    }

    private void Mover(TipoCapa capa, int salto, bool sePuede)
    {
        if (!sePuede)
            throw new InvalidOperationException($"{capa} no se puede mover para ese lado.");

        var posicion = _capas.IndexOf(capa);
        _capas.RemoveAt(posicion);
        _capas.Insert(posicion + salto, capa);
    }

    private static IManejadorDanio Crear(TipoCapa capa, IAleatorio aleatorio) => capa switch
    {
        TipoCapa.Esquiva => new Esquiva(aleatorio),
        TipoCapa.Bloqueo => new Bloqueo(),
        TipoCapa.Armadura => new Armadura(),
        TipoCapa.Escudo => new Escudo(),
        _ => throw new ArgumentOutOfRangeException(nameof(capa)),
    };
}
