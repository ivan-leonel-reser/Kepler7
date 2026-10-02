using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.Strategy;

/// <summary>Hace la cuenta antes de pegar: remata al que cae de un golpe y, si no, va por el que cae antes.</summary>
public class IACalculadora : IEstrategiaIA
{
    public string Nombre => "Calculadora";

    public string Descripcion => "Remata al que cae de un golpe; si no, va por el que cae antes.";

    public DecisionIA DecidirAccion(Enemigo enemigo, Contexto contexto)
    {
        if (!enemigo.PuedeAtacar)
            return contexto.Campo.HayLugar(enemigo.Bando, Fila.Vanguardia)
                ? DecisionIA.Avanzar("desde atrás no llega a nadie")
                : DecisionIA.Esperar("no tiene lugar adelante");

        var atacables = contexto.AtacablesPor(enemigo);
        if (atacables.Count == 0 || enemigo.Ataque <= 0)
            return DecisionIA.Esperar("no tiene a quién atacar");

        var remate = atacables
            .Where(u => u.Vida <= enemigo.Ataque)
            .OrderByDescending(u => u.Ataque)
            .FirstOrDefault();
        if (remate is not null)
            return DecisionIA.Atacar(remate, $"lo derriba de un golpe (vida {remate.Vida})");

        var objetivo = atacables
            .OrderBy(u => GolpesParaDerribar(u, enemigo))
            .ThenByDescending(u => u.Ataque)
            .First();
        return DecisionIA.Atacar(objetivo, $"es el que cae antes (en {GolpesParaDerribar(objetivo, enemigo)} golpes)");
    }

    private static int GolpesParaDerribar(Unidad objetivo, Enemigo enemigo) =>
        (objetivo.Vida + enemigo.Ataque - 1) / enemigo.Ataque;
}
