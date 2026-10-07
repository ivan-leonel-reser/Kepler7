using System.Security.Cryptography;
using System.Text;

namespace Kepler7.Core.Patrones.Estructurales.Bridge;

/// <summary>
/// Simula una clave cuántica de un solo uso: cada byte se mezcla con una clave tan larga como el mensaje,
/// derivada bloque a bloque del par entrelazado que comparten la estación y la Tierra.
/// </summary>
public class CifradoCuantico : ICifrado
{
    private const string ParEntrelazado = "par-entrelazado-kepler-7";
    private const int LargoBloque = 32;

    public string Nombre => "Cuántico";

    public string Descripcion => "Clave de un solo uso repartida por pares entrelazados. Si alguien la mira, el mensaje colapsa.";

    public int Costo => 60;

    public int RiesgoIntercepcion => 0;

    public string Cifrar(string textoPlano)
    {
        var hex = Convert.ToHexString(Mezclar(Encoding.UTF8.GetBytes(textoPlano)));
        return string.Join(" ", hex.Chunk(4).Select(grupo => new string(grupo)));
    }

    public string Descifrar(string textoCifrado) =>
        Encoding.UTF8.GetString(Mezclar(Convert.FromHexString(textoCifrado.Replace(" ", ""))));

    // Mezclar dos veces con la misma clave devuelve el original, así que sirve para los dos lados.
    private static byte[] Mezclar(byte[] datos)
    {
        var salida = new byte[datos.Length];
        var clave = Array.Empty<byte>();
        for (var i = 0; i < datos.Length; i++)
        {
            if (i % LargoBloque == 0)
                clave = SHA256.HashData(Encoding.UTF8.GetBytes($"{ParEntrelazado}:{i / LargoBloque}"));
            salida[i] = (byte)(datos[i] ^ clave[i % LargoBloque]);
        }
        return salida;
    }
}
