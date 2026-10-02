using Kepler7.Core.Progreso;

namespace Kepler7.Core.Persistencia;

public interface IRepositorioProgreso
{
    /// <summary>Devuelve null si no hay partida guardada.</summary>
    ProgresoJugador? Cargar();

    void Guardar(ProgresoJugador progreso);

    void Borrar();
}
