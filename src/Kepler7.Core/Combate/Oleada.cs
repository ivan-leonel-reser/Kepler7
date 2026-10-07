using Kepler7.BE;
using Kepler7.Core.Patrones.Comportamiento.Command;
using Kepler7.Core.Patrones.Comportamiento.State;
using Kepler7.Core.Patrones.Comportamiento.Strategy;
using Kepler7.Core.Patrones.Comportamiento.TemplateMethod;
using Kepler7.Core.Patrones.Creacionales.Builder;
using Kepler7.Services;

namespace Kepler7.Core.Combate;

/// <summary>
/// El combate de una oleada. En el turno del escuadrón cada unidad hace una acción,
/// que pasa por las etapas de su turno, se guarda como comando y se puede deshacer hasta terminar el turno.
/// Después juega el Enjambre, de a un enemigo.
/// </summary>
public class Oleada
{
    public const int BarricadasPorOleada = 2;
    public const int Curacion = 10;
    public const int HackeosPorOleada = 2;
    private const string Origen = "Oleada";

    private readonly HashSet<Unidad> _yaActuaron = [];
    private readonly HashSet<ClaseUnidad> _clasesQueJugaron = [];
    private readonly Queue<Enemigo> _enemigosPorJugar = new();
    private readonly List<JugadaEnemiga> _ultimasJugadas = [];
    private readonly HashSet<Unidad> _cambiaronDeEstado = [];
    private int _barricadasExtra;

    public Oleada(int numero, IEnumerable<UnidadEscuadron> escuadron, IEnumerable<Enemigo> enemigos, IAleatorio aleatorio)
    {
        Numero = numero;
        Aleatorio = aleatorio;
        foreach (var unidad in escuadron)
            Ubicar(unidad);
        foreach (var enemigo in enemigos)
            Ubicar(enemigo);
    }

    public int Numero { get; }
    public int Turno { get; private set; } = 1;
    public EstadoOleada Estado { get; private set; } = EstadoOleada.TurnoJugador;
    public CampoBatalla Campo { get; } = new();
    public GestorComandos Comandos { get; } = new();
    public Bitacora Bitacora { get; } = new();
    public IAleatorio Aleatorio { get; }

    public bool Terminada => Estado is EstadoOleada.Victoria or EstadoOleada.Derrota;

    /// <summary>Qué decidió cada enemigo en el último turno del Enjambre, y por qué.</summary>
    public IReadOnlyList<JugadaEnemiga> UltimasJugadas => _ultimasJugadas;

    public IEnumerable<UnidadEscuadron> Escuadron =>
        Campo.UnidadesVivasDe(Bando.Escuadron).OfType<UnidadEscuadron>();

    public int BarricadasDisponibles => BarricadasPorOleada + _barricadasExtra - Campo.Unidades.OfType<Barricada>().Count();

    /// <summary>Las clases del escuadrón que ya jugaron algún turno en esta oleada.</summary>
    public IReadOnlyCollection<ClaseUnidad> ClasesQueJugaron => _clasesQueJugaron;

    /// <summary>La nave del Hangar que acompaña esta oleada, si se armó una.</summary>
    public Nave? NaveDeApoyo { get; private set; }

    /// <summary>Golpes del Enjambre que las defensas del escuadrón frenaron enteros.</summary>
    public int GolpesAbsorbidos { get; private set; }

    public int HackeosRealizados { get; private set; }

    /// <summary>Cuántos hackeos admite esta oleada: los de siempre más los que suma el nivel.</summary>
    public int HackeosPermitidos { get; private set; } = HackeosPorOleada;

    public int HackeosDisponibles => HackeosPermitidos - HackeosRealizados;

    /// <summary>Las unidades del escuadrón que cambiaron de estado en esta oleada y siguen en pie.</summary>
    public IEnumerable<UnidadEscuadron> SobrevivientesConEstado => Escuadron.Where(_cambiaronDeEstado.Contains);

    public bool YaActuo(Unidad unidad) => _yaActuaron.Contains(unidad);

    // Lo que la unidad puede hacer depende también de su estado: aturdida, pierde el turno.
    public bool PuedeActuar(UnidadEscuadron unidad) =>
        Estado == EstadoOleada.TurnoJugador && unidad.EstaViva && !YaActuo(unidad) && unidad.Estado.PuedeActuar;

    public bool PuedeMover(UnidadEscuadron unidad) =>
        PuedeActuar(unidad) && unidad.Estado.Mover(unidad) && Campo.HayLugar(unidad.Bando, Opuesta(unidad.Fila));

    public IReadOnlyList<Unidad> ObjetivosDeAtaque(UnidadEscuadron unidad) =>
        PuedeActuar(unidad) && unidad.PuedeAtacar ? Campo.Atacables(unidad.Bando) : [];

    public IReadOnlyList<Unidad> ObjetivosDeCuracion(UnidadEscuadron unidad) =>
        PuedeActuar(unidad) && unidad.Clase == ClaseUnidad.Medico
            ? Escuadron.Where(u => u.Vida < u.VidaMaxima).ToList<Unidad>()
            : [];

    public bool PuedeConstruir(UnidadEscuadron unidad) =>
        PuedeActuar(unidad) && BarricadasDisponibles > 0 && Campo.HayLugar(Bando.Escuadron, Fila.Vanguardia);

    /// <summary>Suma la nave de apoyo a la oleada. Se llama antes de que el escuadrón haga su primera jugada.</summary>
    public void RecibirApoyo(Nave nave)
    {
        Exigir(NaveDeApoyo is null && Turno == 1 && _yaActuaron.Count == 0, "La nave de apoyo llega antes de empezar la oleada.");

        NaveDeApoyo = nave;
        Bitacora.Registrar("Hangar", $"La nave {nave.Modelo} acompaña al escuadrón.", TipoEntrada.Exito);

        if (nave.RevelaIA)
        {
            foreach (var enemigo in Campo.UnidadesVivasDe(Bando.Enjambre).OfType<Enemigo>())
                enemigo.IARevelada = true;
            Bitacora.Registrar("Hangar", "El radar identifica la IA de cada enemigo.", TipoEntrada.Exito);
        }

        if (nave.DanioDeApoyo > 0)
        {
            // El disparo ablanda a la vanguardia pero no remata: la oleada se gana peleando.
            foreach (var enemigo in Campo.EnFila(Bando.Enjambre, Fila.Vanguardia).ToList())
                enemigo.PerderVida(Math.Min(nave.DanioDeApoyo, enemigo.Vida - 1));
            Bitacora.Registrar("Hangar", $"Disparo de apoyo: {nave.DanioDeApoyo} de daño a la vanguardia enemiga.", TipoEntrada.Exito);
        }

        if (nave.BonusCreditos > 0)
            Bitacora.Registrar("Hangar", $"Con la bodega, ganar la oleada da {nave.BonusCreditos}% más de créditos.", TipoEntrada.Exito);
    }

    /// <summary>Material que mandó la Tierra: barricadas de más para esta oleada. Llega antes de la primera jugada.</summary>
    public void RecibirRefuerzo(int barricadas)
    {
        Exigir(Turno == 1 && _yaActuaron.Count == 0, "Los refuerzos llegan antes de empezar la oleada.");
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(barricadas);

        _barricadasExtra += barricadas;
        var cuantas = barricadas == 1 ? "una barricada más" : $"{barricadas} barricadas más";
        Bitacora.Registrar("Comunicaciones", $"Llega material de la Tierra: {cuantas} para esta oleada.", TipoEntrada.Exito);
    }

    /// <summary>Las ventajas que da el nivel del comandante. Se aplican antes de la primera jugada.</summary>
    public void AmpliarLimites(int hackeos, int barricadas, string motivo)
    {
        Exigir(Turno == 1 && _yaActuaron.Count == 0, "Las ventajas del nivel se aplican antes de empezar la oleada.");
        if (hackeos <= 0 && barricadas <= 0)
            return;

        HackeosPermitidos += hackeos;
        _barricadasExtra += barricadas;
        var partes = new List<string>();
        if (hackeos > 0)
            partes.Add(hackeos == 1 ? "un hackeo más" : $"{hackeos} hackeos más");
        if (barricadas > 0)
            partes.Add(barricadas == 1 ? "una barricada más" : $"{barricadas} barricadas más");
        Bitacora.Registrar("Progreso", $"Por {motivo}: {string.Join(" y ", partes)} en esta oleada.", TipoEntrada.Exito);
    }

    public bool PuedeHackear(Enemigo enemigo) =>
        Estado == EstadoOleada.TurnoJugador && HackeosDisponibles > 0 && enemigo.EstaViva && Campo.Unidades.Contains(enemigo);

    /// <summary>
    /// Le cambia la IA a un enemigo en plena oleada. No gasta la acción de ninguna unidad
    /// y no es un comando: no se puede deshacer.
    /// </summary>
    public void Hackear(Enemigo enemigo, IEstrategiaIA nueva)
    {
        Exigir(PuedeHackear(enemigo), $"No se puede hackear a {enemigo.Nombre}.");

        var anterior = enemigo.Estrategia.Nombre;
        enemigo.Estrategia = nueva;
        enemigo.IARevelada = true;
        HackeosRealizados++;

        var texto = anterior == nueva.Nombre
            ? $"{enemigo.Nombre} ya usaba la IA {anterior}. Queda al descubierto."
            : $"{enemigo.Nombre} pasa de IA {anterior} a IA {nueva.Nombre}.";
        Bitacora.Registrar("Hackeo", texto, TipoEntrada.Advertencia);
    }

    public void Mover(UnidadEscuadron unidad)
    {
        Exigir(PuedeMover(unidad), $"{unidad.Nombre} no puede cambiar de fila.");
        JugarTurno(unidad, new OrdenTurno(TipoOrden.Mover));
    }

    public void Atacar(UnidadEscuadron atacante, Unidad objetivo)
    {
        Exigir(ObjetivosDeAtaque(atacante).Contains(objetivo), $"{atacante.Nombre} no puede atacar a {objetivo.Nombre}.");
        JugarTurno(atacante, new OrdenTurno(TipoOrden.Atacar, objetivo));
    }

    public void Curar(UnidadEscuadron medico, Unidad objetivo)
    {
        Exigir(ObjetivosDeCuracion(medico).Contains(objetivo), $"{medico.Nombre} no puede curar a {objetivo.Nombre}.");
        JugarTurno(medico, new OrdenTurno(TipoOrden.Curar, objetivo));
    }

    public void Construir(UnidadEscuadron constructor)
    {
        Exigir(PuedeConstruir(constructor), $"{constructor.Nombre} no puede construir una barricada.");
        JugarTurno(constructor, new OrdenTurno(TipoOrden.Construir));
    }

    /// <returns>El comando deshecho, o null si no había nada para deshacer.</returns>
    public IComando? Deshacer()
    {
        if (Estado != EstadoOleada.TurnoJugador || !Comandos.PuedeDeshacer)
            return null;

        var comando = Comandos.Deshacer();
        _yaActuaron.Remove(comando.Actor);
        Bitacora.Registrar("Deshacer", comando.Descripcion, TipoEntrada.Advertencia);
        return comando;
    }

    public IComando? Rehacer()
    {
        if (Estado != EstadoOleada.TurnoJugador || !Comandos.PuedeRehacer)
            return null;

        var comando = Comandos.Rehacer();
        _yaActuaron.Add(comando.Actor);
        Bitacora.Registrar("Rehacer", comando.Descripcion, TipoEntrada.Exito);
        ChequearFin();
        return comando;
    }

    public void TerminarTurno()
    {
        Exigir(Estado == EstadoOleada.TurnoJugador, "No es el turno del escuadrón.");

        Comandos.ConfirmarTurno();
        _yaActuaron.Clear();
        _ultimasJugadas.Clear();

        CerrarTurnoDe(Bando.Escuadron);
        ChequearFin();
        if (Terminada)
            return;

        foreach (var enemigo in Campo.UnidadesVivasDe(Bando.Enjambre).OfType<Enemigo>().OrderBy(e => e.Fila))
            _enemigosPorJugar.Enqueue(enemigo);

        Estado = EstadoOleada.TurnoEnjambre;
        Bitacora.Registrar(Origen, "Turno del Enjambre.", TipoEntrada.Peligro);
    }

    /// <summary>Juega el próximo enemigo. Devuelve null cuando ya jugaron todos y vuelve a tocarle al escuadrón.</summary>
    public JugadaEnemiga? JugarSiguienteEnemigo()
    {
        if (Estado != EstadoOleada.TurnoEnjambre)
            return null;

        while (_enemigosPorJugar.TryDequeue(out var enemigo))
        {
            if (!enemigo.EstaViva)
                continue;
            var jugada = JugarEnemigo(enemigo);
            ChequearFin();
            return jugada;
        }

        CerrarTurnoDe(Bando.Enjambre);
        ChequearFin();
        if (Terminada)
            return null;

        Turno++;
        Estado = EstadoOleada.TurnoJugador;
        Bitacora.Registrar(Origen, $"Turno {Turno}: le toca al escuadrón.");
        return null;
    }

    public void JugarTurnoEnjambre()
    {
        while (JugarSiguienteEnemigo() is not null)
        {
        }
    }

    // Qué hacer lo decide la IA que el enemigo tenga en ese momento (Strategy). Acá solo se ejecuta.
    private JugadaEnemiga JugarEnemigo(Enemigo enemigo)
    {
        var ia = enemigo.Estrategia.Nombre;
        var decision = enemigo.Estado.PuedeActuar
            ? Validar(enemigo, enemigo.DecidirAccion(new Contexto(Campo, Turno, Aleatorio)))
            : DecisionIA.Esperar($"está {enemigo.Estado.Nombre.ToLowerInvariant()} y pierde el turno");
        var objetivo = decision.Accion == AccionIA.Atacar ? decision.Objetivo : null;

        var jugada = new JugadaEnemiga(enemigo, objetivo, Aplicar(enemigo, decision), ia, decision);
        _ultimasJugadas.Add(jugada);
        return jugada;
    }

    // Las reglas del combate las pone la oleada: si la IA pide algo que no se puede, el enemigo espera.
    private DecisionIA Validar(Enemigo enemigo, DecisionIA decision)
    {
        var sePuede = decision.Accion switch
        {
            AccionIA.Atacar => enemigo.PuedeAtacar && decision.Objetivo is { } objetivo && Campo.Atacables(Bando.Enjambre).Contains(objetivo),
            AccionIA.Avanzar => enemigo.Estado.Mover(enemigo) && enemigo.Fila != Fila.Vanguardia && Campo.HayLugar(Bando.Enjambre, Fila.Vanguardia),
            AccionIA.Retroceder => enemigo.Estado.Mover(enemigo) && enemigo.Fila != Fila.Retaguardia && Campo.HayLugar(Bando.Enjambre, Fila.Retaguardia),
            _ => true,
        };
        return sePuede ? decision : DecisionIA.Esperar("no pudo hacer lo que quería");
    }

    private EntradaBitacora Aplicar(Enemigo enemigo, DecisionIA decision)
    {
        switch (decision.Accion)
        {
            case AccionIA.Atacar when decision.Objetivo is { } objetivo:
            {
                var estadoAnterior = objetivo.Estado;
                var golpe = new Danio(enemigo.DanioDeAtaque(), TipoDanio.Fisico, enemigo);
                var vidaQuitada = objetivo.RecibirGolpe(golpe);
                var texto = !objetivo.EstaViva ? $"{enemigo.Nombre} derriba a {objetivo.Nombre} (-{vidaQuitada})"
                    : vidaQuitada == 0 ? $"{enemigo.Nombre} ataca a {objetivo.Nombre}, pero no le saca vida"
                    : $"{enemigo.Nombre} ataca a {objetivo.Nombre} (-{vidaQuitada})";
                var entrada = Bitacora.Registrar("Enjambre", texto, TipoEntrada.Peligro);
                RegistrarDefensa(objetivo, golpe);
                RegistrarCambioDeEstado(objetivo, estadoAnterior);
                return entrada;
            }
            case AccionIA.Avanzar:
                Campo.Mover(enemigo, Fila.Vanguardia);
                return Bitacora.Registrar("Enjambre", $"{enemigo.Nombre} avanza");
            case AccionIA.Retroceder:
                Campo.Mover(enemigo, Fila.Retaguardia);
                return Bitacora.Registrar("Enjambre", $"{enemigo.Nombre} se repliega");
            default:
                return Bitacora.Registrar("Enjambre", enemigo.Estado.PuedeActuar
                    ? $"{enemigo.Nombre} espera"
                    : $"{enemigo.Nombre} está {enemigo.Estado.Nombre.ToLowerInvariant()} y pierde el turno");
        }
    }

    // Si la unidad lleva capas además de la vida, queda en la bitácora cómo las fue atravesando el golpe.
    private void RegistrarDefensa(Unidad objetivo, Danio golpe)
    {
        if (objetivo.Defensas.Eslabones.Count <= 1)
            return;

        var frenado = golpe.VidaPerdida == 0;
        if (frenado)
            GolpesAbsorbidos++;
        Bitacora.Registrar($"Defensas · {objetivo.Nombre}", golpe.DescribirRecorrido(), frenado ? TipoEntrada.Exito : TipoEntrada.Info);
    }

    // Cuando un golpe deja a la unidad en otro estado (State), queda anotado.
    private void RegistrarCambioDeEstado(Unidad unidad, IEstadoPersonaje anterior)
    {
        if (!unidad.EstaViva || ReferenceEquals(unidad.Estado, anterior))
            return;

        _cambiaronDeEstado.Add(unidad);
        Bitacora.Registrar($"Estado · {unidad.Nombre}",
            $"pasa de {anterior.Nombre} a {unidad.Estado.Nombre}: {unidad.Estado.Efecto}", TipoEntrada.Advertencia);
    }

    // Al cerrar el turno de un bando, el estado de cada una de sus unidades hace lo suyo:
    // el veneno resta vida, la aturdida se recupera, la furiosa se calma si ya está mejor.
    private void CerrarTurnoDe(Bando bando)
    {
        foreach (var unidad in Campo.UnidadesVivasDe(bando).ToList())
        {
            var anterior = unidad.Estado;
            if (anterior.TerminarTurno(unidad) is not { } aviso)
                continue;

            if (!ReferenceEquals(unidad.Estado, anterior))
                _cambiaronDeEstado.Add(unidad);
            var tipo = unidad.EstaViva ? TipoEntrada.Advertencia
                : bando == Bando.Escuadron ? TipoEntrada.Peligro
                : TipoEntrada.Exito;
            Bitacora.Registrar($"Estado · {unidad.Nombre}", aviso, tipo);
        }
    }

    // La orden ya está validada. El turno de la unidad sigue sus etapas (Template Method),
    // arma el comando y deja cada etapa en la bitácora.
    private void JugarTurno(UnidadEscuadron unidad, OrdenTurno orden)
    {
        var estadoObjetivo = orden.Objetivo?.Estado;
        TurnoUnidad.Para(unidad, Campo).EjecutarTurno(orden, Comandos, Bitacora);
        if (orden.Objetivo is { } objetivo && estadoObjetivo is not null)
            RegistrarCambioDeEstado(objetivo, estadoObjetivo);
        _yaActuaron.Add(unidad);
        _clasesQueJugaron.Add(unidad.Clase);
        ChequearFin();
    }

    private void ChequearFin()
    {
        if (!Campo.UnidadesVivasDe(Bando.Enjambre).Any())
        {
            Estado = EstadoOleada.Victoria;
            Comandos.ConfirmarTurno();
            Bitacora.Registrar(Origen, $"Oleada {Numero} superada.", TipoEntrada.Exito);
        }
        else if (!Escuadron.Any())
        {
            Estado = EstadoOleada.Derrota;
            Bitacora.Registrar(Origen, "Cayó todo el escuadrón.", TipoEntrada.Peligro);
        }
    }

    private void Ubicar(Unidad unidad)
    {
        var preferida = unidad.AtacaADistancia ? Fila.Retaguardia : Fila.Vanguardia;
        Campo.Agregar(unidad, Campo.HayLugar(unidad.Bando, preferida) ? preferida : Opuesta(preferida));
    }

    private static Fila Opuesta(Fila fila) => fila == Fila.Vanguardia ? Fila.Retaguardia : Fila.Vanguardia;

    private static void Exigir(bool condicion, string mensaje)
    {
        if (!condicion)
            throw new InvalidOperationException(mensaje);
    }
}
