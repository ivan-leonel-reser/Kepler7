using Kepler7.BE;

namespace Kepler7.Core.Comun;

public class Bitacora
{
    private readonly List<EntradaBitacora> _entradas = [];

    public IReadOnlyList<EntradaBitacora> Entradas => _entradas;

    public EntradaBitacora Registrar(string origen, string mensaje, TipoEntrada tipo = TipoEntrada.Info)
    {
        var entrada = new EntradaBitacora(origen, mensaje, tipo);
        _entradas.Add(entrada);
        return entrada;
    }

    public EntradaBitacora Registrar(EntradaBitacora entrada)
    {
        _entradas.Add(entrada);
        return entrada;
    }

    public void Limpiar() => _entradas.Clear();
}
