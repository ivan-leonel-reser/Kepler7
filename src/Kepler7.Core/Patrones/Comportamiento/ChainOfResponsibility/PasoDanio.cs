namespace Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;

/// <summary>Lo que hizo una capa con un golpe: cuánto daño le llegó, cuánto dejó pasar y por qué.</summary>
public record PasoDanio(string Capa, int Entra, int Sale, string Detalle);
