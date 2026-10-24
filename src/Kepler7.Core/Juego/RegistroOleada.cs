namespace Kepler7.Core.Juego;

/// <summary>Cómo le fue al escuadrón en una oleada, para la tabla de la pantalla final.</summary>
public record RegistroOleada(
    int Numero,
    bool Victoria,
    int Turnos,
    int Enemigos,
    int EnemigosDerrotados,
    int Bajas,
    int GolpesAbsorbidos,
    int Hackeos,
    int Xp,
    int Creditos);
