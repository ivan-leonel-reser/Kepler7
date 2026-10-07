using Kepler7.BE;
using Kepler7.Core.Juego;
using Kepler7.Core.Progreso;
using Kepler7.Game.Controles;

namespace Kepler7.Game.Pantallas;

/// <summary>El panel de las 12 insignias, una por patrón, y las ventajas que da cada nivel.</summary>
internal class PantallaLogros : Pantalla
{
    public PantallaLogros(FormPrincipal principal) : base(principal)
    {
        var partida = principal.PartidaActual
            ?? throw new InvalidOperationException("No hay una partida en curso.");
        var progreso = partida.Progreso;

        Controls.Add(CrearGrilla(progreso));
        Controls.Add(CrearDesbloqueos(progreso));
        Controls.Add(CrearEncabezado(progreso));
    }

    private Control CrearEncabezado(ProgresoJugador progreso)
    {
        var titulo = Tema.CrearEtiqueta("Logros · insignias de la estación", Tema.Encabezado, Tema.Acento);
        titulo.Location = new Point(20, 10);
        var detalle = Tema.CrearEtiqueta(
            $"{progreso.Insignias.Count} de {CatalogoInsignias.Todas.Count} patrones dominados · nivel {progreso.Nivel} · {progreso.Xp} XP en total",
            Tema.Normal, Tema.Texto);
        detalle.Location = new Point(22, 50);

        var volver = Tema.CrearBoton("Volver a la estación", 200);
        volver.Click += (_, _) => Principal.IrAEstacion();
        var zonaVolver = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 230, Padding = new Padding(0, 22, 12, 0) };
        zonaVolver.Controls.Add(volver);

        var encabezado = new Panel { Dock = DockStyle.Top, Height = 88, BackColor = Tema.Panel };
        encabezado.Controls.AddRange([zonaVolver, titulo, detalle]);
        return encabezado;
    }

    private static Control CrearGrilla(ProgresoJugador progreso)
    {
        const int columnas = 4;
        var insignias = CatalogoInsignias.Todas;
        var filas = (insignias.Count + columnas - 1) / columnas;
        var grilla = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = columnas,
            RowCount = filas,
            Padding = new Padding(12, 12, 4, 8),
        };
        for (var c = 0; c < columnas; c++)
            grilla.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columnas));
        for (var f = 0; f < filas; f++)
            grilla.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / filas));

        foreach (var insignia in insignias)
            grilla.Controls.Add(new TarjetaInsignia(insignia, progreso.TieneInsignia(insignia.Patron)));
        return grilla;
    }

    // A la derecha, qué da cada nivel y cuáles ya se tienen.
    private static Control CrearDesbloqueos(ProgresoJugador progreso)
    {
        var columna = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            Width = 300,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(8, 14, 12, 0),
            BackColor = Tema.Panel,
        };
        columna.Controls.Add(Tema.CrearEtiqueta("VENTAJAS POR NIVEL", Tema.Chica, Tema.TextoTenue));

        foreach (var desbloqueo in Desbloqueos.Todos)
        {
            var alcanzado = desbloqueo.Nivel <= progreso.Nivel;
            var nombre = Tema.CrearEtiqueta($"Nivel {desbloqueo.Nivel} · {desbloqueo.Nombre}", Tema.Negrita, alcanzado ? Tema.Exito : Tema.TextoTenue);
            nombre.Margin = new Padding(3, 12, 3, 0);
            var detalle = Tema.CrearEtiqueta(desbloqueo.Descripcion + (alcanzado ? " Ya la tenés." : ""), Tema.Chica, alcanzado ? Tema.Texto : Tema.TextoTenue);
            detalle.MaximumSize = new Size(270, 0);
            columna.Controls.AddRange([nombre, detalle]);
        }

        var proximo = Desbloqueos.Proximo(progreso.Nivel);
        var pie = Tema.CrearEtiqueta(proximo is null
            ? "Ya tenés todas las ventajas."
            : $"Faltan {(proximo.Nivel - 1) * ProgresoJugador.XpPorNivel - progreso.Xp} XP para el nivel {proximo.Nivel}.", Tema.Chica, Tema.Advertencia);
        pie.Margin = new Padding(3, 20, 3, 0);
        pie.MaximumSize = new Size(270, 0);
        columna.Controls.Add(pie);
        return columna;
    }
}
