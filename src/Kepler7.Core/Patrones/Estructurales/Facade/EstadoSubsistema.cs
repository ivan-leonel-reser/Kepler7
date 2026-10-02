namespace Kepler7.Core.Patrones.Estructurales.Facade;

/// <summary>Foto del estado de un subsistema para mostrar en pantalla (la UI no toca los subsistemas).</summary>
public record EstadoSubsistema(string Nombre, EstadoOperativo Estado, string Detalle);
