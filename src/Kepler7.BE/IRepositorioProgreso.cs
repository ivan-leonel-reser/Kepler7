namespace Kepler7.BE;

public interface IRepositorioProgreso
{
    /// <summary>Devuelve null si no hay partida guardada.</summary>
    ProgresoJugador? Cargar();

    void Guardar(ProgresoJugador progreso);

    void Borrar();
}
