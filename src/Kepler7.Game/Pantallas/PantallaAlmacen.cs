using Kepler7.BE;
using Kepler7.Core.Juego;
using Kepler7.Core.Patrones.Comportamiento.Visitor;
using Kepler7.Game.Controles;

namespace Kepler7.Game.Pantallas;

/// <summary>
/// El jugador elige un analizador y la partida lo pasea por el inventario.
/// La pantalla no sabe qué hace cada uno: muestra el detalle y el resultado que trae el visitante.
/// </summary>
internal class PantallaAlmacen : PantallaModulo
{
    private readonly ListBox _inventario = new()
    {
        Dock = DockStyle.Fill,
        BorderStyle = BorderStyle.None,
        BackColor = Color.FromArgb(5, 9, 16),
        ForeColor = Tema.Texto,
        Font = Tema.Normal,
        SelectionMode = SelectionMode.None,
        IntegralHeight = false,
    };
    private readonly Label _tituloInventario = new()
    {
        Dock = DockStyle.Top,
        Height = 22,
        Font = Tema.Chica,
        ForeColor = Tema.TextoTenue,
    };
    private readonly Button _vender = Tema.CrearBoton("Vender todo", 300);
    private readonly ConsolaBitacora _consola = new() { Dock = DockStyle.Fill };

    public PantallaAlmacen(FormPrincipal principal)
        : base(principal, "Almacén", Patron.Visitor,
            "Elegí un analizador para recorrer el arsenal y el botín de las oleadas. Lo que no uses, se vende.")
    {
        Contenido.Controls.Add(ArmarAnalizadores());
        Contenido.Controls.Add(ArmarInventario());

        _vender.Click += (_, _) => Vender();
        Refrescar();
    }

    private Control ArmarInventario()
    {
        var pie = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 56, Padding = new Padding(0, 8, 0, 0) };
        pie.Controls.Add(_vender);

        var columna = new Panel { Dock = DockStyle.Left, Width = 430 };
        columna.Controls.Add(_inventario);
        columna.Controls.Add(pie);
        columna.Controls.Add(_tituloInventario);
        return columna;
    }

    private Control ArmarAnalizadores()
    {
        var botones = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52 };
        var cantidad = CatalogoVisitantes.Crear().Count;
        for (var i = 0; i < cantidad; i++)
        {
            var indice = i;
            var boton = Tema.CrearBoton(CatalogoVisitantes.Crear()[indice].Nombre, 186);
            // Cada visitante acumula su resultado: se usa uno nuevo en cada pasada.
            boton.Click += (_, _) => Analizar(CatalogoVisitantes.Crear()[indice]);
            botones.Controls.Add(boton);
        }

        var zona = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16, 0, 0, 0) };
        zona.Controls.Add(_consola);
        zona.Controls.Add(botones);
        zona.Controls.Add(new Label
        {
            Text = "ANALIZADORES · CADA UNO RECORRE TODO EL INVENTARIO",
            Dock = DockStyle.Top,
            Height = 22,
            Font = Tema.Chica,
            ForeColor = Tema.TextoTenue,
        });
        return zona;
    }

    private void Analizar(IVisitor visitante)
    {
        Partida.AnalizarAlmacen(visitante);

        _consola.Items.Clear();
        _consola.Agregar(new EntradaBitacora("Visitante", visitante.Nombre, TipoEntrada.Advertencia));
        foreach (var linea in visitante.Detalle)
            _consola.Agregar(new EntradaBitacora("Visita", linea));
        _consola.Agregar(new EntradaBitacora("Resultado", visitante.Resultado, TipoEntrada.Exito));
        foreach (var insignia in Partida.RevisarLogros())
            _consola.Agregar(new EntradaBitacora("Logros", $"Insignia: {insignia.Nombre}", TipoEntrada.Exito));

        Refrescar();
    }

    private void Vender()
    {
        var respuesta = MessageBox.Show(
            $"Vas a vender los {Partida.Almacen.Items.Count} objetos del Almacén. No se puede deshacer.\n¿Querés seguir?",
            InfoJuego.Titulo, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (respuesta != DialogResult.Yes)
            return;

        var ganado = Partida.VenderAlmacen();
        _consola.Items.Clear();
        _consola.Agregar(new EntradaBitacora("Venta", $"Se vendió todo por {ganado} créditos. Ahora tenés {Partida.Progreso.Creditos}.", TipoEntrada.Exito));
        Refrescar();
    }

    private void Refrescar()
    {
        var items = Partida.Almacen.Items;
        _tituloInventario.Text = $"INVENTARIO · {items.Count} OBJETOS";

        _inventario.BeginUpdate();
        _inventario.Items.Clear();
        foreach (var item in items)
            _inventario.Items.Add($"{item.Nombre} · {item.Descripcion} · {item.Precio} créditos");
        if (items.Count == 0)
            _inventario.Items.Add("El Almacén está vacío. Cada oleada ganada deja su botín acá.");
        _inventario.EndUpdate();

        _vender.Enabled = items.Count > 0;
    }
}
