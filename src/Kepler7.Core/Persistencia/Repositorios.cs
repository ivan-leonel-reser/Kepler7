using Kepler7.BE;
using Kepler7.DAL.Memoria;

namespace Kepler7.Core.Persistencia;

/// <summary>
/// El único lugar donde se elige qué implementación de la capa de datos usa el juego.
/// La interfaz se lo pide a Core y no conoce la capa de datos.
/// </summary>
public static class Repositorios
{
    // Mientras no esté SQL Server, el progreso se guarda solo mientras el juego está abierto.
    public static IRepositorioProgreso CrearProgreso() => new RepositorioProgresoEnMemoria();
}
