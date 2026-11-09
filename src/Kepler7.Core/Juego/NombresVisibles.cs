using Kepler7.BE;
using Kepler7.Core.Combate;

namespace Kepler7.Core.Juego;

/// <summary>Cómo se le muestran al jugador los nombres que en el código van sin tildes ni espacios.</summary>
public static class NombresVisibles
{
    public static string De(Patron patron) => patron switch
    {
        Patron.ChainOfResponsibility => "Chain of Responsibility",
        Patron.TemplateMethod => "Template Method",
        _ => patron.ToString(),
    };

    public static string De(ClaseUnidad clase) => clase == ClaseUnidad.Medico ? "Médico" : clase.ToString();
}
