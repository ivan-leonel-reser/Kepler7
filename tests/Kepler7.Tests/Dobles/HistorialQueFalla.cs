using Kepler7.BE;
using Kepler7.DAL;

namespace Kepler7.Tests.Dobles;

/// <summary>Un historial con la base caída: nunca puede anotar.</summary>
public class HistorialQueFalla : IHistorialPartidas
{
    public void Registrar(PartidaJugada partida) => throw new ErrorDePersistencia("No se pudo conectar al servidor.");

    public IReadOnlyList<PartidaJugada> Ultimas(int cantidad) => throw new ErrorDePersistencia("No se pudo conectar al servidor.");
}
