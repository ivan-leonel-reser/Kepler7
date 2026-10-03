using Kepler7.BE;
using Kepler7.Core.Juego;
using Kepler7.Core.Progreso;

namespace Kepler7.Game.Pantallas;

/// <summary>
/// Base para la pantalla de cada módulo de la estación: encabezado con el patrón y botón para volver.
/// Lo propio de cada módulo se agrega en <see cref="Contenido"/>.
/// </summary>
internal class PantallaModulo : Pantalla
{
    protected PantallaModulo(FormPrincipal principal, string titulo, Patron patron, string descripcion) : base(principal)
    {
        Partida = principal.PartidaActual
            ?? throw new InvalidOperationException("No hay una partida en curso.");

        Contenido = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

        var familia = CatalogoInsignias.Obtener(patron).Familia.ToString().ToUpperInvariant();
        var lblTitulo = Tema.CrearEtiqueta(titulo, Tema.Encabezado, Tema.Acento);
        lblTitulo.Location = new Point(20, 8);
        var lblPatron = Tema.CrearEtiqueta($"PATRÓN {ModulosEstacion.NombrePatron(patron).ToUpperInvariant()} · {familia}", Tema.Chica, Tema.TextoTenue);
        lblPatron.Location = new Point(22, 44);
        var lblDescripcion = Tema.CrearEtiqueta(descripcion, Tema.Normal, Tema.Texto);
        lblDescripcion.Location = new Point(22, 64);

        var volver = Tema.CrearBoton("Volver a la estación", 200);
        volver.Click += (_, _) => Volver();
        var zonaVolver = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 230, Padding = new Padding(0, 24, 12, 0) };
        zonaVolver.Controls.Add(volver);

        var encabezado = new Panel { Dock = DockStyle.Top, Height = 96, BackColor = Tema.Panel };
        encabezado.Controls.AddRange([zonaVolver, lblTitulo, lblPatron, lblDescripcion]);

        Controls.Add(Contenido);
        Controls.Add(encabezado);
    }

    protected Partida Partida { get; }

    protected Panel Contenido { get; }

    protected virtual void Volver() => Principal.IrAEstacion();
}
