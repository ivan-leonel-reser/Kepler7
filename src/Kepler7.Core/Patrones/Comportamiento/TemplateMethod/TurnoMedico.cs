using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.TemplateMethod;

/// <summary>Cuida al escuadrón: si el herido está grave, la curación rinde más.</summary>
public class TurnoMedico : TurnoUnidad
{
    public const int BonusUrgencia = 5;

    public TurnoMedico(UnidadEscuadron unidad, CampoBatalla campo) : base(unidad, campo)
    {
    }

    public override string Habilidad => $"A un herido grave lo cura +{BonusUrgencia}.";

    protected override string Iniciar()
    {
        var herido = Campo.UnidadesVivasDe(Bando.Escuadron)
            .OfType<UnidadEscuadron>()
            .Where(u => u.Vida < u.VidaMaxima)
            .OrderBy(u => u.Vida)
            .FirstOrDefault();
        return herido is null
            ? "revisa al escuadrón: no hay heridos"
            : $"revisa al escuadrón: el más herido es {herido.Nombre} ({herido.Vida}/{herido.VidaMaxima})";
    }

    protected override string Decidir(OrdenTurno orden)
    {
        if (orden.Tipo != TipoOrden.Curar || orden.Objetivo is not { } herido)
            return Describir(orden);
        if (herido.Vida * 2 >= herido.VidaMaxima)
            return $"cura a {herido.Nombre}";

        Bonus = BonusUrgencia;
        return $"atiende de urgencia a {herido.Nombre} (+{BonusUrgencia} de curación)";
    }

    protected override string Finalizar() => "guarda el botiquín";
}
