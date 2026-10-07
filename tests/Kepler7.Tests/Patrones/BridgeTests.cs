using Kepler7.Core.Patrones.Estructurales.Bridge;

namespace Kepler7.Tests.Patrones;

[TestClass]
public class BridgeTests
{
    private const string Contenido = "Enjambre detectado en el sector 7.";

    [TestMethod]
    public void SinCifradoElMensajeViajaTalCual()
    {
        var cifrado = new SinCifrado();

        Assert.AreEqual(Contenido, cifrado.Cifrar(Contenido));
        Assert.AreEqual(0, cifrado.Costo);
        Assert.IsGreaterThan(0, cifrado.RiesgoIntercepcion);
    }

    [TestMethod]
    public void AESCifraYLaTierraLoDescifra()
    {
        var cifrado = new CifradoAES();

        var secreto = cifrado.Cifrar(Contenido);

        Assert.AreNotEqual(Contenido, secreto);
        Assert.DoesNotContain("Enjambre", secreto);
        Assert.AreEqual(Contenido, cifrado.Descifrar(secreto));
        Assert.AreEqual(0, cifrado.RiesgoIntercepcion);
    }

    [TestMethod]
    public void ElCuanticoEsDeterministaYSeDeshace()
    {
        var cifrado = new CifradoCuantico();

        var secreto = cifrado.Cifrar(Contenido);

        Assert.AreEqual(secreto, cifrado.Cifrar(Contenido));
        Assert.AreNotEqual(Contenido, secreto);
        Assert.IsTrue(secreto.All(c => Uri.IsHexDigit(c) || c == ' '));
        Assert.AreEqual(Contenido, cifrado.Descifrar(secreto));
    }

    [TestMethod]
    public void CadaTipoDeMensajeArmaSuEncabezadoYPideSuRefuerzo()
    {
        var sinCifrado = new SinCifrado();
        var texto = new MensajeTexto(Contenido, sinCifrado);
        var alerta = new MensajeAlerta(Contenido, sinCifrado);
        var emergencia = new MensajeEmergencia(Contenido, sinCifrado);

        Assert.StartsWith("[TEXTO", texto.TextoPlano);
        Assert.StartsWith("[ALERTA", alerta.TextoPlano);
        Assert.StartsWith("[EMERGENCIA", emergencia.TextoPlano);
        Assert.IsTrue(new Mensaje[] { texto, alerta, emergencia }.All(m => m.TextoPlano.EndsWith(Contenido)));

        Assert.AreEqual((MensajeTexto.CreditosDeSuministros, 0), (texto.Refuerzo.Creditos, texto.Refuerzo.Barricadas));
        Assert.AreEqual((0, MensajeAlerta.BarricadasDeRefuerzo), (alerta.Refuerzo.Creditos, alerta.Refuerzo.Barricadas));
        Assert.AreEqual((MensajeEmergencia.CreditosDelConvoy, MensajeEmergencia.BarricadasDelConvoy), (emergencia.Refuerzo.Creditos, emergencia.Refuerzo.Barricadas));
    }

    [TestMethod]
    public void ElCifradoSeCambiaEnEjecucionSinTocarElMensaje()
    {
        var mensaje = new MensajeAlerta(Contenido, new SinCifrado());
        var enClaro = mensaje.TextoCifrado;

        mensaje.Cifrado = new CifradoCuantico();

        Assert.AreEqual(mensaje.TextoPlano, enClaro);
        Assert.AreNotEqual(enClaro, mensaje.TextoCifrado);
        Assert.AreEqual("Cuántico", mensaje.Enviar().Cifrado);
        Assert.AreEqual(60, mensaje.Enviar().Costo);
    }

    [TestMethod]
    public void LasNueveCombinacionesSalenDeTresMensajesYTresCifrados()
    {
        var transmisiones = CatalogoComunicaciones.Tipos
            .SelectMany(tipo => CatalogoComunicaciones.Cifrados.Select(cifrado => tipo.Crear(Contenido, cifrado).Enviar()))
            .ToList();

        Assert.HasCount(9, transmisiones);
        Assert.HasCount(9, transmisiones.Select(t => (t.Tipo, t.Cifrado)).Distinct().ToList());
        foreach (var transmision in transmisiones)
        {
            var cifrado = CatalogoComunicaciones.Cifrados.Single(c => c.Nombre == transmision.Cifrado);
            Assert.AreEqual(transmision.TextoPlano, cifrado.Descifrar(transmision.TextoCifrado));
        }
    }

    [TestMethod]
    public void ElEnviarDejaTodoLoQueLaInterfazNecesitaMostrar()
    {
        var transmision = new MensajeEmergencia(Contenido, new CifradoAES()).Enviar();

        Assert.AreEqual("Emergencia", transmision.Tipo);
        Assert.AreEqual("AES", transmision.Cifrado);
        Assert.Contains(Contenido, transmision.TextoPlano);
        Assert.AreNotEqual(transmision.TextoPlano, transmision.TextoCifrado);
        Assert.AreEqual(30, transmision.Costo);
        Assert.IsFalse(transmision.Interceptada);
    }
}
