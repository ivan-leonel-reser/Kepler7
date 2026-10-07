namespace Kepler7.Core.Patrones.Estructurales.Bridge;

/// <summary>Parte de rutina. La Tierra responde con suministros.</summary>
public class MensajeTexto : Mensaje
{
    public const int CreditosDeSuministros = 40;

    public MensajeTexto(string contenido, ICifrado cifrado) : base(contenido, cifrado)
    {
    }

    public override string Tipo => "Texto";

    public override string Prioridad => "rutina";

    public override Refuerzo Refuerzo => new($"Suministros de rutina: +{CreditosDeSuministros} créditos", CreditosDeSuministros, 0);

    protected override string Encabezado => "[TEXTO · KEPLER-7]";
}
