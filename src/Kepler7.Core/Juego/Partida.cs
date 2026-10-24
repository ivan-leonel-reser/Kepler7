using Kepler7.BE;
using Kepler7.Core.Combate;
using Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;
using Kepler7.Core.Patrones.Comportamiento.State;
using Kepler7.Core.Patrones.Comportamiento.Visitor;
using Kepler7.Core.Patrones.Creacionales.Builder;
using Kepler7.Core.Patrones.Creacionales.Prototype;
using Kepler7.Core.Patrones.Estructurales.Bridge;
using Kepler7.Core.Patrones.Estructurales.Facade;
using Kepler7.Core.Patrones.Estructurales.Flyweight;
using Kepler7.Core.Patrones.Estructurales.Proxy;
using Kepler7.Core.Persistencia;
using Kepler7.Core.Progreso;
using Kepler7.DAL;
using Kepler7.Services;

namespace Kepler7.Core.Juego;

/// <summary>La partida en curso. Es lo único que la interfaz necesita tener a mano.</summary>
public class Partida
{
    public const int XpPorInsignia = 50;
    public const int CreditosPorInsignia = 100;
    public const int CostoRecluta = 50;
    public const int CostoClon = 80;

    private readonly IRepositorioProgreso _repositorio;
    private readonly IngenieroJefe _ingeniero = new();
    private readonly HashSet<string> _recetasUsadas = [];
    private readonly HashSet<string> _analizadoresUsados = [];
    private readonly Dictionary<UnidadEscuadron, FichaSoldado> _fichas = [];
    private readonly HashSet<int> _informesLeidos = [];
    private readonly HashSet<string> _cifradosUsados = [];
    private readonly List<Transmision> _transmisiones = [];
    private int _barricadasDeRefuerzo;
    private bool _mensajeInterceptado;
    private UnidadEscuadron? _unidadRecluta;
    private UnidadEscuadron? _unidadClon;
    private bool _clonModificado;

    public Partida(IRepositorioProgreso repositorio, ProgresoJugador progreso, IAleatorio aleatorio, IRegistroAccesos? registroAccesos = null)
    {
        _repositorio = repositorio;
        Progreso = progreso;
        Aleatorio = aleatorio;
        ControlCentral = new ControlCentral(Bitacora);
        Boveda = new ProxyBoveda(CatalogoBoveda.CrearBoveda, registroAccesos ?? Repositorios.CrearRegistroAccesos());
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

    /// <summary>La ficha del clon que salió del Laboratorio y está en el escuadrón. Hay lugar para uno solo.</summary>
    public FichaSoldado? Clon { get; private set; }

    public bool PuedeClonar => !Terminada && Progreso.Creditos >= CostoClon;

    /// <summary>La bóveda de datos, siempre detrás de su proxy. La interfaz nunca toca la bóveda real.</summary>
    public ProxyBoveda Boveda { get; }

    /// <summary>Si ya se leyó el informe de la próxima oleada: al empezarla, la IA de cada enemigo se ve desde el primer turno.</summary>
    public bool InformeLeido => _informesLeidos.Contains(NumeroProximaOleada);

    /// <summary>Los mensajes que ya salieron hacia la Tierra, del primero al último.</summary>
    public IReadOnlyList<Transmision> Transmisiones => _transmisiones;

    public Transmision? UltimaTransmision => _transmisiones.Count > 0 ? _transmisiones[^1] : null;

    /// <summary>La antena manda un solo mensaje por oleada: se libera cuando empieza la siguiente.</summary>
    public bool AntenaDisponible { get; private set; } = true;

    /// <summary>Barricadas que la Tierra ya mandó para la próxima oleada.</summary>
    public int BarricadasDeRefuerzo => _barricadasDeRefuerzo;

    public bool PuedeTransmitir(Mensaje mensaje) =>
        !Terminada && AntenaDisponible && Progreso.Creditos >= mensaje.Cifrado.Costo;

    /// <summary>A quiénes se puede clonar: todo el escuadrón menos el propio clon.</summary>
    public IReadOnlyList<UnidadEscuadron> Clonables => Escuadron.Where(u => u != _unidadClon).ToList();

    /// <summary>La partida se perdió: ya no se guarda más.</summary>
    public bool Terminada { get; private set; }

    public int NumeroProximaOleada => Progreso.OleadasSuperadas + 1;

    public bool PuedeDefender =>
        !Terminada && !Progreso.EstacionSalvada && ControlCentral.Modo == ModoEstacion.Operativa;

    public static Partida Nueva(IRepositorioProgreso repositorio, IAleatorio aleatorio, IRegistroAccesos? registroAccesos = null)
    {
        repositorio.Borrar();
        var partida = new Partida(repositorio, new ProgresoJugador(), aleatorio, registroAccesos);
        partida.Bitacora.Registrar("Consejo", "El Enjambre va a atacar Kepler-7 en 6 oleadas. Prepará al escuadrón y defendé la estación.", TipoEntrada.Advertencia);
        partida.AvisarEstacionApagada();
        partida.Guardar();
        return partida;
    }

    public static Partida Continuar(IRepositorioProgreso repositorio, IAleatorio aleatorio, IRegistroAccesos? registroAccesos = null)
    {
        var progreso = repositorio.Cargar()
            ?? throw new InvalidOperationException("No hay una partida guardada.");

        var partida = new Partida(repositorio, progreso, aleatorio, registroAccesos);
        partida.Bitacora.Registrar("Consejo", $"Bienvenido de nuevo, Comandante. Oleadas superadas: {progreso.OleadasSuperadas} de {ProgresoJugador.OleadasTotales}.");
        partida.AvisarEstacionApagada();
        return partida;
    }

    // Si la base se cayó en medio de la partida, no se pierde el juego: queda el aviso y se sigue en memoria.
    public void Guardar()
    {
        if (Terminada)
            return;

        try
        {
            _repositorio.Guardar(Progreso);
        }
        catch (ErrorDePersistencia e)
        {
            Bitacora.Registrar("Guardado", $"No se pudo guardar la partida: {e.Message}", TipoEntrada.Peligro);
        }
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

    /// <summary>La ficha de una unidad del escuadrón, que es el original del que se sacan los clones.</summary>
    public FichaSoldado FichaDe(UnidadEscuadron unidad)
    {
        if (!_fichas.TryGetValue(unidad, out var ficha))
        {
            var especie = unidad == _unidadRecluta ? Recluta?.Especie : null;
            ficha = new FichaSoldado(unidad.Nombre, unidad.Clase, especie);
            _fichas[unidad] = ficha;
        }
        return ficha;
    }

    /// <summary>
    /// Suma al escuadrón la unidad que sale de la ficha del clon. Si ya había un clon, el nuevo lo reemplaza.
    /// </summary>
    public UnidadEscuadron SumarClon(FichaSoldado original, FichaSoldado clon)
    {
        if (ReferenceEquals(original, clon))
            throw new ArgumentException("El clon tiene que ser una copia, no el mismo original.", nameof(clon));
        if (!PuedeClonar)
            throw new InvalidOperationException($"Sumar un clon cuesta {CostoClon} créditos.");

        Progreso.GastarCreditos(CostoClon);
        if (_unidadClon is not null)
            Escuadron.Remove(_unidadClon);

        _unidadClon = clon.CrearUnidad();
        _fichas[_unidadClon] = clon;
        Escuadron.Add(_unidadClon);
        Clon = clon;
        _clonModificado |= !clon.Habilidades.SequenceEqual(original.Habilidades);

        Bitacora.Registrar("Laboratorio",
            $"{clon.Nombre}, clon de {original.Nombre}, se suma al escuadrón con {clon.Habilidades.Count} habilidades.",
            TipoEntrada.Exito);
        Guardar();
        return _unidadClon;
    }

    /// <summary>
    /// Lee un archivo de la bóveda con la credencial que tenga puesta el proxy. Si el proxy lo rechaza,
    /// el intento ya quedó registrado y la excepción llega a la interfaz para que lo muestre.
    /// Leer el informe de la próxima oleada hace que sus enemigos salgan con la IA a la vista.
    /// </summary>
    public string LeerArchivoBoveda(string id)
    {
        var contenido = Operar(() => Boveda.LeerArchivo(id), ProxyBoveda.OperacionLeer, id);
        if (CatalogoBoveda.OleadaDelInforme(id) is { } oleada && _informesLeidos.Add(oleada) && oleada == NumeroProximaOleada)
            Bitacora.Registrar("Bóveda", $"El informe de la oleada {oleada} revela la IA de cada enemigo: va a verse desde el primer turno.", TipoEntrada.Exito);
        return contenido;
    }

    public void EscribirArchivoBoveda(string id, string contenido) =>
        Operar(() => { Boveda.EscribirArchivo(id, contenido); return contenido; }, ProxyBoveda.OperacionEscribir, id);

    private string Operar(Func<string> operacion, string nombre, string id)
    {
        var quien = Boveda.Credencial?.ToString() ?? "alguien sin credencial";
        try
        {
            var resultado = operacion();
            Bitacora.Registrar("Bóveda", $"{quien} pudo {nombre.ToLowerInvariant()} el archivo '{id}'.", TipoEntrada.Exito);
            return resultado;
        }
        catch (AccesoDenegadoException)
        {
            Bitacora.Registrar("Bóveda", $"Acceso denegado: {quien} intentó {nombre.ToLowerInvariant()} '{id}'. Quedó en el registro.", TipoEntrada.Peligro);
            throw;
        }
    }

    /// <summary>
    /// Manda el mensaje a la Tierra con el cifrado que tenga puesto. Cobra el cifrado, tira el dado de la
    /// intercepción y aplica lo que la Tierra responde: los créditos ahora y las barricadas en la próxima oleada.
    /// </summary>
    public Transmision Transmitir(Mensaje mensaje)
    {
        if (!AntenaDisponible)
            throw new InvalidOperationException("La antena ya transmitió: hay un mensaje por oleada.");
        if (!PuedeTransmitir(mensaje))
            throw new InvalidOperationException($"Transmitir con {mensaje.Cifrado.Nombre} cuesta {mensaje.Cifrado.Costo} créditos.");

        var enviada = mensaje.Enviar();
        var interceptada = enviada.RiesgoIntercepcion > 0 && Aleatorio.Probabilidad(enviada.RiesgoIntercepcion);
        var transmision = enviada with { Interceptada = interceptada };

        Progreso.GastarCreditos(transmision.Costo);
        AntenaDisponible = false;
        _cifradosUsados.Add(transmision.Cifrado);
        _transmisiones.Add(transmision);
        Bitacora.Registrar("Comunicaciones", $"Mensaje de {transmision.Tipo.ToLowerInvariant()} enviado a la Tierra ({transmision.Cifrado}, {transmision.Costo} créditos).");

        if (interceptada)
        {
            _mensajeInterceptado = true;
            Bitacora.Registrar("Comunicaciones", "El Enjambre interceptó el mensaje: la próxima oleada viene con un zángano más.", TipoEntrada.Peligro);
        }

        var refuerzo = transmision.Refuerzo;
        if (refuerzo.Creditos > 0)
            Progreso.GanarCreditos(refuerzo.Creditos);
        _barricadasDeRefuerzo += refuerzo.Barricadas;
        Bitacora.Registrar("Comunicaciones", $"La Tierra responde. {refuerzo.Descripcion}.", TipoEntrada.Exito);

        Guardar();
        return transmision;
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

        // Si en el Almacén quedaron armas envenenadas, el escuadrón sale a pelear con ellas.
        var armasEnvenenadas = Almacen.Items.OfType<Arma>().Any(a => a.Envenenada);
        foreach (var unidad in Escuadron)
        {
            unidad.Curar(unidad.VidaMaxima);
            unidad.CambiarEstado(new Normal());
            unidad.EfectoAlGolpear = armasEnvenenadas ? EfectoGolpe.Veneno : EfectoGolpe.Ninguno;
            Armeria.Equipar(unidad, Aleatorio);
        }

        var numero = NumeroProximaOleada;
        var enjambre = GeneradorOleadas.Crear(numero).ToList();
        if (_mensajeInterceptado)
            enjambre.Add(GeneradorOleadas.CrearInterceptor(numero));
        var oleada = new Oleada(numero, Escuadron, enjambre, Aleatorio);

        // La alarma la da el Control Central: una sola orden pone toda la estación en emergencia.
        foreach (var paso in ControlCentral.EntrarEnEmergencia())
            oleada.Bitacora.Registrar(paso);
        if (NaveDeApoyo is { } nave)
        {
            oleada.RecibirApoyo(nave);
            // La nave sale con esta oleada: para la siguiente hay que armar otra.
            NaveDeApoyo = null;
        }
        if (armasEnvenenadas)
            oleada.Bitacora.Registrar("Almacén", "El escuadrón lleva armas envenenadas: cada golpe que saque vida envenena al enemigo.", TipoEntrada.Exito);
        if (InformeLeido)
        {
            foreach (var enemigo in oleada.Campo.UnidadesVivasDe(Bando.Enjambre).OfType<Enemigo>())
                enemigo.IARevelada = true;
            oleada.Bitacora.Registrar("Bóveda", "Gracias al informe de la bóveda, la IA de cada enemigo está a la vista.", TipoEntrada.Exito);
        }
        if (_mensajeInterceptado)
            oleada.Bitacora.Registrar("Comunicaciones", "El Enjambre leyó el mensaje a la Tierra y manda un zángano más.", TipoEntrada.Peligro);
        if (_barricadasDeRefuerzo > 0)
            oleada.RecibirRefuerzo(_barricadasDeRefuerzo);
        oleada.AmpliarLimites(Desbloqueos.HackeosExtra(Progreso.Nivel), Desbloqueos.BarricadasExtra(Progreso.Nivel), $"ser nivel {Progreso.Nivel}");
        // Lo que se pidió por la antena ya se usó en esta oleada; para la siguiente se vuelve a transmitir.
        _mensajeInterceptado = false;
        _barricadasDeRefuerzo = 0;
        AntenaDisponible = true;
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
        var creditos = 50 + 25 * oleada.Numero + Desbloqueos.CreditosExtraPorOleada(Progreso.Nivel);
        creditos += creditos * (oleada.NaveDeApoyo?.BonusCreditos ?? 0) / 100;
        Progreso.SuperarOleada();
        Progreso.GanarCreditos(creditos);
        var subioNivel = Progreso.GanarXp(xp);
        if (subioNivel)
            AvisarNivel();

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
        foreach (var insignia in CatalogoInsignias.Todas)
        {
            if (AvanceDe(insignia.Patron).Completo)
                Sumar(nuevas, OtorgarInsignia(insignia.Patron));
        }
        return nuevas;
    }

    /// <summary>
    /// Qué tan cerca está el jugador de cada insignia. Lo que se hace dentro de una oleada
    /// (deshacer, hackear, absorber un golpe...) cuenta solo para la última oleada jugada.
    /// </summary>
    public AvanceInsignia AvanceDe(Patron patron)
    {
        var oleada = OleadaActual;
        var avance = patron switch
        {
            Patron.Builder => new AvanceInsignia(_recetasUsadas.Count, CatalogoRecetas.Crear().Count, "recetas usadas"),
            Patron.Prototype => Uno(_clonModificado, "clon con habilidades cambiadas"),
            Patron.Bridge => new AvanceInsignia(_cifradosUsados.Count, CatalogoComunicaciones.Cifrados.Count, "cifrados usados"),
            Patron.Facade => new AvanceInsignia(
                new[] { ControlCentral.TurnosIniciados, ControlCentral.EmergenciasDeclaradas, ControlCentral.VecesApagada }.Count(n => n > 0),
                3, "operaciones del Control Central"),
            Patron.Flyweight => Uno(Recluta is not null, "recluta del censo"),
            Patron.Proxy => Uno(Boveda.AccesosConcedidos > 0, "acceso autorizado a la bóveda"),
            Patron.ChainOfResponsibility => Uno(oleada is { GolpesAbsorbidos: > 0 }, "golpe absorbido entero"),
            Patron.Command => new AvanceInsignia(
                (oleada is { Comandos.VecesDeshecho: > 0 } ? 1 : 0) + (oleada is { Comandos.VecesRehecho: > 0 } ? 1 : 0),
                2, "deshacer y rehacer en la oleada"),
            Patron.Visitor => new AvanceInsignia(_analizadoresUsados.Count, CatalogoVisitantes.Crear().Count, "analizadores usados"),
            Patron.State => Uno(oleada is { Estado: EstadoOleada.Victoria } && oleada.SobrevivientesConEstado.Any(), "oleada ganada con un estado"),
            Patron.TemplateMethod => new AvanceInsignia(oleada?.ClasesQueJugaron.Count ?? 0, Enum.GetValues<ClaseUnidad>().Length, "clases que jugaron en la oleada"),
            Patron.Strategy => Uno(oleada is { HackeosRealizados: > 0 }, "hackeo en la oleada"),
            _ => throw new ArgumentOutOfRangeException(nameof(patron)),
        };
        // Una insignia ganada no se pierde aunque la oleada siguiente arranque de cero.
        return Progreso.TieneInsignia(patron) ? avance with { Actual = avance.Requerido } : avance;
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
            AvisarNivel();

        Guardar();
        return insignia;
    }

    /// <summary>Todo lo que se hizo en la partida, para la pantalla final.</summary>
    public ResumenPartida Resumen() => new(
        Progreso.EstacionSalvada,
        Progreso.OleadasSuperadas,
        Progreso.Nivel,
        Progreso.Xp,
        Progreso.Creditos,
        Progreso.Insignias.Select(CatalogoInsignias.Obtener).ToList(),
        _transmisiones.Count,
        Boveda.AccesosConcedidos,
        Boveda.IntentosDenegados.Count,
        Recluta?.Nombre,
        Clon?.Nombre);

    // Al subir de nivel se anuncia qué se desbloqueó, si hubo algo nuevo.
    private void AvisarNivel()
    {
        var nuevos = Desbloqueos.Todos.Where(d => d.Nivel == Progreso.Nivel).ToList();
        var detalle = nuevos.Count > 0 ? $" Desbloqueaste: {string.Join(", ", nuevos.Select(d => $"{d.Nombre} ({d.Descripcion.TrimEnd('.')})"))}." : "";
        Bitacora.Registrar("Logros", $"Subiste a nivel {Progreso.Nivel}.{detalle}", TipoEntrada.Exito);
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

    private static AvanceInsignia Uno(bool hecho, string detalle) => new(hecho ? 1 : 0, 1, detalle);

    private static void Sumar(List<Insignia> nuevas, Insignia? insignia)
    {
        if (insignia is not null)
            nuevas.Add(insignia);
    }
}
