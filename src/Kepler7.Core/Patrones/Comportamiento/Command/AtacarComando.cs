using Kepler7.Core.Combate;
using Kepler7.Core.Patrones.Comportamiento.State;

namespace Kepler7.Core.Patrones.Comportamiento.Command;

public class AtacarComando : IComando
{
    private readonly Unidad _objetivo;
    private readonly int _bonus;
    private int _vidaQuitada;
    private IEstadoPersonaje? _estadoAnterior;

    /// <param name="bonus">Daño extra de este ataque, por encima del ataque de la unidad.</param>
    public AtacarComando(Unidad atacante, Unidad objetivo, int bonus = 0)
    {
        Actor = atacante;
        _objetivo = objetivo;
        _bonus = bonus;
    }

    public Unidad Actor { get; }
    public string Descripcion { get; private set; } = "";

    public void Ejecutar()
    {
        _estadoAnterior = _objetivo.Estado;
        _vidaQuitada = _objetivo.RecibirGolpe(new Danio(Actor.DanioDeAtaque(_bonus), TipoDanio.Fisico, Actor));
        Descripcion = _objetivo.EstaViva
            ? $"{Actor.Nombre} ataca a {_objetivo.Nombre} (-{_vidaQuitada})"
            : $"{Actor.Nombre} derriba a {_objetivo.Nombre} (-{_vidaQuitada})";
    }

    // Se devuelve exactamente la vida que se quitó, aunque el objetivo haya caído,
    // y el objetivo vuelve al estado que tenía antes del golpe.
    public void Deshacer()
    {
        _objetivo.Curar(_vidaQuitada);
        if (_estadoAnterior is not null)
            _objetivo.CambiarEstado(_estadoAnterior);
    }
}
