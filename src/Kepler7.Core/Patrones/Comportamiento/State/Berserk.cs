using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.State;

/// <summary>Muy herida, la unidad pega más fuerte y se cuida menos. Se calma cuando recupera vida.</summary>
public class Berserk : EstadoPersonaje
{
    public const int BonusAtaque = 3;
    public const int DanioExtra = 2;

    public override string Nombre => "Berserk";

    public override string Color => "#FF5454";

    public override string Efecto => "pega y recibe más";

    /// <summary>Una unidad se pone furiosa cuando le queda menos de un tercio de la vida.</summary>
    public static bool Corresponde(Unidad unidad) => unidad.Vida * 3 < unidad.VidaMaxima;

    public override int Atacar(Unidad unidad, int danio) => danio + BonusAtaque;

    public override int RecibirDanio(Unidad unidad, Danio danio)
    {
        var entra = danio.Restante;
        danio.Restante += DanioExtra;
        danio.Registrar(Nombre, entra, $"recibe {DanioExtra} más");
        return base.RecibirDanio(unidad, danio);
    }

    public override string? TerminarTurno(Unidad unidad)
    {
        if (Corresponde(unidad))
            return null;

        unidad.CambiarEstado(new Normal());
        return "se calma";
    }
}
