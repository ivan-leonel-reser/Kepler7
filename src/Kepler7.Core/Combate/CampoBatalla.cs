namespace Kepler7.Core.Combate;

/// <summary>Donde se pelea una oleada: cada bando tiene una fila de adelante y una de atrás.</summary>
public class CampoBatalla
{
    public const int LugaresPorFila = 3;

    private readonly List<Unidad> _unidades = [];

    // Incluye a las unidades caídas.
    public IReadOnlyList<Unidad> Unidades => _unidades;

    public static Bando RivalDe(Bando bando) => bando == Bando.Escuadron ? Bando.Enjambre : Bando.Escuadron;

    public IEnumerable<Unidad> UnidadesVivasDe(Bando bando) =>
        _unidades.Where(u => u.Bando == bando && u.EstaViva);

    public IEnumerable<Unidad> EnFila(Bando bando, Fila fila) =>
        UnidadesVivasDe(bando).Where(u => u.Fila == fila);

    public bool HayLugar(Bando bando, Fila fila) => EnFila(bando, fila).Count() < LugaresPorFila;

    public void Agregar(Unidad unidad, Fila fila)
    {
        if (_unidades.Contains(unidad))
            throw new InvalidOperationException($"{unidad.Nombre} ya está en el campo.");
        if (!HayLugar(unidad.Bando, fila))
            throw new InvalidOperationException($"No hay lugar en la {fila}.");

        unidad.Fila = fila;
        _unidades.Add(unidad);
    }

    public void Quitar(Unidad unidad) => _unidades.Remove(unidad);

    public void Mover(Unidad unidad, Fila destino)
    {
        if (!_unidades.Contains(unidad))
            throw new InvalidOperationException($"{unidad.Nombre} no está en el campo.");
        if (unidad.Fila == destino)
            return;
        if (!HayLugar(unidad.Bando, destino))
            throw new InvalidOperationException($"No hay lugar en la {destino}.");

        unidad.Fila = destino;
    }

    /// <summary>
    /// A quién puede atacar un bando: la fila de adelante del rival.
    /// Si adelante no queda nadie, queda expuesta la de atrás.
    /// </summary>
    public IReadOnlyList<Unidad> Atacables(Bando atacante)
    {
        var rival = RivalDe(atacante);
        var adelante = EnFila(rival, Fila.Vanguardia).ToList();
        return adelante.Count > 0 ? adelante : EnFila(rival, Fila.Retaguardia).ToList();
    }
}
