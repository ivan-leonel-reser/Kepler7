using Kepler7.BE;

namespace Kepler7.DAL.Memoria;

public class HistorialPartidasEnMemoria : IHistorialPartidas
{
    private readonly List<PartidaJugada> _partidas = [];

    public void Registrar(PartidaJugada partida) => _partidas.Add(partida);

    public IReadOnlyList<PartidaJugada> Ultimas(int cantidad) =>
        _partidas.OrderByDescending(p => p.Fecha).Take(cantidad).ToList();
}
