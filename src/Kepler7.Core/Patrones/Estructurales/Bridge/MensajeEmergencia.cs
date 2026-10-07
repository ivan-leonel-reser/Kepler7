namespace Kepler7.Core.Patrones.Estructurales.Bridge;

/// <summary>Pedido de auxilio. La Tierra manda un convoy: créditos y material.</summary>
public class MensajeEmergencia : Mensaje
{
    public const int CreditosDelConvoy = 60;
    public const int BarricadasDelConvoy = 2;

    public MensajeEmergencia(string contenido, ICifrado cifrado) : base(contenido, cifrado)
    {
    }

    public override string Tipo => "Emergencia";

    public override string Prioridad => "máxima";

    public override Refuerzo Refuerzo =>
        new($"Convoy de emergencia: +{CreditosDelConvoy} créditos y +{BarricadasDelConvoy} barricadas en la próxima oleada", CreditosDelConvoy, BarricadasDelConvoy);

    protected override string Encabezado => "[EMERGENCIA · KEPLER-7 · PRIORIDAD MÁXIMA] Solicitamos refuerzos inmediatos.";
}
