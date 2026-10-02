using Kepler7.Core.Comun;
using Kepler7.Core.Patrones.Comportamiento.Command;

namespace Kepler7.Core.Combate;

/// <summary>
/// El combate de una oleada. En el turno del escuadrón cada unidad hace una acción,
/// que se guarda como comando y se puede deshacer hasta terminar el turno.
/// Después juega el Enjambre, de a un enemigo.
/// </summary>
public class Oleada
{
    public const int BarricadasPorOleada = 2;
    public const int Curacion = 10;
    private const string Origen = "Oleada";

    private readonly HashSet<Unidad> _yaActuaron = [];
    private readonly Queue<Enemigo> _enemigosPorJugar = new();

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

    public IEnumerable<UnidadEscuadron> Escuadron =>
        Campo.UnidadesVivasDe(Bando.Escuadron).OfType<UnidadEscuadron>();

    public int BarricadasDisponibles => BarricadasPorOleada - Campo.Unidades.OfType<Barricada>().Count();

    public bool YaActuo(Unidad unidad) => _yaActuaron.Contains(unidad);

    public bool PuedeActuar(UnidadEscuadron unidad) =>
        Estado == EstadoOleada.TurnoJugador && unidad.EstaViva && !YaActuo(unidad);

    public bool PuedeMover(UnidadEscuadron unidad) =>
        PuedeActuar(unidad) && Campo.HayLugar(unidad.Bando, Opuesta(unidad.Fila));

    public IReadOnlyList<Unidad> ObjetivosDeAtaque(UnidadEscuadron unidad) =>
        PuedeActuar(unidad) && unidad.PuedeAtacar ? Campo.Atacables(unidad.Bando) : [];

    public IReadOnlyList<Unidad> ObjetivosDeCuracion(UnidadEscuadron unidad) =>
        PuedeActuar(unidad) && unidad.Clase == ClaseUnidad.Medico
            ? Escuadron.Where(u => u.Vida < u.VidaMaxima).ToList<Unidad>()
            : [];

    public bool PuedeConstruir(UnidadEscuadron unidad) =>
        PuedeActuar(unidad) && BarricadasDisponibles > 0 && Campo.HayLugar(Bando.Escuadron, Fila.Vanguardia);

    public void Mover(UnidadEscuadron unidad)
    {
        Exigir(PuedeMover(unidad), $"{unidad.Nombre} no puede cambiar de fila.");
        Ejecutar(new MoverComando(Campo, unidad, Opuesta(unidad.Fila)));
    }

    public void Atacar(UnidadEscuadron atacante, Unidad objetivo)
    {
        Exigir(ObjetivosDeAtaque(atacante).Contains(objetivo), $"{atacante.Nombre} no puede atacar a {objetivo.Nombre}.");
        Ejecutar(new AtacarComando(atacante, objetivo));
    }

    public void Curar(UnidadEscuadron medico, Unidad objetivo)
    {
        Exigir(ObjetivosDeCuracion(medico).Contains(objetivo), $"{medico.Nombre} no puede curar a {objetivo.Nombre}.");
        Ejecutar(new CurarComando(medico, objetivo, Curacion));
    }

    public void Construir(UnidadEscuadron constructor)
    {
        Exigir(PuedeConstruir(constructor), $"{constructor.Nombre} no puede construir una barricada.");
        Ejecutar(new ConstruirComando(Campo, constructor));
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

    // Hasta que esté el patrón Strategy todos los enemigos piensan igual: avanzan y le pegan al más débil.
    private JugadaEnemiga JugarEnemigo(Enemigo enemigo)
    {
        if (!enemigo.PuedeAtacar && Campo.HayLugar(Bando.Enjambre, Fila.Vanguardia))
        {
            Campo.Mover(enemigo, Fila.Vanguardia);
            return new JugadaEnemiga(enemigo, null, Bitacora.Registrar("Enjambre", $"{enemigo.Nombre} avanza"));
        }

        var objetivo = new Contexto(Campo, Turno, Aleatorio).AtacableMasDebil(enemigo);
        if (objetivo is null || !enemigo.PuedeAtacar)
            return new JugadaEnemiga(enemigo, null, Bitacora.Registrar("Enjambre", $"{enemigo.Nombre} espera"));

        var vidaQuitada = objetivo.RecibirGolpe(new Danio(enemigo.Ataque, TipoDanio.Fisico, enemigo));
        var texto = objetivo.EstaViva
            ? $"{enemigo.Nombre} ataca a {objetivo.Nombre} (-{vidaQuitada})"
            : $"{enemigo.Nombre} derriba a {objetivo.Nombre} (-{vidaQuitada})";
        return new JugadaEnemiga(enemigo, objetivo, Bitacora.Registrar("Enjambre", texto, TipoEntrada.Peligro));
    }

    private void Ejecutar(IComando comando)
    {
        Comandos.Ejecutar(comando);
        _yaActuaron.Add(comando.Actor);
        Bitacora.Registrar("Escuadrón", comando.Descripcion, TipoEntrada.Exito);
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
