using Kepler7.BE;

namespace Kepler7.Tests.Dobles;

public class RegistroAccesosFalso : IRegistroAccesos
{
    public List<IntentoAcceso> Intentos { get; } = [];

    public void Registrar(IntentoAcceso intento) => Intentos.Add(intento);

    public IReadOnlyList<IntentoAcceso> ObtenerTodos() => Intentos;
}
