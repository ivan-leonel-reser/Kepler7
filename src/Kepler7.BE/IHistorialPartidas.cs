namespace Kepler7.BE;

/// <summary>Las partidas terminadas. A diferencia del progreso, no se borra al empezar una partida nueva.</summary>
public interface IHistorialPartidas
{
    void Registrar(PartidaJugada partida);

    /// <summary>Las últimas partidas, de la más nueva a la más vieja.</summary>
    IReadOnlyList<PartidaJugada> Ultimas(int cantidad);
}
