using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.Command;

/// <summary>Cambia a la unidad de fila (adelante / atrás).</summary>
public class MoverComando : IComando
{
    private readonly CampoBatalla _campo;
    private readonly Fila _destino;
    private Fila _origen;

    public MoverComando(CampoBatalla campo, Unidad unidad, Fila destino)
    {
        _campo = campo;
        Actor = unidad;
        _destino = destino;
    }

    public Unidad Actor { get; }
    public string Descripcion { get; private set; } = "";

    public void Ejecutar()
    {
        _origen = Actor.Fila;
        _campo.Mover(Actor, _destino);
        Descripcion = $"{Actor.Nombre} pasa {(_destino == Fila.Vanguardia ? "adelante" : "atrás")}";
    }

    public void Deshacer() => _campo.Mover(Actor, _origen);
}
