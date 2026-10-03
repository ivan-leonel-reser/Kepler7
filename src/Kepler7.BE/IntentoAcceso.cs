namespace Kepler7.BE;

public record IntentoAcceso(DateTime Fecha, string Usuario, string Rol, string Operacion, string IdArchivo);
