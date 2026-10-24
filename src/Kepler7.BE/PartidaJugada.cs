namespace Kepler7.BE;

/// <summary>Una partida que ya terminó, ganada o perdida, como queda en el historial.</summary>
public record PartidaJugada(DateTime Fecha, bool EstacionSalvada, int OleadasSuperadas, int Nivel, int Xp, int Insignias);
