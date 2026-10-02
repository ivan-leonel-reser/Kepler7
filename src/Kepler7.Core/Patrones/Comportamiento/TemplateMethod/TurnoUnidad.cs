using Kepler7.BE;
using Kepler7.Core.Combate;
using Kepler7.Core.Comun;
using Kepler7.Core.Patrones.Comportamiento.Command;

namespace Kepler7.Core.Patrones.Comportamiento.TemplateMethod;

/// <summary>
/// El turno de una unidad del escuadrón. Todas pasan por las mismas cuatro etapas;
/// cada clase cambia lo que pasa en cada una, pero no el orden.
/// </summary>
public abstract class TurnoUnidad
{
    protected TurnoUnidad(UnidadEscuadron unidad, CampoBatalla campo)
    {
        Unidad = unidad;
        Campo = campo;
    }

    protected UnidadEscuadron Unidad { get; }

    protected CampoBatalla Campo { get; }

    /// <summary>Lo que la clase le suma al ataque o a la curación de este turno. Se carga al decidir.</summary>
    protected int Bonus { get; set; }

    public static TurnoUnidad Para(UnidadEscuadron unidad, CampoBatalla campo) => unidad.Clase switch
    {
        ClaseUnidad.Soldado => new TurnoSoldado(unidad, campo),
        ClaseUnidad.Medico => new TurnoMedico(unidad, campo),
        ClaseUnidad.Francotirador => new TurnoFrancotirador(unidad, campo),
        ClaseUnidad.Comandante => new TurnoComandante(unidad, campo),
        _ => throw new ArgumentOutOfRangeException(nameof(unidad)),
    };

    // Método plantilla. No es virtual: ninguna subclase puede saltear ni reordenar las etapas.
    public IComando EjecutarTurno(OrdenTurno orden, GestorComandos gestor, Bitacora historial)
    {
        Registrar(historial, "inicio", Iniciar());
        Registrar(historial, "decisión", Decidir(orden));

        var comando = CrearComando(orden);
        gestor.Ejecutar(comando);
        Registrar(historial, "acción", comando.Descripcion, TipoEntrada.Exito);

        Registrar(historial, "fin", Finalizar());
        return comando;
    }

    /// <summary>Lo que distingue el turno de esta clase, para mostrárselo al jugador.</summary>
    public abstract string Habilidad { get; }

    protected abstract string Iniciar();

    /// <summary>Acá cada clase interpreta la orden y, si corresponde, carga su <see cref="Bonus"/>.</summary>
    protected abstract string Decidir(OrdenTurno orden);

    protected virtual string Finalizar() =>
        Unidad.Fila == Fila.Vanguardia ? "queda sosteniendo la línea de adelante" : "queda a cubierto atrás";

    /// <summary>Texto de una orden que la clase cumple sin nada especial.</summary>
    protected static string Describir(OrdenTurno orden) => orden.Tipo switch
    {
        TipoOrden.Mover => "cambia de fila",
        TipoOrden.Atacar => $"ataca a {orden.Objetivo?.Nombre}",
        TipoOrden.Curar => $"cura a {orden.Objetivo?.Nombre}",
        _ => "levanta una barricada",
    };

    private IComando CrearComando(OrdenTurno orden) => orden.Tipo switch
    {
        TipoOrden.Mover => new MoverComando(Campo, Unidad, Unidad.Fila == Fila.Vanguardia ? Fila.Retaguardia : Fila.Vanguardia),
        TipoOrden.Atacar => new AtacarComando(Unidad, ObjetivoDe(orden), Bonus),
        TipoOrden.Curar => new CurarComando(Unidad, ObjetivoDe(orden), Oleada.Curacion + Bonus),
        TipoOrden.Construir => new ConstruirComando(Campo, Unidad),
        _ => throw new ArgumentOutOfRangeException(nameof(orden)),
    };

    private static Unidad ObjetivoDe(OrdenTurno orden) =>
        orden.Objetivo ?? throw new ArgumentException("La orden necesita un objetivo.", nameof(orden));

    private void Registrar(Bitacora historial, string etapa, string detalle, TipoEntrada tipo = TipoEntrada.Info) =>
        historial.Registrar($"{Unidad.Nombre} · {etapa}", detalle, tipo);
}
