namespace Kepler7.Core.Juego;

/// <summary>Datos fijos del juego que muestra la pantalla inicial (consigna 5.4.1).</summary>
public static class InfoJuego
{
    public const string Titulo = "Kepler-7: Protocolo Fénix";

    public const string Equipo = "Grupo 9";

    public static IReadOnlyList<string> Integrantes { get; } =
    [
        "Maite Stefania Benitez",
        "María Belén Fiuza",
        "Ivan Leonel Reser",
        "Carlos Joel Sánchez Quiroz",
        "Agustin Villaverde",
    ];
}
