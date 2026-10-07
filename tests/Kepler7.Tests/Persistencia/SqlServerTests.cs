using Kepler7.BE;
using Kepler7.DAL;
using Kepler7.DAL.SqlServer;

namespace Kepler7.Tests.Persistencia;

/// <summary>
/// Pruebas contra una base aparte (Kepler7_Pruebas). Si en la máquina no hay SQL Server, quedan como no concluyentes
/// en vez de fallar. La cadena se puede cambiar con la variable de entorno KEPLER7_SQL_PRUEBAS.
/// </summary>
[TestClass]
[DoNotParallelize]
public class SqlServerTests
{
    private const string CadenaPorDefecto = "Server=(localdb)\\MSSQLLocalDB;Database=Kepler7_Pruebas;Integrated Security=true;TrustServerCertificate=true";
    private static ConexionSql? _conexion;

    [ClassInitialize]
    public static void Preparar(TestContext _)
    {
        var cadena = Environment.GetEnvironmentVariable("KEPLER7_SQL_PRUEBAS") ?? CadenaPorDefecto;
        try
        {
            var conexion = new ConexionSql(cadena);
            conexion.AsegurarBase();
            _conexion = conexion;
        }
        catch (ErrorDePersistencia)
        {
            _conexion = null;
        }
    }

    private static ConexionSql Conexion =>
        _conexion ?? throw new AssertInconclusiveException("No hay SQL Server disponible para las pruebas.");

    [TestMethod]
    public void LaBaseSeCreaUnaVezYElScriptSePuedeRepetir()
    {
        Conexion.AsegurarBase();
        Conexion.AsegurarBase();

        Assert.AreEqual("Kepler7_Pruebas", Conexion.Base);
    }

    [TestMethod]
    public void GuardarYCargarDevuelveLaMismaPartida()
    {
        var repositorio = new RepositorioProgresoSql(Conexion);
        var progreso = new ProgresoJugador();
        progreso.GanarXp(230);
        progreso.GanarCreditos(175);
        progreso.SuperarOleada();
        progreso.SuperarOleada();
        progreso.DesbloquearInsignia(Patron.Proxy);
        progreso.DesbloquearInsignia(Patron.Bridge);

        repositorio.Guardar(progreso);
        var cargado = repositorio.Cargar();

        Assert.IsNotNull(cargado);
        Assert.AreEqual(230, cargado.Xp);
        Assert.AreEqual(175, cargado.Creditos);
        Assert.AreEqual(2, cargado.OleadasSuperadas);
        CollectionAssert.AreEquivalent(new[] { Patron.Proxy, Patron.Bridge }, cargado.Insignias.ToArray());
    }

    [TestMethod]
    public void GuardarDeNuevoReemplazaLaPartidaAnterior()
    {
        var repositorio = new RepositorioProgresoSql(Conexion);
        var primera = new ProgresoJugador();
        primera.DesbloquearInsignia(Patron.State);
        repositorio.Guardar(primera);

        var segunda = ProgresoJugador.Restaurar(10, 20, 1, [Patron.Facade]);
        repositorio.Guardar(segunda);
        var cargado = repositorio.Cargar();

        Assert.AreEqual(10, cargado!.Xp);
        CollectionAssert.AreEquivalent(new[] { Patron.Facade }, cargado.Insignias.ToArray());
    }

    [TestMethod]
    public void BorrarDejaLaBaseSinPartida()
    {
        var repositorio = new RepositorioProgresoSql(Conexion);
        repositorio.Guardar(new ProgresoJugador());

        repositorio.Borrar();

        Assert.IsNull(repositorio.Cargar());
    }

    [TestMethod]
    public void ElRegistroDeAccesosAcumulaLosIntentos()
    {
        var registro = new RegistroAccesosSql(Conexion);
        var antes = registro.ObtenerTodos().Count;
        var intento = new IntentoAcceso(new DateTime(2026, 10, 7, 18, 30, 0), "Rojas", "Soldado", "Leer", "informe-oleada-1");

        registro.Registrar(intento);
        var todos = registro.ObtenerTodos();

        Assert.HasCount(antes + 1, todos);
        Assert.AreEqual(intento, todos[^1]);
    }

    [TestMethod]
    public void UnServidorInexistenteAvisaConUnErrorDePersistencia()
    {
        var conexion = new ConexionSql("Server=np:\\\\.\\pipe\\no-existe\\sql\\query;Database=Kepler7;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=1");

        Assert.ThrowsExactly<ErrorDePersistencia>(conexion.AsegurarBase);
        Assert.ThrowsExactly<ErrorDePersistencia>(() => conexion.Abrir());
    }
}
