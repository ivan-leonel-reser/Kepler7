namespace Kepler7.Core.Patrones.Estructurales.Bridge;

/// <summary>El lado de la implementación del puente: cómo se cifra lo que sale por la antena. Los mensajes no saben cuál llevan.</summary>
public interface ICifrado
{
    string Nombre { get; }

    string Descripcion { get; }

    /// <summary>Créditos que cuesta transmitir con este cifrado.</summary>
    int Costo { get; }

    /// <summary>Probabilidad (0 a 100) de que el Enjambre lea el mensaje.</summary>
    int RiesgoIntercepcion { get; }

    string Cifrar(string textoPlano);

    string Descifrar(string textoCifrado);
}
