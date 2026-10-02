using Kepler7.BE;

namespace Kepler7.Core.Juego;

/// <summary>Cómo terminó una oleada y lo que hizo el Control Central después.</summary>
public record ResultadoOleada(bool Victoria, int Xp, int Creditos, bool SubioNivel, IReadOnlyList<EntradaBitacora> SecuenciaEstacion);
