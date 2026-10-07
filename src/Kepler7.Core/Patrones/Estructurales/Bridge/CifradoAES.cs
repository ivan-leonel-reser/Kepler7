using System.Security.Cryptography;
using System.Text;

namespace Kepler7.Core.Patrones.Estructurales.Bridge;

/// <summary>AES de verdad, con la clave de la estación. El vector de inicialización viaja adelante del mensaje.</summary>
public class CifradoAES : ICifrado
{
    private const int LargoIV = 16;
    private static readonly byte[] Clave = SHA256.HashData(Encoding.UTF8.GetBytes("Kepler-7 · Protocolo Fénix"));

    public string Nombre => "AES";

    public string Descripcion => "Cifrado simétrico de 256 bits con la clave de la estación. Seguro y accesible.";

    public int Costo => 30;

    public int RiesgoIntercepcion => 0;

    public string Cifrar(string textoPlano)
    {
        using var aes = Aes.Create();
        aes.Key = Clave;
        aes.GenerateIV();
        var cifrado = aes.EncryptCbc(Encoding.UTF8.GetBytes(textoPlano), aes.IV);
        return Convert.ToBase64String([.. aes.IV, .. cifrado]);
    }

    public string Descifrar(string textoCifrado)
    {
        var bytes = Convert.FromBase64String(textoCifrado);
        using var aes = Aes.Create();
        aes.Key = Clave;
        return Encoding.UTF8.GetString(aes.DecryptCbc(bytes[LargoIV..], bytes[..LargoIV]));
    }
}
