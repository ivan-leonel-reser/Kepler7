using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.Command;

/// <summary>Levanta una barricada en la fila de adelante del escuadrón.</summary>
public class ConstruirComando : IComando
{
    private readonly CampoBatalla _campo;
    private Barricada? _barricada;

    public ConstruirComando(CampoBatalla campo, Unidad constructor)
    {
        _campo = campo;
        Actor = constructor;
    }

    public Unidad Actor { get; }
    public string Descripcion { get; private set; } = "";

    public void Ejecutar()
    {
        // Al rehacer se vuelve a poner la misma barricada, no una nueva.
        _barricada ??= new Barricada($"Barricada {_campo.Unidades.OfType<Barricada>().Count() + 1}");
        _campo.Agregar(_barricada, Fila.Vanguardia);
        Descripcion = $"{Actor.Nombre} levanta {_barricada.Nombre}";
    }

    public void Deshacer()
    {
        if (_barricada is not null)
            _campo.Quitar(_barricada);
    }
}
