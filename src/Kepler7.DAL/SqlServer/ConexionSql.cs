using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace Kepler7.DAL.SqlServer;

/// <summary>
/// Abre conexiones con la cadena configurada y prepara la base: corre el script de <c>db/</c>,
/// que crea la base y las tablas solo si faltan.
/// </summary>
public class ConexionSql
{
    private const string BasePorDefecto = "Kepler7";
    private const int SegundosDeEspera = 5;
    private static readonly Regex SeparadorDeLotes = new(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

    private readonly string _cadena;

    public ConexionSql(string cadena)
    {
        var armador = new SqlConnectionStringBuilder(cadena);
        // Si la cadena no fija un tiempo de espera, no vale la pena esperar los 15 segundos por defecto.
        if (!armador.ShouldSerialize("Connect Timeout"))
            armador.ConnectTimeout = SegundosDeEspera;
        if (string.IsNullOrWhiteSpace(armador.InitialCatalog))
            armador.InitialCatalog = BasePorDefecto;

        Servidor = armador.DataSource;
        Base = armador.InitialCatalog;
        _cadena = armador.ConnectionString;
    }

    public string Servidor { get; }

    public string Base { get; }

    public SqlConnection Abrir()
    {
        var conexion = new SqlConnection(_cadena);
        try
        {
            conexion.Open();
            return conexion;
        }
        catch (SqlException e)
        {
            conexion.Dispose();
            throw new ErrorDePersistencia($"No se pudo conectar a {Servidor}, base {Base}: {e.Message}", e);
        }
    }

    /// <summary>Entra por master y corre el script de creación. Si la base y las tablas ya están, no hace nada.</summary>
    public void AsegurarBase()
    {
        var master = new SqlConnectionStringBuilder(_cadena) { InitialCatalog = "master" }.ConnectionString;
        try
        {
            using var conexion = new SqlConnection(master);
            conexion.Open();
            foreach (var lote in LotesDelScript())
            {
                using var comando = new SqlCommand(lote, conexion);
                comando.ExecuteNonQuery();
            }
        }
        catch (SqlException e)
        {
            throw new ErrorDePersistencia($"No se pudo preparar la base {Base} en {Servidor}: {e.Message}", e);
        }
    }

    // El script viene embebido desde db/01_crear_base.sql, así hay una sola versión. Se parte por los GO.
    private IEnumerable<string> LotesDelScript()
    {
        using var flujo = typeof(ConexionSql).Assembly.GetManifestResourceStream("crear_base.sql")
            ?? throw new InvalidOperationException("Falta el script de creación embebido en la capa de datos.");
        using var lector = new StreamReader(flujo);
        var script = lector.ReadToEnd().Replace(BasePorDefecto, Base);
        return SeparadorDeLotes.Split(script).Where(lote => !string.IsNullOrWhiteSpace(lote)).ToList();
    }
}
