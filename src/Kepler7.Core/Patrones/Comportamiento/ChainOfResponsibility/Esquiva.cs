using Kepler7.Core.Combate;
using Kepler7.Services;

namespace Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;

/// <summary>A veces la unidad se corre a tiempo y el golpe no la toca. Es todo o nada.</summary>
public class Esquiva : ManejadorDanio
{
    public const int Probabilidad = 15;

    private readonly IAleatorio _aleatorio;

    public Esquiva(IAleatorio aleatorio)
    {
        _aleatorio = aleatorio;
    }

    public override string Nombre => "Esquiva";

    protected override string Absorber(Danio danio)
    {
        if (!_aleatorio.Probabilidad(Probabilidad))
            return "no llega a esquivar";

        danio.Restante = 0;
        return "esquiva el golpe entero";
    }
}
