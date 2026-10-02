using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.TemplateMethod;

/// <summary>Lo que el jugador le pidió a una unidad en su turno. Objetivo solo se usa al atacar o curar.</summary>
public record OrdenTurno(TipoOrden Tipo, Unidad? Objetivo = null);
