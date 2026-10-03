using Kepler7.BE;
using Kepler7.Core.Patrones.Comportamiento.Strategy;

namespace Kepler7.Core.Combate;

/// <summary>
/// Lo que hizo un enemigo en su turno. Objetivo es null si no atacó a nadie.
/// IA es el nombre de la estrategia que tenía cuando decidió.
/// </summary>
public record JugadaEnemiga(Enemigo Enemigo, Unidad? Objetivo, EntradaBitacora Entrada, string IA, DecisionIA Decision);
