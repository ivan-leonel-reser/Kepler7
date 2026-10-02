namespace Kepler7.Core.Progreso;

/// <summary>Una insignia por patrón. La condición la chequea el módulo de cada patrón.</summary>
public static class CatalogoInsignias
{
    public static IReadOnlyList<Insignia> Todas { get; } =
    [
        new(Patron.Builder, FamiliaPatron.Creacional, "Ingeniero de hangar", "Ensamblá una nave con cada una de las tres recetas."),
        new(Patron.Prototype, FamiliaPatron.Creacional, "Genetista", "Cloná un tripulante y modificá el clon sin alterar al original."),
        new(Patron.Bridge, FamiliaPatron.Estructural, "Enlace con la Tierra", "Enviá mensajes usando los tres tipos de cifrado."),
        new(Patron.Facade, FamiliaPatron.Estructural, "Operador central", "Usá las tres operaciones del Control Central."),
        new(Patron.Flyweight, FamiliaPatron.Estructural, "Censista", "Reclutá a un tripulante del censo de 10.000."),
        new(Patron.Proxy, FamiliaPatron.Estructural, "Guardián de la bóveda", "Entrá a la bóveda con un rol autorizado."),
        new(Patron.ChainOfResponsibility, FamiliaPatron.Comportamiento, "Muralla", "Absorbé un golpe entero sin perder vida."),
        new(Patron.Command, FamiliaPatron.Comportamiento, "Dueño del tiempo", "Deshacé y rehacé una jugada durante una oleada."),
        new(Patron.Visitor, FamiliaPatron.Comportamiento, "Inventarista", "Aplicá los tres analizadores sobre el inventario."),
        new(Patron.State, FamiliaPatron.Comportamiento, "Superviviente", "Ganá una oleada con una unidad que cambió de estado."),
        new(Patron.TemplateMethod, FamiliaPatron.Comportamiento, "Cadena de mando", "Jugá un turno con cada una de las cuatro clases."),
        new(Patron.Strategy, FamiliaPatron.Comportamiento, "Hacker del Enjambre", "Hackeá la IA de un enemigo en plena oleada."),
    ];

    public static Insignia Obtener(Patron patron) => Todas.First(i => i.Patron == patron);
}
