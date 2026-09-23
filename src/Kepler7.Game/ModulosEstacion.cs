using Kepler7.Core.Progreso;
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
        new("Hangar", Patron.Builder, "Ensamblá la nave de apoyo para la próxima misión.", null),
        new("Laboratorio", Patron.Prototype, "Cloná tripulantes para completar el escuadrón.", null),
        new("Comunicaciones", Patron.Bridge, "Enviá mensajes cifrados a la Tierra.", null),
        new("Control Central", Patron.Facade, "Operá los seis subsistemas con una sola orden.", p => new PantallaControlCentral(p)),
        new("Censo", Patron.Flyweight, "Registro de los 10.000 tripulantes de la estación.", null),
        new("Bóveda", Patron.Proxy, "Archivos reservados del Consejo de Ingeniería.", null),
        new("Almacén", Patron.Visitor, "Analizá el botín recuperado en las misiones.", null),
        new("Armería", Patron.ChainOfResponsibility, "Configurá las capas de defensa del escuadrón.", null),
    ];

    public static string NombrePatron(Patron patron) => patron switch
    {
        Patron.ChainOfResponsibility => "Chain of Responsibility",
        Patron.TemplateMethod => "Template Method",
        _ => patron.ToString(),
    };
}
