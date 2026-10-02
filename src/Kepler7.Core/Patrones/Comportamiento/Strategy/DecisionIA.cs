using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.Strategy;

/// <summary>Lo que una IA decide para el turno de un enemigo y por qué. Objetivo solo se usa al atacar.</summary>
public record DecisionIA(AccionIA Accion, Unidad? Objetivo, string Motivo)
{
    public static DecisionIA Atacar(Unidad objetivo, string motivo) => new(AccionIA.Atacar, objetivo, motivo);

    public static DecisionIA Avanzar(string motivo) => new(AccionIA.Avanzar, null, motivo);

    public static DecisionIA Retroceder(string motivo) => new(AccionIA.Retroceder, null, motivo);

    public static DecisionIA Esperar(string motivo) => new(AccionIA.Esperar, null, motivo);
}
