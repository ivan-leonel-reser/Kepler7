namespace Kepler7.Core.Patrones.Estructurales.Bridge;

/// <summary>Las dos jerarquías del puente, listas para combinarse: tres tipos de mensaje por tres cifrados.</summary>
public static class CatalogoComunicaciones
{
    public static IReadOnlyList<ICifrado> Cifrados { get; } =
    [
        new SinCifrado(),
        new CifradoAES(),
        new CifradoCuantico(),
    ];

    public static IReadOnlyList<TipoMensaje> Tipos { get; } =
    [
        new("Texto", "Parte de rutina. La Tierra responde con suministros.",
            "Kepler-7 operativa. Escuadrón en posición.", (contenido, cifrado) => new MensajeTexto(contenido, cifrado)),
        new("Alerta", "Aviso de ataque. La Tierra manda material para fortificar la próxima oleada.",
            "Enjambre detectado en el sector 7. Preparamos la defensa.", (contenido, cifrado) => new MensajeAlerta(contenido, cifrado)),
        new("Emergencia", "Pedido de auxilio. La Tierra manda un convoy con créditos y material.",
            "Bajas en la vanguardia. Necesitamos apoyo ya.", (contenido, cifrado) => new MensajeEmergencia(contenido, cifrado)),
    ];
}
