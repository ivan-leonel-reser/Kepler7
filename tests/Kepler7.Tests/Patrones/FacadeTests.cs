using Kepler7.Core.Comun;
using Kepler7.Core.Patrones.Estructurales.Facade;

namespace Kepler7.Tests.Patrones;

[TestClass]
public class FacadeTests
{
    [TestMethod]
    public void LaEstacionArrancaApagada()
    {
        var control = new ControlCentral();

        Assert.AreEqual(ModoEstacion.Apagada, control.Modo);
        Assert.HasCount(6, control.Subsistemas);
        Assert.IsTrue(control.Subsistemas.All(s => s.Estado == EstadoOperativo.Apagado));
    }

    [TestMethod]
    public void IniciarTurno_PrendeLosSeisEmpezandoPorLaEnergia()
    {
        var control = new ControlCentral();

        var secuencia = control.IniciarTurno();

        Assert.AreEqual(ModoEstacion.Operativa, control.Modo);
        Assert.IsTrue(control.Subsistemas.All(s => s.Estado == EstadoOperativo.Operativo));
        Assert.HasCount(7, secuencia);
        Assert.AreEqual("Energía", secuencia[0].Origen);
        Assert.AreEqual(1, control.TurnosIniciados);
    }

    [TestMethod]
    public void Emergencia_ConLaEstacionApagadaNoHaceNada()
    {
        var control = new ControlCentral();

        var secuencia = control.EntrarEnEmergencia();

        Assert.HasCount(1, secuencia);
        Assert.AreEqual(TipoEntrada.Advertencia, secuencia[0].Tipo);
        Assert.AreEqual(ModoEstacion.Apagada, control.Modo);
        Assert.AreEqual(0, control.EmergenciasDeclaradas);
    }

    [TestMethod]
    public void Emergencia_PoneTodoEnAlertaEmpezandoPorSeguridad()
    {
        var control = new ControlCentral();
        control.IniciarTurno();

        var secuencia = control.EntrarEnEmergencia();

        Assert.AreEqual(ModoEstacion.Emergencia, control.Modo);
        Assert.IsTrue(control.Subsistemas.All(s => s.Estado == EstadoOperativo.Alerta));
        Assert.AreEqual("Seguridad", secuencia[0].Origen);
    }

    [TestMethod]
    public void Apagar_DejaElReactorParaElFinal()
    {
        var control = new ControlCentral();
        control.IniciarTurno();

        var secuencia = control.ApagarEstacion();

        Assert.AreEqual(ModoEstacion.Apagada, control.Modo);
        Assert.IsTrue(control.Subsistemas.All(s => s.Estado == EstadoOperativo.Apagado));
        Assert.AreEqual("Energía", secuencia[^2].Origen);
    }

    [TestMethod]
    public void CadaOperacionDejaUnResumenEnLaBitacoraDeLaEstacion()
    {
        var bitacora = new Bitacora();
        var control = new ControlCentral(bitacora);

        control.IniciarTurno();
        control.EntrarEnEmergencia();

        Assert.HasCount(2, bitacora.Entradas);
    }

    [TestMethod]
    public void LosSubsistemasNoSonAccesiblesDesdeAfuera()
    {
        var subsistemas = typeof(ControlCentral).Assembly.GetTypes()
            .Where(t => t.Namespace == typeof(ControlCentral).Namespace && t.Name.StartsWith("Sistema"))
            .ToList();

        Assert.HasCount(6, subsistemas);
        Assert.IsTrue(subsistemas.All(t => !t.IsPublic));
    }
}
