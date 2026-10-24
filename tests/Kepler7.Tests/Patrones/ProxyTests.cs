using Kepler7.Core.Patrones.Estructurales.Proxy;
using Kepler7.Tests.Dobles;

namespace Kepler7.Tests.Patrones;

[TestClass]
public class ProxyTests
{
    private static readonly Credencial Capitan = new("Vega", Rol.Capitan);
    private static readonly Credencial Seguridad = new("Ortega", Rol.OficialDeSeguridad);
    private static readonly Credencial Soldado = new("Rojas", Rol.Soldado);

    private RegistroAccesosFalso _registro = null!;
    private int _vecesAbierta;
    private ProxyBoveda _proxy = null!;

    [TestInitialize]
    public void Preparar()
    {
        _registro = new RegistroAccesosFalso();
        _vecesAbierta = 0;
        _proxy = new ProxyBoveda(AbrirBoveda, _registro);
    }

    private IBoveda AbrirBoveda()
    {
        _vecesAbierta++;
        return new BovedaDatos([KeyValuePair.Create("plano", "Plano del reactor"), KeyValuePair.Create("diario", "Sin anotaciones.")]);
    }

    [TestMethod]
    public void LaBovedaRealLeeYEscribeSinPreguntarNada()
    {
        var boveda = new BovedaDatos([KeyValuePair.Create("plano", "Plano del reactor")]);

        boveda.EscribirArchivo("plano", "Plano actualizado");

        Assert.AreEqual("Plano actualizado", boveda.LeerArchivo("plano"));
        Assert.ThrowsExactly<KeyNotFoundException>(() => boveda.LeerArchivo("no-existe"));
        Assert.ThrowsExactly<KeyNotFoundException>(() => boveda.EscribirArchivo("no-existe", "x"));
    }

    [TestMethod]
    public void UnRolAutorizadoLeeATravesDelProxy()
    {
        _proxy.Credencial = Capitan;

        Assert.AreEqual("Plano del reactor", _proxy.LeerArchivo("plano"));
        Assert.AreEqual(1, _proxy.AccesosConcedidos);
        Assert.IsEmpty(_registro.Intentos);
    }

    [TestMethod]
    public void ElOficialDeSeguridadTambienEstaAutorizado()
    {
        _proxy.Credencial = Seguridad;

        _proxy.EscribirArchivo("diario", "Turno sin novedades.");

        Assert.AreEqual("Turno sin novedades.", _proxy.LeerArchivo("diario"));
        Assert.IsEmpty(_registro.Intentos);
    }

    [TestMethod]
    public void UnRolNoAutorizadoEsRechazadoYQuedaRegistrado()
    {
        _proxy.Credencial = Soldado;
        var antes = DateTime.Now;

        var error = Assert.ThrowsExactly<AccesoDenegadoException>(() => _proxy.LeerArchivo("plano"));

        var intento = Assert.ContainsSingle(_registro.Intentos);
        Assert.AreSame(intento, error.Intento);
        Assert.AreEqual("Rojas", intento.Usuario);
        Assert.AreEqual("Soldado", intento.Rol);
        Assert.AreEqual(ProxyBoveda.OperacionLeer, intento.Operacion);
        Assert.AreEqual("plano", intento.IdArchivo);
        Assert.IsGreaterThanOrEqualTo(antes, intento.Fecha);
        Assert.AreEqual(0, _proxy.AccesosConcedidos);
    }

    [TestMethod]
    public void EscribirSinPermisoTampocoLlegaALaBoveda()
    {
        _proxy.Credencial = Soldado;

        Assert.ThrowsExactly<AccesoDenegadoException>(() => _proxy.EscribirArchivo("diario", "Hola"));

        _proxy.Credencial = Capitan;
        Assert.AreEqual("Sin anotaciones.", _proxy.LeerArchivo("diario"));
        Assert.AreEqual(ProxyBoveda.OperacionEscribir, _registro.Intentos.Single().Operacion);
    }

    [TestMethod]
    public void SinCredencialTodoSeRechaza()
    {
        Assert.ThrowsExactly<AccesoDenegadoException>(() => _proxy.LeerArchivo("plano"));

        Assert.AreEqual("sin credencial", _registro.Intentos.Single().Rol);
    }

    [TestMethod]
    public void LaBovedaRealSeAbreRecienConElPrimerAccesoAutorizado()
    {
        _proxy.Credencial = Soldado;
        Assert.ThrowsExactly<AccesoDenegadoException>(() => _proxy.LeerArchivo("plano"));
        Assert.IsFalse(_proxy.BovedaAbierta);
        Assert.AreEqual(0, _vecesAbierta);

        _proxy.Credencial = Capitan;
        _proxy.LeerArchivo("plano");
        _proxy.LeerArchivo("diario");

        Assert.IsTrue(_proxy.BovedaAbierta);
        Assert.AreEqual(1, _vecesAbierta);
    }

    [TestMethod]
    public void ElRegistroAcumulaCadaIntentoDenegado()
    {
        _proxy.Credencial = Soldado;
        Assert.ThrowsExactly<AccesoDenegadoException>(() => _proxy.LeerArchivo("plano"));
        _proxy.Credencial = new Credencial("Nadir", Rol.Cadete);
        Assert.ThrowsExactly<AccesoDenegadoException>(() => _proxy.EscribirArchivo("diario", "x"));

        Assert.HasCount(2, _proxy.IntentosDenegados);
        CollectionAssert.AreEqual(new[] { "Rojas", "Nadir" }, _proxy.IntentosDenegados.Select(i => i.Usuario).ToArray());
    }

    [TestMethod]
    public void SiElRegistroNoRespondeElAccesoSeNiegaIgual()
    {
        var proxy = new ProxyBoveda(AbrirBoveda, new RegistroQueFalla()) { Credencial = Soldado };

        var error = Assert.ThrowsExactly<AccesoDenegadoException>(() => proxy.LeerArchivo("plano"));

        Assert.IsFalse(error.Registrado);
        Assert.AreEqual(0, _vecesAbierta);
    }

    [TestMethod]
    public void ConElRegistroCaidoElResumenDeLaPartidaNoSeRompe()
    {
        var partida = new Kepler7.Core.Juego.Partida(new RepositorioProgresoFalso(), new Kepler7.BE.ProgresoJugador(), new AleatorioFijo(),
            new RegistroQueFalla(), new HistorialQueFalla());
        partida.Boveda.Credencial = Soldado;

        Assert.ThrowsExactly<AccesoDenegadoException>(() => partida.LeerArchivoBoveda("informe-oleada-1"));

        Assert.IsNull(partida.Resumen().IntentosDenegados);
        Assert.IsTrue(partida.Bitacora.Entradas.Any(e => e.Mensaje.Contains("No quedó en el registro")));
    }

    [TestMethod]
    public void ElCatalogoTieneUnInformePorOleadaYElDiario()
    {
        var boveda = CatalogoBoveda.CrearBoveda();

        Assert.HasCount(7, CatalogoBoveda.Archivos);
        Assert.IsTrue(CatalogoBoveda.Archivos.All(a => boveda.Ids.Contains(a.Id)));
        Assert.Contains("Zángano 1", boveda.LeerArchivo(CatalogoBoveda.InformeOleada(1)));
        Assert.Contains("IA Agresiva", boveda.LeerArchivo(CatalogoBoveda.InformeOleada(1)));
        Assert.Contains("Reina del Enjambre", boveda.LeerArchivo(CatalogoBoveda.InformeOleada(6)));
        Assert.AreEqual(3, CatalogoBoveda.OleadaDelInforme(CatalogoBoveda.InformeOleada(3)));
        Assert.IsNull(CatalogoBoveda.OleadaDelInforme(CatalogoBoveda.DiarioDelCapitan));
    }

    [TestMethod]
    public void SoloCapitanYOficialDeSeguridadEstanAutorizados()
    {
        foreach (var credencial in CatalogoBoveda.Credenciales)
            Assert.AreEqual(credencial.Rol is Rol.Capitan or Rol.OficialDeSeguridad, ProxyBoveda.EstaAutorizada(credencial), credencial.ToString());
        Assert.IsFalse(ProxyBoveda.EstaAutorizada(null));
    }
}
