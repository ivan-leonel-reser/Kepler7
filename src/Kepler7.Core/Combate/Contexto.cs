using Kepler7.Services;

namespace Kepler7.Core.Combate;

/// <summary>Lo que una unidad puede mirar para decidir qué hacer en su turno.</summary>
public class Contexto
{
    public Contexto(CampoBatalla campo, int numeroTurno, IAleatorio aleatorio)
    {
        Campo = campo;
        NumeroTurno = numeroTurno;
        Aleatorio = aleatorio;
    }

    public CampoBatalla Campo { get; }
    public int NumeroTurno { get; }
    public IAleatorio Aleatorio { get; }

    public IEnumerable<Unidad> RivalesDe(Unidad unidad) =>
        Campo.UnidadesVivasDe(CampoBatalla.RivalDe(unidad.Bando));

    public IEnumerable<Unidad> AliadosDe(Unidad unidad) =>
        Campo.UnidadesVivasDe(unidad.Bando).Where(u => u != unidad);

    public IReadOnlyList<Unidad> AtacablesPor(Unidad unidad) => Campo.Atacables(unidad.Bando);

    public Unidad? AtacableMasDebil(Unidad unidad) =>
        AtacablesPor(unidad).OrderBy(u => u.Vida).FirstOrDefault();
}
