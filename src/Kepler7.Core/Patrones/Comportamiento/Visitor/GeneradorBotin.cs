using Kepler7.BE;

namespace Kepler7.Core.Patrones.Comportamiento.Visitor;

/// <summary>Lo que hay en el Almacén al empezar y lo que deja cada oleada al ganarla.</summary>
public static class GeneradorBotin
{
    public static IReadOnlyList<IItem> ArsenalInicial() =>
    [
        new Arma("Rifle de pulso", danio: 4, precio: 25),
        new Arma("Cuchillo táctico", danio: 2, precio: 10),
        new Pocion("Botiquín de campaña", curacion: 10, precio: 15),
        new Armadura("Chaleco reforzado", defensa: 2, precio: 30),
    ];

    public static IReadOnlyList<IItem> Crear(int numeroOleada)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(numeroOleada, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(numeroOleada, ProgresoJugador.OleadasTotales);

        return numeroOleada switch
        {
            1 => [new Arma("Aguijón de Zángano", 3, 20), new Pocion("Suero regenerador", 10, 15)],
            2 => [new Arma("Lanza de quitina", 4, 30), new Armadura("Placa de caparazón", 2, 35)],
            3 => [new Arma("Glándula ácida", 5, 40), new Pocion("Suero concentrado", 15, 25)],
            4 => [new Arma("Garra de Coloso", 6, 55), new Armadura("Coraza de Coloso", 4, 60)],
            5 => [new Arma("Espina perforante", 7, 70), new Pocion("Néctar del Enjambre", 20, 40), new Armadura("Manto de quitina", 3, 50)],
            _ => [new Arma("Aguijón de la Reina", 10, 150)],
        };
    }
}
