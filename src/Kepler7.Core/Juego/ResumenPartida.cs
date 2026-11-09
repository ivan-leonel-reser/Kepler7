using Kepler7.BE;

namespace Kepler7.Core.Juego;

/// <summary>Lo que muestra la pantalla final: cómo terminó la partida y qué se hizo en el camino.</summary>
public record ResumenPartida(
    bool EstacionSalvada,
    int OleadasSuperadas,
    int Nivel,
    int Xp,
    int Creditos,
    IReadOnlyList<Insignia> Insignias,
    int Transmisiones,
    int AccesosALaBoveda,
    int? IntentosDenegados,
    string? Recluta,
    string? Clon,
    IReadOnlyList<RegistroOleada> Oleadas);
