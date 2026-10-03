using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;

/// <summary>Para en seco los golpes chicos, pero solo un par de veces por oleada.</summary>
public class Bloqueo : ManejadorDanio
{
    public const int UsosPorOleada = 2;
    public const int GolpeMaximo = 3;

    public override string Nombre => "Bloqueo";

    public int UsosRestantes { get; private set; } = UsosPorOleada;

    protected override string Absorber(Danio danio)
    {
        if (UsosRestantes == 0)
            return "ya no le quedan bloqueos";
        if (danio.Restante > GolpeMaximo)
            return $"el golpe es demasiado fuerte para bloquearlo (más de {GolpeMaximo})";

        danio.Restante = 0;
        UsosRestantes--;
        return $"bloquea el golpe (le quedan {UsosRestantes})";
    }
}
