using Kepler7.BE;
using Kepler7.Core.Juego;
using Kepler7.Core.Progreso;

namespace Kepler7.Game.Pantallas;

/// <summary>El cierre de la partida: si la estación se salvó o se apagó, y el resumen de lo que se hizo.</summary>
internal class PantallaFinal : Pantalla
{
    public PantallaFinal(FormPrincipal principal, ResumenPartida resumen, bool victoria) : base(principal)
    {
        var titulo = Tema.CrearEtiqueta(victoria ? "Kepler-7 está a salvo" : "Kepler-7 se apagó", Tema.Titulo, victoria ? Tema.Exito : Tema.Peligro);
        var subtitulo = Tema.CrearEtiqueta(victoria
            ? $"El escuadrón resistió las {ProgresoJugador.OleadasTotales} oleadas del Enjambre. La estación sigue en pie."
            : $"El escuadrón cayó en la oleada {resumen.OleadasSuperadas + 1}. El Enjambre tomó la estación.",
            Tema.Subtitulo, Tema.TextoTenue);
        subtitulo.Margin = new Padding(6, 0, 3, 24);

        var datos = new TableLayoutPanel { AutoSize = true, ColumnCount = 2, Margin = new Padding(3, 0, 3, 20), BackColor = Color.Transparent };
        foreach (var (nombre, valor) in Renglones(resumen))
        {
            var etiqueta = Tema.CrearEtiqueta(nombre, Tema.Chica, Tema.TextoTenue);
            etiqueta.Margin = new Padding(3, 6, 24, 0);
            var dato = Tema.CrearEtiqueta(valor, Tema.Negrita, Tema.Texto);
            dato.Margin = new Padding(3, 4, 3, 0);
            dato.MaximumSize = new Size(400, 0);
            datos.Controls.Add(etiqueta);
            datos.Controls.Add(dato);
        }

        // Los datos generales a la izquierda y la tabla de oleadas a la derecha.
        var cuerpo = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0), BackColor = Color.Transparent };
        cuerpo.Controls.AddRange([datos, CrearTablaOleadas(resumen)]);

        var volver = Tema.CrearBoton("Menú principal", 260);
        volver.Click += (_, _) =>
        {
            if (victoria)
                Principal.IrAInicio();
            else
                Principal.TerminarPartida();
        };

        var columna = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent,
        };
        columna.Controls.AddRange([titulo, subtitulo, cuerpo, volver]);

        Controls.Add(columna);
        MantenerCentrado(columna);
    }

    private static Control CrearTablaOleadas(ResumenPartida r)
    {
        var columna = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(36, 6, 3, 20),
            BackColor = Color.Transparent,
        };
        columna.Controls.Add(Tema.CrearEtiqueta("OLEADA POR OLEADA", Tema.Chica, Tema.TextoTenue));

        var tabla = new TableLayoutPanel { AutoSize = true, ColumnCount = 6, Margin = new Padding(0, 8, 0, 0), BackColor = Color.Transparent };
        foreach (var titulo in new[] { "N°", "RESULTADO", "TURNOS", "ENEMIGOS", "BAJAS", "RECOMPENSA" })
            tabla.Controls.Add(Celda(titulo, Tema.Chica, Tema.TextoTenue));
        foreach (var o in r.Oleadas)
        {
            tabla.Controls.Add(Celda(o.Numero.ToString(), Tema.Negrita, Tema.Texto));
            tabla.Controls.Add(Celda(o.Victoria ? "superada" : "perdida", Tema.Negrita, o.Victoria ? Tema.Exito : Tema.Peligro));
            tabla.Controls.Add(Celda(o.Turnos.ToString(), Tema.Normal, Tema.Texto));
            tabla.Controls.Add(Celda($"{o.EnemigosDerrotados} de {o.Enemigos}", Tema.Normal, Tema.Texto));
            tabla.Controls.Add(Celda(o.Bajas.ToString(), Tema.Normal, o.Bajas > 0 ? Tema.Advertencia : Tema.Texto));
            tabla.Controls.Add(Celda(o.Victoria ? $"+{o.Xp} XP · +{o.Creditos} cr." : "-", Tema.Normal, Tema.Texto));
        }
        columna.Controls.Add(tabla);

        // Al continuar una partida guardada, lo de antes no está en memoria.
        if (r.Oleadas.Count < r.OleadasSuperadas)
        {
            var nota = Tema.CrearEtiqueta("Las oleadas jugadas antes de continuar la partida no quedan en la tabla.", Tema.Chica, Tema.TextoTenue);
            nota.MaximumSize = new Size(420, 0);
            nota.Margin = new Padding(3, 10, 3, 0);
            columna.Controls.Add(nota);
        }
        return columna;
    }

    private static string Cantidad(int n, string uno, string varios) => $"{n} {(n == 1 ? uno : varios)}";

    private static Label Celda(string texto, Font fuente, Color color)
    {
        var celda = Tema.CrearEtiqueta(texto, fuente, color);
        celda.Margin = new Padding(3, 5, 18, 0);
        return celda;
    }

    private static IEnumerable<(string, string)> Renglones(ResumenPartida r)
    {
        yield return ("OLEADAS SUPERADAS", $"{r.OleadasSuperadas} de {ProgresoJugador.OleadasTotales}");
        yield return ("NIVEL", $"{r.Nivel} ({r.Xp} XP)");
        yield return ("CRÉDITOS", r.Creditos.ToString());
        yield return ("INSIGNIAS", r.Insignias.Count == 0
            ? $"ninguna de {CatalogoInsignias.Todas.Count}"
            : $"{r.Insignias.Count} de {CatalogoInsignias.Todas.Count}: {string.Join(", ", r.Insignias.Select(i => i.Nombre))}");
        yield return ("ESCUADRÓN", string.Join(" · ", new[]
        {
            r.Recluta is null ? "sin recluta del Censo" : $"recluta {r.Recluta}",
            r.Clon is null ? "sin clon del Laboratorio" : $"clon {r.Clon}",
        }));
        if (r.Oleadas.Count > 0)
            yield return ("COMBATE", string.Join(" · ",
                Cantidad(r.Oleadas.Sum(o => o.EnemigosDerrotados), "enemigo derrotado", "enemigos derrotados"),
                Cantidad(r.Oleadas.Sum(o => o.Bajas), "baja propia", "bajas propias"),
                Cantidad(r.Oleadas.Sum(o => o.GolpesAbsorbidos), "golpe absorbido", "golpes absorbidos"),
                Cantidad(r.Oleadas.Sum(o => o.Hackeos), "hackeo", "hackeos")));
        yield return ("COMUNICACIONES", r.Transmisiones == 1 ? "1 mensaje a la Tierra" : $"{r.Transmisiones} mensajes a la Tierra");
        yield return ("BÓVEDA", $"{r.AccesosALaBoveda} accesos autorizados · {r.IntentosDenegados} intentos denegados en el registro");
        var ventajas = Desbloqueos.Alcanzados(r.Nivel);
        yield return ("VENTAJAS DEL NIVEL", ventajas.Count == 0 ? "ninguna" : string.Join(", ", ventajas.Select(d => d.Nombre)));
    }
}
