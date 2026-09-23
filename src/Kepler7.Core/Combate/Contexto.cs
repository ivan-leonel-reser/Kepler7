using Kepler7.Core.Comun;

namespace Kepler7.Core.Combate;

/// <summary>Lo que una unidad puede mirar para decidir qué hacer en su turno.</summary>
public class Contexto
{
    public Contexto(Grilla grilla, int numeroTurno, IAleatorio aleatorio)
    {
        Grilla = grilla;
        NumeroTurno = numeroTurno;
        Aleatorio = aleatorio;
    }

    public Grilla Grilla { get; }
    public int NumeroTurno { get; }
    public IAleatorio Aleatorio { get; }

    public IEnumerable<Unidad> RivalesDe(Unidad unidad) =>
        Grilla.UnidadesVivasDe(unidad.Bando == Bando.Escuadron ? Bando.Enjambre : Bando.Escuadron);

    public IEnumerable<Unidad> AliadosDe(Unidad unidad) =>
        Grilla.UnidadesVivasDe(unidad.Bando).Where(u => u != unidad);

    public Unidad? RivalMasCercano(Unidad unidad) =>
        RivalesDe(unidad).OrderBy(r => r.Posicion.DistanciaA(unidad.Posicion)).FirstOrDefault();
}
