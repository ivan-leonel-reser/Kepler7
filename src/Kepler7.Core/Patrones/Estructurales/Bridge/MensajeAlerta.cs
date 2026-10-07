namespace Kepler7.Core.Patrones.Estructurales.Bridge;

/// <summary>Aviso de ataque. La Tierra manda material para fortificar la próxima oleada.</summary>
public class MensajeAlerta : Mensaje
{
    public const int BarricadasDeRefuerzo = 1;

    public MensajeAlerta(string contenido, ICifrado cifrado) : base(contenido, cifrado)
    {
    }

    public override string Tipo => "Alerta";

    public override string Prioridad => "alta";

    public override Refuerzo Refuerzo => new($"Material de fortificación: +{BarricadasDeRefuerzo} barricada en la próxima oleada", 0, BarricadasDeRefuerzo);

    protected override string Encabezado => "[ALERTA · KEPLER-7 · PRIORIDAD ALTA]";
}
