using Kepler7.Core.Combate;

namespace Kepler7.Core.Patrones.Creacionales.Prototype;

/// <summary>Las habilidades que conoce el Laboratorio: la propia de cada clase y las que se pueden entrenar.</summary>
public static class CatalogoHabilidades
{
    public static IReadOnlyList<Habilidad> Entrenables { get; } =
    [
        new("Tiro certero", BonusVida: 0, BonusAtaque: 2),
        new("Piel reforzada", BonusVida: 6, BonusAtaque: 0),
        new("Reflejos de combate", BonusVida: 3, BonusAtaque: 1),
        new("Resistencia", BonusVida: 8, BonusAtaque: 0),
        new("Golpe pesado", BonusVida: 0, BonusAtaque: 3),
    ];

    /// <summary>La habilidad con la que nace cada clase. Es la que usa en su turno y no se puede quitar.</summary>
    public static string DeClase(ClaseUnidad clase) => clase switch
    {
        ClaseUnidad.Soldado => "Carga",
        ClaseUnidad.Medico => "Atención de urgencia",
        ClaseUnidad.Francotirador => "Tiro apuntado",
        ClaseUnidad.Comandante => "Mando",
        _ => throw new ArgumentOutOfRangeException(nameof(clase)),
    };

    /// <summary>Devuelve null si no es una habilidad entrenable (por ejemplo, la de la clase).</summary>
    public static Habilidad? Buscar(string nombre) => Entrenables.FirstOrDefault(h => h.Nombre == nombre);
}
