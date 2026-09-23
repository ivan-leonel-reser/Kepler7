namespace Kepler7.Core.Combate;

/// <summary>La levanta el escuadrón en su fila de adelante. No ataca, solo se come los golpes.</summary>
public class Barricada : Unidad
{
    public const int VidaInicial = 15;

    public Barricada(string nombre)
        : base(nombre, Bando.Escuadron, VidaInicial, ataque: 0, atacaADistancia: false)
    {
    }
}
