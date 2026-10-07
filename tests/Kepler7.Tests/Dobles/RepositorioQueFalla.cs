using Kepler7.BE;
using Kepler7.DAL;

namespace Kepler7.Tests.Dobles;

/// <summary>Se comporta como una base que se cayó después de arrancar: cargar anda, guardar y borrar fallan.</summary>
public class RepositorioQueFalla : IRepositorioProgreso
{
    public int IntentosDeGuardar { get; private set; }

    public ProgresoJugador? Cargar() => null;

    public void Guardar(ProgresoJugador progreso)
    {
        IntentosDeGuardar++;
        throw new ErrorDePersistencia("No se pudo conectar al servidor.");
    }

    public void Borrar() => throw new ErrorDePersistencia("No se pudo conectar al servidor.");
}
