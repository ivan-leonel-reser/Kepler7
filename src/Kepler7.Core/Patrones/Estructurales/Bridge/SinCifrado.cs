namespace Kepler7.Core.Patrones.Estructurales.Bridge;

/// <summary>El mensaje viaja tal cual. No cuesta nada, pero el Enjambre lo puede leer.</summary>
public class SinCifrado : ICifrado
{
    public const int Riesgo = 50;

    public string Nombre => "Sin cifrado";

    public string Descripcion => "Se transmite en claro. Gratis, pero el Enjambre puede interceptarlo.";

    public int Costo => 0;

    public int RiesgoIntercepcion => Riesgo;

    public string Cifrar(string textoPlano) => textoPlano;

    public string Descifrar(string textoCifrado) => textoCifrado;
}
