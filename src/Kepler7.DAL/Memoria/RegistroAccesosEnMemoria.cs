using Kepler7.BE;

namespace Kepler7.DAL.Memoria;

public class RegistroAccesosEnMemoria : IRegistroAccesos
{
    private readonly List<IntentoAcceso> _intentos = [];

    public void Registrar(IntentoAcceso intento) => _intentos.Add(intento);

    public IReadOnlyList<IntentoAcceso> ObtenerTodos() => _intentos.ToList();
}
