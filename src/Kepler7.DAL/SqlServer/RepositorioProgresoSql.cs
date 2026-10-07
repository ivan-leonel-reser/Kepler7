using Kepler7.BE;
using Microsoft.Data.SqlClient;

namespace Kepler7.DAL.SqlServer;

/// <summary>Guarda la única partida en la fila 1 de Progreso y sus insignias en Insignia.</summary>
public class RepositorioProgresoSql : IRepositorioProgreso
{
    private readonly ConexionSql _conexion;

    public RepositorioProgresoSql(ConexionSql conexion)
    {
        _conexion = conexion;
    }

    public ProgresoJugador? Cargar()
    {
        using var conexion = _conexion.Abrir();

        int xp, creditos, oleadas;
        using (var comando = new SqlCommand("SELECT Xp, Creditos, OleadasSuperadas FROM dbo.Progreso WHERE Id = 1", conexion))
        using (var lector = comando.ExecuteReader())
        {
            if (!lector.Read())
                return null;
            xp = lector.GetInt32(0);
            creditos = lector.GetInt32(1);
            oleadas = lector.GetInt32(2);
        }

        var insignias = new List<Patron>();
        using (var comando = new SqlCommand("SELECT Patron FROM dbo.Insignia", conexion))
        using (var lector = comando.ExecuteReader())
        {
            while (lector.Read())
            {
                if (Enum.TryParse<Patron>(lector.GetString(0), out var patron))
                    insignias.Add(patron);
            }
        }

        return ProgresoJugador.Restaurar(xp, creditos, oleadas, insignias);
    }

    // Todo en una transacción: o queda la partida entera o no cambia nada.
    public void Guardar(ProgresoJugador progreso)
    {
        using var conexion = _conexion.Abrir();
        using var transaccion = conexion.BeginTransaction();

        using (var comando = new SqlCommand(
            """
            IF EXISTS (SELECT 1 FROM dbo.Progreso WHERE Id = 1)
                UPDATE dbo.Progreso SET Xp = @xp, Creditos = @creditos, OleadasSuperadas = @oleadas, Actualizado = SYSDATETIME() WHERE Id = 1;
            ELSE
                INSERT INTO dbo.Progreso (Id, Xp, Creditos, OleadasSuperadas) VALUES (1, @xp, @creditos, @oleadas);
            DELETE FROM dbo.Insignia;
            """, conexion, transaccion))
        {
            comando.Parameters.AddWithValue("@xp", progreso.Xp);
            comando.Parameters.AddWithValue("@creditos", progreso.Creditos);
            comando.Parameters.AddWithValue("@oleadas", progreso.OleadasSuperadas);
            comando.ExecuteNonQuery();
        }

        foreach (var patron in progreso.Insignias)
        {
            using var comando = new SqlCommand("INSERT INTO dbo.Insignia (Patron) VALUES (@patron)", conexion, transaccion);
            comando.Parameters.AddWithValue("@patron", patron.ToString());
            comando.ExecuteNonQuery();
        }

        transaccion.Commit();
    }

    public void Borrar()
    {
        using var conexion = _conexion.Abrir();
        using var comando = new SqlCommand("DELETE FROM dbo.Insignia; DELETE FROM dbo.Progreso;", conexion);
        comando.ExecuteNonQuery();
    }
}
