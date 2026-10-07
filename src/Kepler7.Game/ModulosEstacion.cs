using Kepler7.BE;
using Kepler7.Game.Pantallas;

namespace Kepler7.Game;

internal record ModuloEstacion(string Nombre, Patron Patron, string Descripcion, Func<FormPrincipal, Pantalla>? Abrir)
{
    public bool Disponible => Abrir is not null;
}

/// <summary>
/// Los módulos que aparecen en el puente de mando. Cada dueño habilita el suyo
/// cargando <c>Abrir</c> en el mismo PR en que lo termina.
/// </summary>
internal static class ModulosEstacion
{
    public static IReadOnlyList<ModuloEstacion> Todos { get; } =
    [
        new("Hangar", Patron.Builder, "Armá la nave de apoyo para la próxima oleada.", p => new PantallaHangar(p)),
        new("Censo", Patron.Flyweight, "Reclutá tripulantes entre los 10.000 de la estación.", p => new PantallaCenso(p)),
        new("Laboratorio", Patron.Prototype, "Cloná a un soldado para sumarlo al escuadrón.", p => new PantallaLaboratorio(p)),
        new("Comunicaciones", Patron.Bridge, "Pedí refuerzos a la Tierra con el cifrado que elijas.", null),
        new("Bóveda", Patron.Proxy, "Leé el informe secreto sobre la próxima oleada.", p => new PantallaBoveda(p)),
        new("Almacén", Patron.Visitor, "Revisá y vendé el botín de las oleadas.", p => new PantallaAlmacen(p)),
        new("Armería", Patron.ChainOfResponsibility, "Ordená las capas de defensa del escuadrón.", p => new PantallaArmeria(p)),
        new("Control Central", Patron.Facade, "Operá los seis subsistemas con una sola orden.", p => new PantallaControlCentral(p)),
    ];

    public static string NombrePatron(Patron patron) => patron switch
    {
        Patron.ChainOfResponsibility => "Chain of Responsibility",
        Patron.TemplateMethod => "Template Method",
        _ => patron.ToString(),
    };
}
