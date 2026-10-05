using Kepler7.BE;
using Kepler7.Core.Combate;
using Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;
using Kepler7.Core.Patrones.Comportamiento.Visitor;
using Kepler7.Core.Patrones.Creacionales.Builder;
using Kepler7.Core.Patrones.Estructurales.Facade;
using Kepler7.Core.Patrones.Estructurales.Flyweight;
using Kepler7.Core.Progreso;
using Kepler7.Services;

namespace Kepler7.Core.Juego;

/// <summary>La partida en curso. Es lo único que la interfaz necesita tener a mano.</summary>
public class Partida
{
    public const int XpPorInsignia = 50;
    public const int CreditosPorInsignia = 100;
    public const int CostoRecluta = 50;

    private readonly IRepositorioProgreso _repositorio;
    private readonly IngenieroJefe _ingeniero = new();
    private readonly HashSet<string> _recetasUsadas = [];
    private readonly HashSet<string> _analizadoresUsados = [];
    private UnidadEscuadron? _unidadRecluta;

    public Partida(IRepositorioProgreso repositorio, ProgresoJugador progreso, IAleatorio aleatorio)
    {
        _repositorio = repositorio;
        Progreso = progreso;
        Aleatorio = aleatorio;
        ControlCentral = new ControlCentral(Bitacora);
    }

    public ProgresoJugador Progreso { get; }
    public IAleatorio Aleatorio { get; }
    public Bitacora Bitacora { get; } = new();
    public ControlCentral ControlCentral { get; }
    public List<UnidadEscuadron> Escuadron { get; } = EscuadronInicial();
    public Oleada? OleadaActual { get; private set; }

    /// <summary>Las capas de defensa que lleva el escuadrón y su orden.</summary>
    public Armeria Armeria { get; } = new();

    /// <summary>El arsenal de la estación y el botín que van dejando las oleadas.</summary>
    public Inventario Almacen { get; } = AlmacenInicial();

    /// <summary>La nave que espera en el Hangar para la próxima oleada. Hay lugar para una sola.</summary>
    public Nave? NaveDeApoyo { get; private set; }

    /// <summary>El padrón de la estación, de donde se recluta.</summary>
    public Censo Censo { get; } = new();

    /// <summary>El tripulante del censo que está sirviendo en el escuadrón. Hay lugar para uno solo.</summary>
    public Tripulante? Recluta { get; private set; }

    public bool PuedeReclutar => !Terminada && Progreso.Creditos >= CostoRecluta;

    /// <summary>La partida se perdió: ya no se guarda más.</summary>
    public bool Terminada { get; private set; }

    public int NumeroProximaOleada => Progreso.OleadasSuperadas + 1;

    public bool PuedeDefender =>
        !Terminada && !Progreso.EstacionSalvada && ControlCentral.Modo == ModoEstacion.Operativa;

    public static Partida Nueva(IRepositorioProgreso repositorio, IAleatorio aleatorio)
    {
        repositorio.Borrar();
        var partida = new Partida(repositorio, new ProgresoJugador(), aleatorio);
        partida.Bitacora.Registrar("Consejo", "El Enjambre va a atacar Kepler-7 en 6 oleadas. Prepará al escuadrón y defendé la estación.", TipoEntrada.Advertencia);
        partida.AvisarEstacionApagada();
        partida.Guardar();
        return partida;
    }

    public static Partida Continuar(IRepositorioProgreso repositorio, IAleatorio aleatorio)
    {
        var progreso = repositorio.Cargar()
            ?? throw new InvalidOperationException("No hay una partida guardada.");

        var partida = new Partida(repositorio, progreso, aleatorio);
        partida.Bitacora.Registrar("Consejo", $"Bienvenido de nuevo, Comandante. Oleadas superadas: {progreso.OleadasSuperadas} de {ProgresoJugador.OleadasTotales}.");
        partida.AvisarEstacionApagada();
        return partida;
    }

    public void Guardar()
    {
        if (!Terminada)
            _repositorio.Guardar(Progreso);
    }

    /// <summary>El ingeniero jefe arma la nave con la receta elegida y la deja lista para la próxima oleada.</summary>
    public Nave EnsamblarNave(INaveBuilder receta)
    {
        var nave = _ingeniero.Ensamblar(receta);
        NaveDeApoyo = nave;
        _recetasUsadas.Add(receta.Nombre);
        Bitacora.Registrar("Hangar", $"Nave {nave.Modelo} lista para la próxima oleada: {nave.Apoyo}.", TipoEntrada.Exito);
        return nave;
    }

    /// <summary>Arma el padrón la primera vez que se lo consulta. Son 10.000 tripulantes: la interfaz lo pide fuera del hilo de la ventana.</summary>
    public void GenerarCenso() => Censo.Generar(Aleatorio);

    /// <summary>
    /// Suma al escuadrón a un tripulante del censo, como soldado y con el bonus de su especie.
    /// Si ya había un recluta, el nuevo lo reemplaza.
    /// </summary>
    public UnidadEscuadron Reclutar(Tripulante tripulante)
    {
        if (!PuedeReclutar)
            throw new InvalidOperationException($"Reclutar cuesta {CostoRecluta} créditos.");

        Progreso.GastarCreditos(CostoRecluta);
        if (_unidadRecluta is not null)
            Escuadron.Remove(_unidadRecluta);

        var especie = tripulante.Especie;
        _unidadRecluta = UnidadEscuadron.Crear(tripulante.Nombre, ClaseUnidad.Soldado, especie.BonusVida, especie.BonusAtaque);
        Escuadron.Add(_unidadRecluta);
        Recluta = tripulante;

        Bitacora.Registrar("Censo",
            $"{tripulante.Nombre} ({especie.Nombre}) se suma al escuadrón como soldado: +{especie.BonusVida} de vida y +{especie.BonusAtaque} de ataque por su especie.",
            TipoEntrada.Exito);
        Guardar();
        return _unidadRecluta;
    }

    /// <summary>Pasea por todo el Almacén al analizador que eligió el jugador.</summary>
    public void AnalizarAlmacen(IVisitor visitante)
    {
        Almacen.Aplicar(visitante);
        _analizadoresUsados.Add(visitante.Nombre);
        Bitacora.Registrar("Almacén", $"{visitante.Nombre}: {visitante.Resultado}");
    }

    /// <summary>Vende todo lo que hay en el Almacén al valor que da el calculador de precio.</summary>
    /// <returns>Los créditos que entraron.</returns>
    public int VenderAlmacen()
    {
        if (Almacen.Items.Count == 0)
            return 0;

        var calculador = new CalculadorPrecio();
        Almacen.Aplicar(calculador);
        Progreso.GanarCreditos(calculador.Total);
        Bitacora.Registrar("Almacén", $"Se vendieron {Almacen.Items.Count} objetos por {calculador.Total} créditos.", TipoEntrada.Exito);
        Almacen.Vaciar();
        Guardar();
        return calculador.Total;
    }

    public Oleada EmpezarOleada()
    {
        if (!PuedeDefender)
            throw new InvalidOperationException("Para defender una oleada la estación tiene que estar operativa.");

        foreach (var unidad in Escuadron)
        {
            unidad.Curar(unidad.VidaMaxima);
            Armeria.Equipar(unidad, Aleatorio);
        }

        var numero = NumeroProximaOleada;
        var oleada = new Oleada(numero, Escuadron, GeneradorOleadas.Crear(numero), Aleatorio);

        // La alarma la da el Control Central: una sola orden pone toda la estación en emergencia.
        foreach (var paso in ControlCentral.EntrarEnEmergencia())
            oleada.Bitacora.Registrar(paso);
        if (NaveDeApoyo is { } nave)
        {
            oleada.RecibirApoyo(nave);
            // La nave sale con esta oleada: para la siguiente hay que armar otra.
            NaveDeApoyo = null;
        }
        var enemigos = oleada.Campo.UnidadesVivasDe(Bando.Enjambre).Count();
        oleada.Bitacora.Registrar("Oleada", $"Oleada {numero} de {ProgresoJugador.OleadasTotales}: {enemigos} enemigos. Le toca al escuadrón.", TipoEntrada.Peligro);

        OleadaActual = oleada;
        return oleada;
    }

    public ResultadoOleada CerrarOleada()
    {
        var oleada = OleadaActual ?? throw new InvalidOperationException("No hay una oleada en curso.");
        if (!oleada.Terminada)
            throw new InvalidOperationException("La oleada todavía no terminó.");

        if (oleada.Estado == EstadoOleada.Derrota)
        {
            var apagado = ControlCentral.ApagarEstacion();
            Bitacora.Registrar("Oleada", $"El escuadrón cayó en la oleada {oleada.Numero}. Kepler-7 se apaga.", TipoEntrada.Peligro);
            Terminada = true;
            _repositorio.Borrar();
            return new ResultadoOleada(false, 0, 0, false, apagado);
        }

        var xp = 40 + 10 * oleada.Numero;
        var creditos = 50 + 25 * oleada.Numero;
        creditos += creditos * (oleada.NaveDeApoyo?.BonusCreditos ?? 0) / 100;
        Progreso.SuperarOleada();
        Progreso.GanarCreditos(creditos);
        var subioNivel = Progreso.GanarXp(xp);

        var botin = GeneradorBotin.Crear(oleada.Numero);
        Almacen.AgregarTodos(botin);

        // Pasado el ataque, la fachada vuelve a poner la estación en su turno normal.
        var normalidad = ControlCentral.IniciarTurno();
        Bitacora.Registrar("Oleada", $"Oleada {oleada.Numero} superada: +{xp} XP, +{creditos} créditos.", TipoEntrada.Exito);
        Bitacora.Registrar("Almacén", $"Botín de la oleada: {string.Join(", ", botin.Select(i => i.Nombre))}.", TipoEntrada.Exito);
        if (Progreso.EstacionSalvada)
            Bitacora.Registrar("Consejo", "Kepler-7 resistió las 6 oleadas. La estación está a salvo.", TipoEntrada.Exito);

        Guardar();
        return new ResultadoOleada(true, xp, creditos, subioNivel, normalidad);
    }

    /// <summary>Se llama después de cada acción del jugador. Devuelve las insignias que acaba de ganar.</summary>
    public IReadOnlyList<Insignia> RevisarLogros()
    {
        var nuevas = new List<Insignia>();

        if (ControlCentral.TurnosIniciados > 0 && ControlCentral.EmergenciasDeclaradas > 0 && ControlCentral.VecesApagada > 0)
            Sumar(nuevas, OtorgarInsignia(Patron.Facade));

        if (_recetasUsadas.Count == CatalogoRecetas.Crear().Count)
            Sumar(nuevas, OtorgarInsignia(Patron.Builder));

        if (Recluta is not null)
            Sumar(nuevas, OtorgarInsignia(Patron.Flyweight));

        if (OleadaActual is { } oleada && oleada.Comandos.VecesDeshecho > 0 && oleada.Comandos.VecesRehecho > 0)
            Sumar(nuevas, OtorgarInsignia(Patron.Command));

        if (OleadaActual is { } actual && actual.ClasesQueJugaron.Count == Enum.GetValues<ClaseUnidad>().Length)
            Sumar(nuevas, OtorgarInsignia(Patron.TemplateMethod));

        if (_analizadoresUsados.Count == CatalogoVisitantes.Crear().Count)
            Sumar(nuevas, OtorgarInsignia(Patron.Visitor));

        if (OleadaActual is { GolpesAbsorbidos: > 0 })
            Sumar(nuevas, OtorgarInsignia(Patron.ChainOfResponsibility));

        if (OleadaActual is { HackeosRealizados: > 0 })
            Sumar(nuevas, OtorgarInsignia(Patron.Strategy));

        return nuevas;
    }

    /// <summary>Da la insignia con su recompensa y guarda. Devuelve null si ya la tenía.</summary>
    public Insignia? OtorgarInsignia(Patron patron)
    {
        if (!Progreso.DesbloquearInsignia(patron))
            return null;

        var insignia = CatalogoInsignias.Obtener(patron);
        Progreso.GanarCreditos(CreditosPorInsignia);
        var subioNivel = Progreso.GanarXp(XpPorInsignia);

        Bitacora.Registrar("Logros", $"Insignia desbloqueada: {insignia.Nombre} ({patron}).", TipoEntrada.Exito);
        if (subioNivel)
            Bitacora.Registrar("Logros", $"Subiste a nivel {Progreso.Nivel}.", TipoEntrada.Exito);

        Guardar();
        return insignia;
    }

    private static List<UnidadEscuadron> EscuadronInicial() =>
    [
        UnidadEscuadron.Crear("Vega", ClaseUnidad.Comandante),
        UnidadEscuadron.Crear("Rojas", ClaseUnidad.Soldado),
        UnidadEscuadron.Crear("Paz", ClaseUnidad.Medico),
        UnidadEscuadron.Crear("Luna", ClaseUnidad.Francotirador),
    ];

    private static Inventario AlmacenInicial()
    {
        var almacen = new Inventario();
        almacen.AgregarTodos(GeneradorBotin.ArsenalInicial());
        return almacen;
    }

    private void AvisarEstacionApagada() =>
        Bitacora.Registrar("Control central", "La estación está apagada. Iniciá un turno desde el Control Central.", TipoEntrada.Advertencia);

    private static void Sumar(List<Insignia> nuevas, Insignia? insignia)
    {
        if (insignia is not null)
            nuevas.Add(insignia);
    }
}
