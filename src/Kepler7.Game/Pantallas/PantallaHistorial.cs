using Kepler7.BE;
using Kepler7.Core.Persistencia;
using Kepler7.Core.Progreso;

namespace Kepler7.Game.Pantallas;

/// <summary>Las últimas partidas que se terminaron en esta máquina, ganadas o perdidas.</summary>
internal class PantallaHistorial : Pantalla
{
    private const int Cantidad = 10;

    public PantallaHistorial(FormPrincipal principal) : base(principal)
    {
        var titulo = Tema.CrearEtiqueta("Historial de partidas", Tema.Encabezado, Tema.Acento);

        var columna = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
        };
        columna.Controls.Add(titulo);

        // La interfaz no conoce la capa de datos: cualquier falla al leer se muestra como aviso.
        IReadOnlyList<PartidaJugada> partidas;
        try
        {
            partidas = Repositorios.CrearHistorial().Ultimas(Cantidad);
        }
        catch (Exception e)
        {
            partidas = [];
            columna.Controls.Add(Aviso($"No se pudo leer el historial: {e.Message}", Tema.Peligro));
        }

        if (partidas.Count > 0)
        {
            var salvadas = partidas.Count(p => p.EstacionSalvada);
            var cuantas = partidas.Count == 1 ? "Última partida" : $"Últimas {partidas.Count} partidas";
            columna.Controls.Add(Aviso($"{cuantas} · {salvadas} con la estación salvada", Tema.Texto));
            columna.Controls.Add(CrearTabla(partidas));
        }
        else if (columna.Controls.Count == 1)
        {
            columna.Controls.Add(Aviso("Todavía no terminó ninguna partida. Cuando una termine, ganada o perdida, aparece acá.", Tema.TextoTenue));
        }

        var guardado = Tema.CrearEtiqueta($"Guardado: {Repositorios.Descripcion}", Tema.Chica, Tema.TextoTenue);
        guardado.Margin = new Padding(3, 16, 3, 0);
        guardado.MaximumSize = new Size(760, 0);

        var volver = Tema.CrearBoton("Volver");
        volver.Margin = new Padding(3, 20, 3, 0);
        volver.Click += (_, _) => Principal.IrAInicio();

        columna.Controls.AddRange([guardado, volver]);
        Controls.Add(columna);
        MantenerCentrado(columna);
    }

    private static Control CrearTabla(IReadOnlyList<PartidaJugada> partidas)
    {
        var tabla = new TableLayoutPanel { AutoSize = true, ColumnCount = 6, Margin = new Padding(0, 14, 0, 0), BackColor = Color.Transparent };
        foreach (var titulo in new[] { "FECHA", "RESULTADO", "OLEADAS", "NIVEL", "XP", "INSIGNIAS" })
            tabla.Controls.Add(Celda(titulo, Tema.Chica, Tema.TextoTenue));

        foreach (var p in partidas)
        {
            tabla.Controls.Add(Celda(p.Fecha.ToString("dd/MM/yyyy HH:mm"), Tema.Normal, Tema.Texto));
            tabla.Controls.Add(Celda(p.EstacionSalvada ? "estación salvada" : "estación perdida", Tema.Negrita, p.EstacionSalvada ? Tema.Exito : Tema.Peligro));
            tabla.Controls.Add(Celda($"{p.OleadasSuperadas} de {ProgresoJugador.OleadasTotales}", Tema.Normal, Tema.Texto));
            tabla.Controls.Add(Celda(p.Nivel.ToString(), Tema.Normal, Tema.Texto));
            tabla.Controls.Add(Celda(p.Xp.ToString(), Tema.Normal, Tema.Texto));
            tabla.Controls.Add(Celda($"{p.Insignias} de {CatalogoInsignias.Todas.Count}", Tema.Normal, Tema.Texto));
        }
        return tabla;
    }

    private static Label Aviso(string texto, Color color)
    {
        var aviso = Tema.CrearEtiqueta(texto, Tema.Normal, color);
        aviso.Margin = new Padding(6, 8, 3, 0);
        aviso.MaximumSize = new Size(760, 0);
        return aviso;
    }

    private static Label Celda(string texto, Font fuente, Color color)
    {
        var celda = Tema.CrearEtiqueta(texto, fuente, color);
        celda.Margin = new Padding(3, 6, 22, 0);
        return celda;
    }
}
