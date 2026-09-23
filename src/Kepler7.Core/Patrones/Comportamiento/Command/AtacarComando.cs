using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Comportamiento.Command;

public class AtacarComando : IComando
{
    private readonly Unidad _objetivo;
    private int _vidaQuitada;

    public AtacarComando(Unidad atacante, Unidad objetivo)
    {
        Actor = atacante;
        _objetivo = objetivo;
    }

    public Unidad Actor { get; }
    public string Descripcion { get; private set; } = "";

    public void Ejecutar()
    {
        _vidaQuitada = _objetivo.RecibirGolpe(new Danio(Actor.Ataque, TipoDanio.Fisico, Actor));
        Descripcion = _objetivo.EstaViva
            ? $"{Actor.Nombre} ataca a {_objetivo.Nombre} (-{_vidaQuitada})"
            : $"{Actor.Nombre} derriba a {_objetivo.Nombre} (-{_vidaQuitada})";
    }

    // Se devuelve exactamente la vida que se quitó, aunque el objetivo haya caído.
    public void Deshacer() => _objetivo.Curar(_vidaQuitada);
}
