namespace Kepler7.Core.Patrones.Comportamiento.Command;

/// <summary>
/// Invocador: ejecuta los comandos y guarda las pilas para deshacer y rehacer.
/// Lo del turno actual se puede deshacer; al confirmar el turno pasa al historial.
/// </summary>
public class GestorComandos
{
    private readonly Stack<IComando> _deshacer = new();
    private readonly Stack<IComando> _rehacer = new();
    private readonly List<string> _historial = [];

    public bool PuedeDeshacer => _deshacer.Count > 0;
    public bool PuedeRehacer => _rehacer.Count > 0;
    public int VecesDeshecho { get; private set; }
    public int VecesRehecho { get; private set; }

    /// <summary>Lo que se hizo en este turno, en orden.</summary>
    public IReadOnlyList<IComando> AccionesDelTurno => _deshacer.Reverse().ToList();

    /// <summary>Lo confirmado en los turnos anteriores.</summary>
    public IReadOnlyList<string> Historial => _historial;

    public void Ejecutar(IComando comando)
    {
        comando.Ejecutar();
        _deshacer.Push(comando);
        // Una acción nueva invalida lo que se había deshecho.
        _rehacer.Clear();
    }

    public IComando Deshacer()
    {
        var comando = _deshacer.Pop();
        comando.Deshacer();
        _rehacer.Push(comando);
        VecesDeshecho++;
        return comando;
    }

    public IComando Rehacer()
    {
        var comando = _rehacer.Pop();
        comando.Ejecutar();
        _deshacer.Push(comando);
        VecesRehecho++;
        return comando;
    }

    public void ConfirmarTurno()
    {
        foreach (var comando in _deshacer.Reverse())
            _historial.Add(comando.Descripcion);
        _deshacer.Clear();
        _rehacer.Clear();
    }
}
