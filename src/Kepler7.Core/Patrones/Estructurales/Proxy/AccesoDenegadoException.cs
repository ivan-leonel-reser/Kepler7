using Kepler7.BE;

namespace Kepler7.Core.Patrones.Estructurales.Proxy;

/// <summary>El proxy rechazó la operación. El intento ya quedó en el registro.</summary>
public class AccesoDenegadoException : InvalidOperationException
{
    public AccesoDenegadoException(IntentoAcceso intento)
        : base($"Acceso denegado: {intento.Usuario} ({intento.Rol}) no puede {intento.Operacion.ToLowerInvariant()} '{intento.IdArchivo}'.")
    {
        Intento = intento;
    }

    public IntentoAcceso Intento { get; }
}
