using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.Strategy;

/// <summary>No sigue ningún plan: sortea entre todo lo que puede hacer en ese momento.</summary>
public class IAAleatoria : IEstrategiaIA
{
    public string Nombre => "Aleatoria";

    public string Descripcion => "Sortea entre atacar a cualquiera, cambiar de fila o esperar.";

    public DecisionIA DecidirAccion(Enemigo enemigo, Contexto contexto)
    {
        var opciones = new List<DecisionIA>();

        if (enemigo.PuedeAtacar)
            opciones.AddRange(contexto.AtacablesPor(enemigo).Select(u => DecisionIA.Atacar(u, "")));

        var otraFila = enemigo.Fila == Fila.Vanguardia ? Fila.Retaguardia : Fila.Vanguardia;
        if (contexto.Campo.HayLugar(enemigo.Bando, otraFila))
            opciones.Add(otraFila == Fila.Vanguardia ? DecisionIA.Avanzar("") : DecisionIA.Retroceder(""));

        opciones.Add(DecisionIA.Esperar(""));

        // El azar viene del contexto para poder fijarlo en las pruebas.
        var elegida = contexto.Aleatorio.Siguiente(0, opciones.Count);
        return opciones[elegida] with { Motivo = $"salió al azar la opción {elegida + 1} de {opciones.Count}" };
    }
}
