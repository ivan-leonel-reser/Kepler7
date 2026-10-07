namespace Kepler7.Core.Progreso;

/// <summary>Lo que se gana al llegar a un nivel. Los extras se suman a cada oleada desde ese nivel.</summary>
public record Desbloqueo(int Nivel, string Nombre, string Descripcion, int HackeosExtra = 0, int BarricadasExtra = 0, int CreditosExtraPorOleada = 0);
