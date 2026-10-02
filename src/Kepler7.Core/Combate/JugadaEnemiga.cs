using Kepler7.Core.Comun;

namespace Kepler7.Core.Combate;

/// <summary>Lo que hizo un enemigo en su turno. Objetivo es null si no atacó a nadie.</summary>
public record JugadaEnemiga(Enemigo Enemigo, Unidad? Objetivo, EntradaBitacora Entrada);
