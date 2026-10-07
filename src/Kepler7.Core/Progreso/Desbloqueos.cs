namespace Kepler7.Core.Progreso;

/// <summary>Subir de nivel sirve para algo: cada nivel suma una ventaja fija para el resto de la partida.</summary>
public static class Desbloqueos
{
    public static IReadOnlyList<Desbloqueo> Todos { get; } =
    [
        new(2, "Segundo hackeo", "Un hackeo más por oleada.", HackeosExtra: 1),
        new(3, "Material extra", "Una barricada más por oleada.", BarricadasExtra: 1),
        new(4, "Logística", "25 créditos más por cada oleada superada.", CreditosExtraPorOleada: 25),
        new(5, "Fortificación", "Otra barricada más por oleada.", BarricadasExtra: 1),
        new(6, "Tercer hackeo", "Otro hackeo más por oleada.", HackeosExtra: 1),
    ];

    public static IReadOnlyList<Desbloqueo> Alcanzados(int nivel) => Todos.Where(d => d.Nivel <= nivel).ToList();

    public static Desbloqueo? Proximo(int nivel) => Todos.FirstOrDefault(d => d.Nivel > nivel);

    public static int HackeosExtra(int nivel) => Alcanzados(nivel).Sum(d => d.HackeosExtra);

    public static int BarricadasExtra(int nivel) => Alcanzados(nivel).Sum(d => d.BarricadasExtra);

    public static int CreditosExtraPorOleada(int nivel) => Alcanzados(nivel).Sum(d => d.CreditosExtraPorOleada);
}
