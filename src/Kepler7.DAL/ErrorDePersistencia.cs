namespace Kepler7.DAL;

/// <summary>La capa de datos no pudo hablar con SQL Server. El mensaje está pensado para mostrárselo al jugador.</summary>
public class ErrorDePersistencia : Exception
{
    public ErrorDePersistencia(string mensaje, Exception? causa = null) : base(mensaje, causa)
    {
    }
}
