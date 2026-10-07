using System.Text.Json;
using Kepler7.BE;
using Kepler7.DAL;
using Kepler7.DAL.Memoria;
using Kepler7.DAL.SqlServer;

namespace Kepler7.Core.Persistencia;

/// <summary>
/// El único lugar donde se elige qué implementación de la capa de datos usa el juego.
/// Con una cadena de conexión que responde se usa SQL Server; si no, todo queda en memoria y se avisa.
/// La interfaz se lo pide a Core y no conoce la capa de datos.
/// </summary>
public static class Repositorios
{
    private static readonly Lazy<Almacenamiento> Elegido = new(Elegir);

    public static IRepositorioProgreso CrearProgreso() => Elegido.Value.Progreso;

    // El registro de la bóveda es uno solo para toda la estación.
    public static IRegistroAccesos CrearRegistroAccesos() => Elegido.Value.Accesos;

    public static bool UsaSqlServer => Elegido.Value.Sql;

    /// <summary>Dónde se está guardando, para decírselo al jugador.</summary>
    public static string Descripcion => Elegido.Value.Descripcion;

    private static Almacenamiento Elegir()
    {
        string? cadena;
        try
        {
            cadena = ConfiguracionPersistencia.CadenaConexion();
        }
        catch (JsonException e)
        {
            return EnMemoria($"no se pudo leer {ConfiguracionPersistencia.Archivo} ({e.Message})");
        }
        if (cadena is null)
            return EnMemoria($"no hay {ConfiguracionPersistencia.Archivo} con la clave {ConfiguracionPersistencia.Clave}");

        try
        {
            var conexion = new ConexionSql(cadena);
            conexion.AsegurarBase();
            return new Almacenamiento(new RepositorioProgresoSql(conexion), new RegistroAccesosSql(conexion), true,
                $"SQL Server · {conexion.Servidor} · base {conexion.Base}");
        }
        catch (ErrorDePersistencia e)
        {
            return EnMemoria(e.Message);
        }
        catch (ArgumentException e)
        {
            return EnMemoria($"la cadena de conexión no es válida ({e.Message})");
        }
    }

    private static Almacenamiento EnMemoria(string motivo) =>
        new(new RepositorioProgresoEnMemoria(), new RegistroAccesosEnMemoria(), false,
            $"En memoria, se pierde al cerrar el juego: {motivo}");

    private sealed record Almacenamiento(IRepositorioProgreso Progreso, IRegistroAccesos Accesos, bool Sql, string Descripcion);
}
