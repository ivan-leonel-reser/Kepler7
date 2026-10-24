using Kepler7.BE;
using Kepler7.DAL;

namespace Kepler7.Tests.Dobles;

/// <summary>Un registro de la bóveda con la base caída: no guarda ni devuelve nada.</summary>
public class RegistroQueFalla : IRegistroAccesos
{
    public void Registrar(IntentoAcceso intento) => throw new ErrorDePersistencia("No se pudo conectar al servidor.");

    public IReadOnlyList<IntentoAcceso> ObtenerTodos() => throw new ErrorDePersistencia("No se pudo conectar al servidor.");
}
