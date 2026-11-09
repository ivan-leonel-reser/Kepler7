using Kepler7.BE;

namespace Kepler7.Core.Patrones.Estructurales.Proxy;

/// <summary>El proxy rechazó la operación. Salvo que la base no responda, el intento ya quedó en el registro.</summary>
public class AccesoDenegadoException : InvalidOperationException
{
    public AccesoDenegadoException(IntentoAcceso intento, bool registrado = true)
        : base($"Acceso denegado: {intento.Usuario} ({intento.Rol}) no puede {intento.Operacion.ToLowerInvariant()} '{intento.IdArchivo}'.")
    {
        Intento = intento;
        Registrado = registrado;
    }

    public IntentoAcceso Intento { get; }

    /// <summary>false si el registro no pudo guardar el intento.</summary>
    public bool Registrado { get; }
}
