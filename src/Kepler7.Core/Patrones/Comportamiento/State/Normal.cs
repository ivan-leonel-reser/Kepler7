using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.State;

/// <summary>La unidad pelea como siempre. Desde acá un golpe la puede dejar furiosa, envenenada o aturdida.</summary>
public class Normal : EstadoPersonaje
{
    private bool _recienRecuperada;

    /// <param name="recienRecuperada">Viene de estar aturdida: hasta su próximo turno no la pueden volver a aturdir.</param>
    public Normal(bool recienRecuperada = false)
    {
        _recienRecuperada = recienRecuperada;
    }

    public override string Nombre => "Normal";

    public override string Color => "#DEE7F2";

    public override string Efecto => "pelea como siempre";

    public override int RecibirDanio(Unidad unidad, Danio danio)
    {
        var vidaPerdida = base.RecibirDanio(unidad, danio);
        if (vidaPerdida == 0 || !unidad.EstaViva)
            return vidaPerdida;

        // Muy herida se pone furiosa; si no, queda con lo que le deje quien la golpeó.
        var efecto = danio.Atacante?.EfectoAlGolpear ?? EfectoGolpe.Ninguno;
        if (Berserk.Corresponde(unidad))
            unidad.CambiarEstado(new Berserk());
        else if (efecto == EfectoGolpe.Veneno)
            unidad.CambiarEstado(new Envenenado());
        else if (efecto == EfectoGolpe.Aturdimiento && !_recienRecuperada)
            unidad.CambiarEstado(new Aturdido());

        return vidaPerdida;
    }

    public override string? TerminarTurno(Unidad unidad)
    {
        _recienRecuperada = false;
        return null;
    }
}
