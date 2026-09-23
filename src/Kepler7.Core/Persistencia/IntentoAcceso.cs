namespace Kepler7.Core.Persistencia;

public record IntentoAcceso(DateTime Fecha, string Usuario, string Rol, string Operacion, string IdArchivo);
