using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.Strategy;

/// <summary>Cuida su vida: con menos de la mitad se repliega y no vuelve al frente.</summary>
public class IADefensiva : IEstrategiaIA
{
    public string Nombre => "Defensiva";

    public string Descripcion => "Herida se repliega atrás; entera ataca al más débil.";

    public DecisionIA DecidirAccion(Enemigo enemigo, Contexto contexto)
    {
        var herido = enemigo.Vida * 2 < enemigo.VidaMaxima;
        var vida = $"{enemigo.Vida}/{enemigo.VidaMaxima}";

        if (herido && enemigo.Fila == Fila.Vanguardia && contexto.Campo.HayLugar(enemigo.Bando, Fila.Retaguardia))
            return DecisionIA.Retroceder($"está herido ({vida}) y se cubre atrás");

        if (!enemigo.PuedeAtacar)
        {
            if (herido)
                return DecisionIA.Esperar($"está herido ({vida}) y no se expone");

            return contexto.Campo.HayLugar(enemigo.Bando, Fila.Vanguardia)
                ? DecisionIA.Avanzar("está entero y vuelve al frente")
                : DecisionIA.Esperar("no tiene lugar adelante");
        }

        var objetivo = contexto.AtacableMasDebil(enemigo);
        return objetivo is null
            ? DecisionIA.Esperar("no tiene a quién atacar")
            : DecisionIA.Atacar(objetivo, $"va por el más débil (vida {objetivo.Vida})");
    }
}
