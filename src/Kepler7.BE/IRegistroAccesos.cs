namespace Kepler7.BE;

/// <summary>Registro persistente de los intentos denegados a la bóveda (patrón Proxy).</summary>
public interface IRegistroAccesos
{
    void Registrar(IntentoAcceso intento);

    IReadOnlyList<IntentoAcceso> ObtenerTodos();
}
