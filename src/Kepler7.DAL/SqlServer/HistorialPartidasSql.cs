using Kepler7.BE;
using Microsoft.Data.SqlClient;

namespace Kepler7.DAL.SqlServer;

/// <summary>El historial en la tabla PartidaJugada. Igual que el registro de la Bóveda, sobrevive a las partidas.</summary>
public class HistorialPartidasSql : IHistorialPartidas
{
    private readonly ConexionSql _conexion;

    public HistorialPartidasSql(ConexionSql conexion)
    {
        _conexion = conexion;
    }

    public void Registrar(PartidaJugada partida)
    {
        using var conexion = _conexion.Abrir();
        using var comando = new SqlCommand(
            "INSERT INTO dbo.PartidaJugada (Fecha, EstacionSalvada, OleadasSuperadas, Nivel, Xp, Insignias) " +
            "VALUES (@fecha, @salvada, @oleadas, @nivel, @xp, @insignias)",
            conexion);
        comando.Parameters.AddWithValue("@fecha", partida.Fecha);
        comando.Parameters.AddWithValue("@salvada", partida.EstacionSalvada);
        comando.Parameters.AddWithValue("@oleadas", partida.OleadasSuperadas);
        comando.Parameters.AddWithValue("@nivel", partida.Nivel);
        comando.Parameters.AddWithValue("@xp", partida.Xp);
        comando.Parameters.AddWithValue("@insignias", partida.Insignias);
        comando.ExecuteNonQuery();
    }

    public IReadOnlyList<PartidaJugada> Ultimas(int cantidad)
    {
        var partidas = new List<PartidaJugada>();
        using var conexion = _conexion.Abrir();
        using var comando = new SqlCommand(
            "SELECT TOP (@cantidad) Fecha, EstacionSalvada, OleadasSuperadas, Nivel, Xp, Insignias " +
            "FROM dbo.PartidaJugada ORDER BY Fecha DESC, Id DESC",
            conexion);
        comando.Parameters.AddWithValue("@cantidad", cantidad);
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            partidas.Add(new PartidaJugada(lector.GetDateTime(0), lector.GetBoolean(1), lector.GetInt32(2), lector.GetInt32(3), lector.GetInt32(4), lector.GetInt32(5)));
        return partidas;
    }
}
