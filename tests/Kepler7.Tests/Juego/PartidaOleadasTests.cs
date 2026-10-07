using Kepler7.BE;
using Kepler7.Core.Combate;
using Kepler7.Core.Juego;
using Kepler7.Core.Patrones.Comportamiento.ChainOfResponsibility;
using Kepler7.Core.Patrones.Comportamiento.State;
using Kepler7.Core.Patrones.Comportamiento.Strategy;
using Kepler7.Core.Patrones.Comportamiento.Visitor;
using Kepler7.Core.Patrones.Creacionales.Builder;
using Kepler7.Core.Patrones.Creacionales.Prototype;
using Kepler7.Core.Patrones.Estructurales.Facade;
using Kepler7.Core.Patrones.Estructurales.Proxy;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Juego;

[TestClass]
public class PartidaOleadasTests
{
    private RepositorioProgresoFalso _repositorio = null!;
    private Partida _partida = null!;

    [TestInitialize]
    public void Preparar()
    {
        _repositorio = new RepositorioProgresoFalso();
        _partida = Partida.Nueva(_repositorio, new AleatorioFijo());
    }

    private static void GanarRapido(Oleada oleada)
    {
        foreach (var enemigo in oleada.Campo.UnidadesVivasDe(Bando.Enjambre).ToList())
            enemigo.PerderVida(enemigo.Vida - 1);
        foreach (var unidad in oleada.Escuadron.ToList())
        {
            var objetivos = oleada.ObjetivosDeAtaque(unidad);
            if (objetivos.Count > 0)
                oleada.Atacar(unidad, objetivos[0]);
        }
    }

    [TestMethod]
    public void ConLaEstacionApagadaNoSePuedeDefender()
    {
        Assert.IsFalse(_partida.PuedeDefender);
        Assert.ThrowsExactly<InvalidOperationException>(() => _partida.EmpezarOleada());
    }

    [TestMethod]
    public void DefenderPoneLaEstacionEnEmergencia()
    {
        _partida.ControlCentral.IniciarTurno();

        var oleada = _partida.EmpezarOleada();

        Assert.AreEqual(ModoEstacion.Emergencia, _partida.ControlCentral.Modo);
        Assert.AreEqual(1, oleada.Numero);
        Assert.IsTrue(oleada.Bitacora.Entradas.Any(e => e.Origen == "Seguridad"));
    }

    [TestMethod]
    public void GanarDaRecompensaYVuelveLaNormalidad()
    {
        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();
        GanarRapido(oleada);

        var resultado = _partida.CerrarOleada();

        Assert.IsTrue(resultado.Victoria);
        Assert.AreEqual(1, _partida.Progreso.OleadasSuperadas);
        Assert.AreEqual(resultado.Creditos, _partida.Progreso.Creditos);
        Assert.AreEqual(ModoEstacion.Operativa, _partida.ControlCentral.Modo);
        Assert.AreEqual(2, _partida.NumeroProximaOleada);
        Assert.AreSame(_partida.Progreso, _repositorio.Guardado);
    }

    [TestMethod]
    public void PerderApagaLaEstacionYTerminaLaPartida()
    {
        // Sin capas de defensa, para que el escuadrón caiga en dos turnos.
        foreach (var capa in _partida.Armeria.Capas.ToList())
            _partida.Armeria.Quitar(capa);
        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();
        foreach (var unidad in _partida.Escuadron)
            unidad.PerderVida(unidad.Vida - 1);

        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();
        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();
        var resultado = _partida.CerrarOleada();

        Assert.IsFalse(resultado.Victoria);
        Assert.AreEqual(ModoEstacion.Apagada, _partida.ControlCentral.Modo);
        Assert.IsTrue(_partida.Terminada);
        Assert.IsNull(_repositorio.Guardado);
        Assert.IsFalse(_partida.PuedeDefender);
    }

    [TestMethod]
    public void CadaOleadaEmpiezaConElEscuadronCurado()
    {
        _partida.ControlCentral.IniciarTurno();
        _partida.Escuadron[0].PerderVida(20);

        _partida.EmpezarOleada();

        Assert.IsTrue(_partida.Escuadron.All(u => u.Vida == u.VidaMaxima));
    }

    [TestMethod]
    public void DeshacerYRehacerEnUnaOleadaDaLaInsigniaDeCommand()
    {
        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();
        var comandante = _partida.Escuadron[0];

        oleada.Atacar(comandante, oleada.ObjetivosDeAtaque(comandante)[0]);
        oleada.Deshacer();
        oleada.Rehacer();

        Assert.IsTrue(_partida.RevisarLogros().Any(i => i.Patron == Patron.Command));
    }

    [TestMethod]
    public void LaNaveExploradoraRevelaLaIAYSeUsaEnUnaSolaOleada()
    {
        _partida.ControlCentral.IniciarTurno();
        _partida.EnsamblarNave(new RecetaExploradora());

        var oleada = _partida.EmpezarOleada();

        Assert.IsTrue(oleada.Campo.UnidadesVivasDe(Bando.Enjambre).OfType<Enemigo>().All(e => e.IARevelada));
        Assert.AreEqual("Exploradora", oleada.NaveDeApoyo?.Modelo);
        Assert.IsNull(_partida.NaveDeApoyo);
    }

    [TestMethod]
    public void LaNaveDeCombateGolpeaALaVanguardiaSinRematar()
    {
        _partida.ControlCentral.IniciarTurno();
        var nave = _partida.EnsamblarNave(new RecetaCombate());

        var oleada = _partida.EmpezarOleada();

        var adelante = oleada.Campo.EnFila(Bando.Enjambre, Fila.Vanguardia).ToList();
        Assert.IsTrue(adelante.All(e => e.Vida == e.VidaMaxima - nave.DanioDeApoyo));
        Assert.IsFalse(oleada.Terminada);
    }

    [TestMethod]
    public void LaNaveDeCargaTraeMasCreditos()
    {
        _partida.ControlCentral.IniciarTurno();
        _partida.EnsamblarNave(new RecetaCarga());
        var oleada = _partida.EmpezarOleada();
        GanarRapido(oleada);

        var resultado = _partida.CerrarOleada();

        // La oleada 1 da 75 créditos; con la bodega, la mitad más.
        Assert.AreEqual(112, resultado.Creditos);
    }

    [TestMethod]
    public void SinNaveLaOleadaEmpiezaComoSiempre()
    {
        _partida.ControlCentral.IniciarTurno();

        var oleada = _partida.EmpezarOleada();

        Assert.IsNull(oleada.NaveDeApoyo);
        Assert.IsFalse(oleada.Campo.UnidadesVivasDe(Bando.Enjambre).OfType<Enemigo>().Any(e => e.IARevelada));
    }

    [TestMethod]
    public void ArmarLasTresRecetasDaLaInsigniaDeBuilder()
    {
        _partida.EnsamblarNave(new RecetaExploradora());
        _partida.EnsamblarNave(new RecetaCombate());
        _partida.EnsamblarNave(new RecetaCombate());
        Assert.IsFalse(_partida.RevisarLogros().Any(i => i.Patron == Patron.Builder));

        _partida.EnsamblarNave(new RecetaCarga());

        Assert.IsTrue(_partida.RevisarLogros().Any(i => i.Patron == Patron.Builder));
        Assert.AreEqual("Carga", _partida.NaveDeApoyo?.Modelo);
    }

    [TestMethod]
    public void CadaOleadaEquipaAlEscuadronConLaCadenaDeLaArmeria()
    {
        _partida.Armeria.Quitar(TipoCapa.Esquiva);
        _partida.Armeria.Subir(TipoCapa.Escudo);
        _partida.ControlCentral.IniciarTurno();

        _partida.EmpezarOleada();

        foreach (var unidad in _partida.Escuadron)
            CollectionAssert.AreEqual(
                new[] { "Bloqueo", "Escudo", "Armadura", "Vida" },
                unidad.Defensas.Eslabones.Select(e => e.Nombre).ToArray(),
                unidad.Nombre);
    }

    [TestMethod]
    public void ElEscudoYLosBloqueosSeRecarganEnCadaOleada()
    {
        _partida.ControlCentral.IniciarTurno();
        var primera = _partida.EmpezarOleada();
        var vega = _partida.Escuadron[0];
        vega.RecibirGolpe(new Danio(20));
        GanarRapido(primera);
        _partida.CerrarOleada();

        _partida.EmpezarOleada();

        var escudo = vega.Defensas.Eslabones.OfType<Escudo>().Single();
        Assert.AreEqual(Escudo.PuntosPorOleada, escudo.Puntos);
        Assert.AreEqual(Bloqueo.UsosPorOleada, vega.Defensas.Eslabones.OfType<Bloqueo>().Single().UsosRestantes);
    }

    [TestMethod]
    public void FrenarUnGolpeEnteroDaLaInsigniaDeChain()
    {
        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();
        Assert.IsFalse(_partida.RevisarLogros().Any(i => i.Patron == Patron.ChainOfResponsibility));

        // Los Zánganos de la oleada 1 pegan de a 2: el bloqueo los para enteros.
        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();

        Assert.IsGreaterThan(0, oleada.GolpesAbsorbidos);
        Assert.IsTrue(_partida.Escuadron.All(u => u.Vida == u.VidaMaxima));
        Assert.IsTrue(oleada.Bitacora.Entradas.Any(e => e.Origen.StartsWith("Defensas") && e.Mensaje.Contains("no pierde vida")));
        Assert.IsTrue(_partida.RevisarLogros().Any(i => i.Patron == Patron.ChainOfResponsibility));
    }

    [TestMethod]
    public void GanarUnaOleadaDejaSuBotinEnElAlmacen()
    {
        var alEmpezar = _partida.Almacen.Items.Count;
        _partida.ControlCentral.IniciarTurno();
        GanarRapido(_partida.EmpezarOleada());

        _partida.CerrarOleada();

        Assert.HasCount(alEmpezar + GeneradorBotin.Crear(1).Count, _partida.Almacen.Items);
        Assert.IsTrue(_partida.Almacen.Items.Any(i => i.Nombre == "Aguijón de Zángano"));
    }

    [TestMethod]
    public void VenderElAlmacenDaLosCreditosQueCalculaElVisitante()
    {
        var calculador = new CalculadorPrecio();
        _partida.Almacen.Aplicar(calculador);

        var ganado = _partida.VenderAlmacen();

        Assert.AreEqual(calculador.Total, ganado);
        Assert.AreEqual(calculador.Total, _partida.Progreso.Creditos);
        Assert.IsEmpty(_partida.Almacen.Items);
        Assert.AreEqual(0, _partida.VenderAlmacen());
    }

    [TestMethod]
    public void EnvenenarLasArmasSubeLoQueSeCobraAlVender()
    {
        var sinVeneno = new CalculadorPrecio();
        _partida.Almacen.Aplicar(sinVeneno);

        _partida.AnalizarAlmacen(new AplicadorVeneno());

        Assert.IsGreaterThan(sinVeneno.Total, _partida.VenderAlmacen());
    }

    [TestMethod]
    public void UsarLosTresAnalizadoresDaLaInsigniaDeVisitor()
    {
        _partida.AnalizarAlmacen(new CalculadorPrecio());
        _partida.AnalizarAlmacen(new CalculadorPrecio());
        _partida.AnalizarAlmacen(new ContadorArmas());
        Assert.IsFalse(_partida.RevisarLogros().Any(i => i.Patron == Patron.Visitor));

        _partida.AnalizarAlmacen(new AplicadorVeneno());

        Assert.IsTrue(_partida.RevisarLogros().Any(i => i.Patron == Patron.Visitor));
    }

    [TestMethod]
    public void ReclutarSumaUnSoldadoConElBonusDeSuEspecie()
    {
        _partida.Progreso.GanarCreditos(Partida.CostoRecluta);
        _partida.GenerarCenso();
        var abisal = _partida.Censo.Tripulantes.First(t => t.Especie.Nombre == "Abisal");
        var soldadoBase = UnidadEscuadron.Crear("Base", ClaseUnidad.Soldado);

        var recluta = _partida.Reclutar(abisal);

        Assert.HasCount(5, _partida.Escuadron);
        Assert.AreEqual(abisal.Nombre, recluta.Nombre);
        Assert.AreEqual(ClaseUnidad.Soldado, recluta.Clase);
        Assert.AreEqual(soldadoBase.VidaMaxima + abisal.Especie.BonusVida, recluta.VidaMaxima);
        Assert.AreEqual(soldadoBase.Ataque + abisal.Especie.BonusAtaque, recluta.Ataque);
        Assert.AreSame(abisal, _partida.Recluta);
        Assert.AreEqual(0, _partida.Progreso.Creditos);
    }

    [TestMethod]
    public void SinCreditosNoSePuedeReclutar()
    {
        _partida.GenerarCenso();

        Assert.IsFalse(_partida.PuedeReclutar);
        Assert.ThrowsExactly<InvalidOperationException>(() => _partida.Reclutar(_partida.Censo.Tripulantes[0]));
        Assert.HasCount(4, _partida.Escuadron);
    }

    [TestMethod]
    public void UnReclutaNuevoReemplazaAlAnterior()
    {
        _partida.Progreso.GanarCreditos(2 * Partida.CostoRecluta);
        _partida.GenerarCenso();
        var primero = _partida.Reclutar(_partida.Censo.Tripulantes[0]);

        var segundo = _partida.Reclutar(_partida.Censo.Tripulantes[1]);

        Assert.HasCount(5, _partida.Escuadron);
        Assert.DoesNotContain(primero, _partida.Escuadron);
        Assert.Contains(segundo, _partida.Escuadron);
    }

    [TestMethod]
    public void ElReclutaSaleAPelearConElEscuadron()
    {
        _partida.Progreso.GanarCreditos(Partida.CostoRecluta);
        _partida.GenerarCenso();
        var recluta = _partida.Reclutar(_partida.Censo.Tripulantes[0]);
        _partida.ControlCentral.IniciarTurno();

        var oleada = _partida.EmpezarOleada();

        Assert.Contains(recluta, oleada.Escuadron.ToList());
        Assert.IsTrue(oleada.PuedeActuar(recluta));
    }

    [TestMethod]
    public void ReclutarDaLaInsigniaDeFlyweight()
    {
        _partida.Progreso.GanarCreditos(Partida.CostoRecluta);
        _partida.GenerarCenso();
        Assert.IsFalse(_partida.RevisarLogros().Any(i => i.Patron == Patron.Flyweight));

        _partida.Reclutar(_partida.Censo.Tripulantes[0]);

        Assert.IsTrue(_partida.RevisarLogros().Any(i => i.Patron == Patron.Flyweight));
    }

    [TestMethod]
    public void ElClonSeSumaAlEscuadronConLoQueAprendio()
    {
        _partida.Progreso.GanarCreditos(Partida.CostoClon);
        var francotirador = _partida.Escuadron.Single(u => u.Clase == ClaseUnidad.Francotirador);
        var original = _partida.FichaDe(francotirador);
        var clon = (FichaSoldado)original.Clonar();
        clon.Nombre = "Luna II";
        clon.Aprender("Tiro certero");

        var unidad = _partida.SumarClon(original, clon);

        Assert.HasCount(5, _partida.Escuadron);
        Assert.AreEqual("Luna II", unidad.Nombre);
        Assert.AreEqual(ClaseUnidad.Francotirador, unidad.Clase);
        Assert.AreEqual(francotirador.Ataque + 2, unidad.Ataque);
        Assert.AreSame(clon, _partida.Clon);
        Assert.HasCount(1, original.Habilidades);
        Assert.AreEqual(0, _partida.Progreso.Creditos);
    }

    [TestMethod]
    public void SinCreditosNoSeSumaElClon()
    {
        var original = _partida.FichaDe(_partida.Escuadron[0]);

        Assert.IsFalse(_partida.PuedeClonar);
        Assert.ThrowsExactly<InvalidOperationException>(() => _partida.SumarClon(original, (FichaSoldado)original.Clonar()));
        Assert.HasCount(4, _partida.Escuadron);
    }

    [TestMethod]
    public void UnClonNuevoReemplazaAlAnteriorYNoSePuedeClonarAlClon()
    {
        _partida.Progreso.GanarCreditos(2 * Partida.CostoClon);
        var original = _partida.FichaDe(_partida.Escuadron[0]);
        var primero = _partida.SumarClon(original, (FichaSoldado)original.Clonar());
        Assert.DoesNotContain(primero, _partida.Clonables.ToList());

        var segundo = _partida.SumarClon(original, (FichaSoldado)original.Clonar());

        Assert.HasCount(5, _partida.Escuadron);
        Assert.DoesNotContain(primero, _partida.Escuadron);
        Assert.Contains(segundo, _partida.Escuadron);
    }

    [TestMethod]
    public void ElClonDeUnReclutaConservaElBonusDeLaEspecie()
    {
        _partida.Progreso.GanarCreditos(Partida.CostoRecluta + Partida.CostoClon);
        _partida.GenerarCenso();
        var tripulante = _partida.Censo.Tripulantes.First(t => t.Especie.Nombre == "Kepleriano");
        var recluta = _partida.Reclutar(tripulante);
        var original = _partida.FichaDe(recluta);

        var unidad = _partida.SumarClon(original, (FichaSoldado)original.Clonar());

        Assert.AreSame(tripulante.Especie, original.Especie);
        Assert.AreEqual(recluta.VidaMaxima, unidad.VidaMaxima);
        Assert.HasCount(6, _partida.Escuadron);
    }

    [TestMethod]
    public void ModificarElClonDaLaInsigniaDePrototype()
    {
        _partida.Progreso.GanarCreditos(2 * Partida.CostoClon);
        var original = _partida.FichaDe(_partida.Escuadron[0]);
        _partida.SumarClon(original, (FichaSoldado)original.Clonar());
        Assert.IsFalse(_partida.RevisarLogros().Any(i => i.Patron == Patron.Prototype));

        var modificado = (FichaSoldado)original.Clonar();
        modificado.Aprender("Resistencia");
        _partida.SumarClon(original, modificado);

        Assert.IsTrue(_partida.RevisarLogros().Any(i => i.Patron == Patron.Prototype));
        Assert.HasCount(1, original.Habilidades);
    }

    [TestMethod]
    public void JugarConLasCuatroClasesDaLaInsigniaDeTemplateMethod()
    {
        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();
        var comandante = _partida.Escuadron.Single(u => u.Clase == ClaseUnidad.Comandante);
        var soldado = _partida.Escuadron.Single(u => u.Clase == ClaseUnidad.Soldado);
        var medico = _partida.Escuadron.Single(u => u.Clase == ClaseUnidad.Medico);
        var francotirador = _partida.Escuadron.Single(u => u.Clase == ClaseUnidad.Francotirador);

        oleada.Atacar(comandante, oleada.ObjetivosDeAtaque(comandante)[0]);
        oleada.Mover(soldado);
        oleada.Mover(medico);
        Assert.IsFalse(_partida.RevisarLogros().Any(i => i.Patron == Patron.TemplateMethod));

        oleada.Atacar(francotirador, oleada.ObjetivosDeAtaque(francotirador)[0]);
        Assert.IsTrue(_partida.RevisarLogros().Any(i => i.Patron == Patron.TemplateMethod));
    }

    [TestMethod]
    public void HackearUnEnemigoDaLaInsigniaDeStrategy()
    {
        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();
        Assert.IsFalse(_partida.RevisarLogros().Any(i => i.Patron == Patron.Strategy));

        var enemigo = oleada.Campo.UnidadesVivasDe(Bando.Enjambre).OfType<Enemigo>().First();
        oleada.Hackear(enemigo, new IADefensiva());

        Assert.IsTrue(_partida.RevisarLogros().Any(i => i.Patron == Patron.Strategy));
        Assert.IsTrue(_partida.Progreso.TieneInsignia(Patron.Strategy));
    }

    [TestMethod]
    public void CadaOleadaEmpiezaConElEscuadronEnEstadoNormal()
    {
        _partida.ControlCentral.IniciarTurno();
        _partida.Escuadron[0].CambiarEstado(new Envenenado());

        _partida.EmpezarOleada();

        Assert.IsTrue(_partida.Escuadron.All(u => u.Estado is Normal));
    }

    [TestMethod]
    public void SinArmasEnvenenadasElEscuadronNoEnvenena()
    {
        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();
        var medico = _partida.Escuadron.First(u => u.Clase == ClaseUnidad.Medico);
        var objetivo = oleada.ObjetivosDeAtaque(medico)[0];

        oleada.Atacar(medico, objetivo);

        Assert.IsInstanceOfType<Normal>(objetivo.Estado);
    }

    [TestMethod]
    public void LasArmasEnvenenadasDelAlmacenEnvenenanAlEnemigo()
    {
        _partida.AnalizarAlmacen(new AplicadorVeneno());
        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();
        var medico = _partida.Escuadron.First(u => u.Clase == ClaseUnidad.Medico);
        var objetivo = oleada.ObjetivosDeAtaque(medico)[0];

        oleada.Atacar(medico, objetivo);

        Assert.IsInstanceOfType<Envenenado>(objetivo.Estado);
    }

    [TestMethod]
    public void SiSeVendenLasArmasEnvenenadasElEscuadronDejaDeEnvenenar()
    {
        _partida.AnalizarAlmacen(new AplicadorVeneno());
        _partida.VenderAlmacen();
        _partida.ControlCentral.IniciarTurno();

        _partida.EmpezarOleada();

        Assert.IsTrue(_partida.Escuadron.All(u => u.EfectoAlGolpear == EfectoGolpe.Ninguno));
    }

    [TestMethod]
    public void GanarConUnaUnidadQueCambioDeEstadoDaLaInsigniaDeState()
    {
        // Sin capas, para que los golpes del Enjambre lleguen a la vida.
        foreach (var capa in _partida.Armeria.Capas.ToList())
            _partida.Armeria.Quitar(capa);
        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();
        var soldado = _partida.Escuadron.First(u => u.Clase == ClaseUnidad.Soldado);
        soldado.PerderVida(soldado.Vida - 10);

        oleada.TerminarTurno();
        oleada.JugarTurnoEnjambre();
        Assert.IsInstanceOfType<Berserk>(soldado.Estado);
        Assert.IsFalse(_partida.RevisarLogros().Any(i => i.Patron == Patron.State));

        GanarRapido(oleada);

        Assert.IsTrue(_partida.RevisarLogros().Any(i => i.Patron == Patron.State));
        Assert.IsTrue(_partida.Progreso.TieneInsignia(Patron.State));
    }

    [TestMethod]
    public void LeerElInformeDeLaProximaOleadaRevelaLaIADeLosEnemigos()
    {
        _partida.Boveda.Credencial = CatalogoBoveda.Credenciales.First(c => c.Rol == Rol.Capitan);
        var informe = _partida.LeerArchivoBoveda(CatalogoBoveda.InformeOleada(1));
        Assert.Contains("Zángano", informe);
        Assert.IsTrue(_partida.InformeLeido);

        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();

        Assert.IsTrue(oleada.Campo.UnidadesVivasDe(Bando.Enjambre).OfType<Enemigo>().All(e => e.IARevelada));
        Assert.IsTrue(oleada.Bitacora.Entradas.Any(e => e.Origen == "Bóveda"));
    }

    [TestMethod]
    public void SinLeerElInformeLaIAQuedaOculta()
    {
        _partida.Boveda.Credencial = CatalogoBoveda.Credenciales.First(c => c.Rol == Rol.Capitan);
        _partida.LeerArchivoBoveda(CatalogoBoveda.InformeOleada(2));
        Assert.IsFalse(_partida.InformeLeido);

        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();

        Assert.IsFalse(oleada.Campo.UnidadesVivasDe(Bando.Enjambre).OfType<Enemigo>().Any(e => e.IARevelada));
    }

    [TestMethod]
    public void UnAccesoDenegadoQuedaRegistradoYEnLaBitacora()
    {
        _partida.Boveda.Credencial = CatalogoBoveda.Credenciales.First(c => c.Rol == Rol.Cadete);

        Assert.ThrowsExactly<AccesoDenegadoException>(() => _partida.LeerArchivoBoveda(CatalogoBoveda.InformeOleada(1)));

        Assert.HasCount(1, _partida.Boveda.IntentosDenegados);
        Assert.IsTrue(_partida.Bitacora.Entradas.Any(e => e.Origen == "Bóveda" && e.Tipo == TipoEntrada.Peligro));
        Assert.IsFalse(_partida.InformeLeido);
        Assert.IsFalse(_partida.RevisarLogros().Any(i => i.Patron == Patron.Proxy));
    }

    [TestMethod]
    public void EntrarConUnRolAutorizadoDaLaInsigniaDeProxy()
    {
        _partida.Boveda.Credencial = CatalogoBoveda.Credenciales.First(c => c.Rol == Rol.OficialDeSeguridad);

        _partida.EscribirArchivoBoveda(CatalogoBoveda.DiarioDelCapitan, "Oleada 1 sin novedades.");

        Assert.AreEqual("Oleada 1 sin novedades.", _partida.LeerArchivoBoveda(CatalogoBoveda.DiarioDelCapitan));
        Assert.IsTrue(_partida.RevisarLogros().Any(i => i.Patron == Patron.Proxy));
        Assert.IsTrue(_partida.Progreso.TieneInsignia(Patron.Proxy));
    }

    [TestMethod]
    public void ElRegistroDeLaBovedaSobreviveALaPartida()
    {
        var registro = new RegistroAccesosFalso();
        var primera = Partida.Nueva(_repositorio, new AleatorioFijo(), registro);
        primera.Boveda.Credencial = CatalogoBoveda.Credenciales.First(c => c.Rol == Rol.Soldado);
        Assert.ThrowsExactly<AccesoDenegadoException>(() => primera.LeerArchivoBoveda(CatalogoBoveda.DiarioDelCapitan));

        var segunda = Partida.Nueva(_repositorio, new AleatorioFijo(), registro);

        Assert.HasCount(1, segunda.Boveda.IntentosDenegados);
    }

    [TestMethod]
    public void GanarSinCambiosDeEstadoNoDaLaInsigniaDeState()
    {
        _partida.ControlCentral.IniciarTurno();
        var oleada = _partida.EmpezarOleada();

        GanarRapido(oleada);

        Assert.IsFalse(_partida.RevisarLogros().Any(i => i.Patron == Patron.State));
    }
}
