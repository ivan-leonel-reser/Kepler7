using Kepler7.BE;
using Microsoft.Data.SqlClient;

namespace Kepler7.DAL.SqlServer;

/// <summary>El registro de la Bóveda en la tabla IntentoAcceso. Nunca se borra: sobrevive a las partidas.</summary>
public class RegistroAccesosSql : IRegistroAccesos
{
    private readonly ConexionSql _conexion;

    public RegistroAccesosSql(ConexionSql conexion)
    {
        _conexion = conexion;
    }

    public void Registrar(IntentoAcceso intento)
    {
        using var conexion = _conexion.Abrir();
        using var comando = new SqlCommand(
            "INSERT INTO dbo.IntentoAcceso (Fecha, Usuario, Rol, Operacion, IdArchivo) VALUES (@fecha, @usuario, @rol, @operacion, @archivo)",
            conexion);
        comando.Parameters.AddWithValue("@fecha", intento.Fecha);
        comando.Parameters.AddWithValue("@usuario", intento.Usuario);
        comando.Parameters.AddWithValue("@rol", intento.Rol);
        comando.Parameters.AddWithValue("@operacion", intento.Operacion);
        comando.Parameters.AddWithValue("@archivo", intento.IdArchivo);
        comando.ExecuteNonQuery();
    }

    public IReadOnlyList<IntentoAcceso> ObtenerTodos()
    {
        var intentos = new List<IntentoAcceso>();
        using var conexion = _conexion.Abrir();
        using var comando = new SqlCommand("SELECT Fecha, Usuario, Rol, Operacion, IdArchivo FROM dbo.IntentoAcceso ORDER BY Fecha, Id", conexion);
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            intentos.Add(new IntentoAcceso(lector.GetDateTime(0), lector.GetString(1), lector.GetString(2), lector.GetString(3), lector.GetString(4)));
        return intentos;
    }
}
