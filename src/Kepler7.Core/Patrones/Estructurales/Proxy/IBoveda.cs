namespace Kepler7.Core.Patrones.Estructurales.Proxy;

/// <summary>Lo que se le puede pedir a la bóveda de datos. Lo implementan la bóveda real y su proxy.</summary>
public interface IBoveda
{
    string LeerArchivo(string id);

    void EscribirArchivo(string id, string contenido);
}
