namespace Kepler7.Core.Patrones.Estructurales.Proxy;

/// <summary>La bóveda real: guarda los archivos y hace las operaciones. No sabe nada de roles ni permisos.</summary>
public class BovedaDatos : IBoveda
{
    private readonly Dictionary<string, string> _archivos;

    public BovedaDatos(IEnumerable<KeyValuePair<string, string>> archivos)
    {
        _archivos = new Dictionary<string, string>(archivos);
    }

    public IReadOnlyCollection<string> Ids => _archivos.Keys;

    public string LeerArchivo(string id) =>
        _archivos.TryGetValue(id, out var contenido)
            ? contenido
            : throw new KeyNotFoundException($"En la bóveda no hay ningún archivo '{id}'.");

    public void EscribirArchivo(string id, string contenido)
    {
        if (!_archivos.ContainsKey(id))
            throw new KeyNotFoundException($"En la bóveda no hay ningún archivo '{id}'.");

        _archivos[id] = contenido;
    }
}
