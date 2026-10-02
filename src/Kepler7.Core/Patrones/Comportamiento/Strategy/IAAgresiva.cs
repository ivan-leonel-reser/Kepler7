using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.Strategy;

/// <summary>Busca el frente y le pega siempre al rival que más daño hace.</summary>
public class IAAgresiva : IEstrategiaIA
{
    public string Nombre => "Agresiva";

    public string Descripcion => "Va al frente y ataca al rival que más pega.";

    public DecisionIA DecidirAccion(Enemigo enemigo, Contexto contexto)
    {
        if (!enemigo.PuedeAtacar)
            return contexto.Campo.HayLugar(enemigo.Bando, Fila.Vanguardia)
                ? DecisionIA.Avanzar("quiere pelear de cerca")
                : DecisionIA.Esperar("no tiene lugar adelante");

        var objetivo = contexto.AtacablesPor(enemigo)
            .OrderByDescending(u => u.Ataque)
            .ThenBy(u => u.Vida)
            .FirstOrDefault();
        if (objetivo is null)
            return DecisionIA.Esperar("no tiene a quién atacar");

        // Una barricada no ataca: si es lo único que hay adelante, la rompe para pasar.
        return objetivo.Ataque > 0
            ? DecisionIA.Atacar(objetivo, $"es el que más pega (ataque {objetivo.Ataque})")
            : DecisionIA.Atacar(objetivo, "le tapa el paso");
    }
}
