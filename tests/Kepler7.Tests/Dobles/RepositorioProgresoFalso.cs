using Kepler7.Core.Persistencia;
using Kepler7.Core.Progreso;

namespace Kepler7.Tests.Dobles;

public class RepositorioProgresoFalso : IRepositorioProgreso
{
    public ProgresoJugador? Guardado { get; private set; }
    public int VecesGuardado { get; private set; }

    public ProgresoJugador? Cargar() => Guardado;

    public void Guardar(ProgresoJugador progreso)
    {
        Guardado = progreso;
        VecesGuardado++;
    }

    public void Borrar() => Guardado = null;
}
