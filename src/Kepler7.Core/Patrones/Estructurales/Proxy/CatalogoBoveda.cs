using Kepler7.BE;
using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Estructurales.Proxy;

/// <summary>Las credenciales de la estación y los archivos que guarda la bóveda: un informe por oleada y el diario del capitán.</summary>
public static class CatalogoBoveda
{
    public const string DiarioDelCapitan = "diario-del-capitan";

    public static IReadOnlyList<Credencial> Credenciales { get; } =
    [
        new("Vega", Rol.Capitan),
        new("Ortega", Rol.OficialDeSeguridad),
        new("Paz", Rol.Medico),
        new("Rojas", Rol.Soldado),
        new("Nadir", Rol.Cadete),
    ];

    public static IReadOnlyList<ArchivoBoveda> Archivos { get; } =
        Enumerable.Range(1, ProgresoJugador.OleadasTotales)
            .Select(n => new ArchivoBoveda(InformeOleada(n), $"Informe de inteligencia · oleada {n}"))
            .Append(new ArchivoBoveda(DiarioDelCapitan, "Diario del capitán"))
            .ToList();

    public static string InformeOleada(int numero) => $"informe-oleada-{numero}";

    /// <summary>Si el id es el informe de una oleada, devuelve su número.</summary>
    public static int? OleadaDelInforme(string id) =>
        id.StartsWith("informe-oleada-") && int.TryParse(id["informe-oleada-".Length..], out var numero) ? numero : null;

    /// <summary>Arma la bóveda real con los informes de las seis oleadas. Se llama recién cuando el proxy autoriza el primer acceso.</summary>
    public static BovedaDatos CrearBoveda()
    {
        var archivos = Enumerable.Range(1, ProgresoJugador.OleadasTotales)
            .Select(n => KeyValuePair.Create(InformeOleada(n), RedactarInforme(n)))
            .Append(KeyValuePair.Create(DiarioDelCapitan, "Sin anotaciones."));
        return new BovedaDatos(archivos);
    }

    // El informe cuenta qué viene en la oleada y qué IA usa cada enemigo: lo mismo que la Exploradora revela en combate.
    private static string RedactarInforme(int numero)
    {
        var enemigos = GeneradorOleadas.Crear(numero);
        var lineas = enemigos.Select(e => $"- {e.Nombre}: vida {e.VidaMaxima}, ataque {e.Ataque}. IA {e.Estrategia.Nombre}: {e.Estrategia.Descripcion}");
        return string.Join(Environment.NewLine,
            new[] { $"RESERVADO · Oleada {numero}: {enemigos.Count} enemigos detectados." }.Concat(lineas));
    }
}
