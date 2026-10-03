namespace Kepler7.BE;

/// <summary>Una línea de lo que pasó en el juego. La UI solo la pinta según su tipo.</summary>
public record EntradaBitacora(string Origen, string Mensaje, TipoEntrada Tipo = TipoEntrada.Info)
{
    public override string ToString() => $"[{Origen}] {Mensaje}";
}
