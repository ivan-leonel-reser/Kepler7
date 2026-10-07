using System.Text.Json;

namespace Kepler7.Core.Persistencia;

/// <summary>Lee la cadena de conexión de <c>appsettings.json</c>, al lado del ejecutable. Sin archivo no hay SQL Server.</summary>
public static class ConfiguracionPersistencia
{
    public const string Archivo = "appsettings.json";
    public const string Clave = "ConexionSqlServer";

    public static string? CadenaConexion(string? directorio = null)
    {
        var ruta = Path.Combine(directorio ?? AppContext.BaseDirectory, Archivo);
        if (!File.Exists(ruta))
            return null;

        using var json = JsonDocument.Parse(File.ReadAllText(ruta));
        return json.RootElement.TryGetProperty(Clave, out var valor) && valor.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(valor.GetString())
            ? valor.GetString()
            : null;
    }
}
