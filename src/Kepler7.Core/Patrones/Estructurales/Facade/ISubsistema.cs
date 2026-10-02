namespace Kepler7.Core.Patrones.Estructurales.Facade;

// Solo para leer el estado. Cada subsistema tiene sus propias operaciones y solo la fachada las conoce.
internal interface ISubsistema
{
    string Nombre { get; }
    EstadoOperativo Estado { get; }
    string Detalle { get; }
}
