using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.Command;

public class CurarComando : IComando
{
    private readonly Unidad _objetivo;
    private readonly int _cantidad;
    private int _vidaCurada;

    public CurarComando(Unidad medico, Unidad objetivo, int cantidad)
    {
        Actor = medico;
        _objetivo = objetivo;
        _cantidad = cantidad;
    }

    public Unidad Actor { get; }
    public string Descripcion { get; private set; } = "";

    public void Ejecutar()
    {
        _vidaCurada = _objetivo.Curar(_cantidad);
        Descripcion = $"{Actor.Nombre} cura a {_objetivo.Nombre} (+{_vidaCurada})";
    }

    public void Deshacer() => _objetivo.PerderVida(_vidaCurada);
}
