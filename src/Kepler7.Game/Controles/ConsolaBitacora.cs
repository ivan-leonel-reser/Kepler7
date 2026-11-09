using Kepler7.BE;

namespace Kepler7.Game.Controles;

/// <summary>Lista de entradas de bitácora pintadas con el color de su tipo. Las largas ocupan más de un renglón.</summary>
internal class ConsolaBitacora : ListBox
{
    private const int Margen = 6;
    private const int AltoMinimo = 22;
    private const TextFormatFlags Formato = TextFormatFlags.WordBreak | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix;

    private int _anchoMedido;

    public ConsolaBitacora()
    {
        DrawMode = DrawMode.OwnerDrawVariable;
        IntegralHeight = false;
        SelectionMode = SelectionMode.None;
        BorderStyle = BorderStyle.None;
        BackColor = Color.FromArgb(5, 9, 16);
        ForeColor = Tema.Texto;
        Font = Tema.Consola;
    }

    public void Agregar(EntradaBitacora entrada)
    {
        Items.Add(entrada);
        TopIndex = Items.Count - 1;
    }

    public void AgregarTodas(IEnumerable<EntradaBitacora> entradas)
    {
        BeginUpdate();
        foreach (var entrada in entradas)
            Items.Add(entrada);
        EndUpdate();
        if (Items.Count > 0)
            TopIndex = Items.Count - 1;
    }

    // Lo que se carga antes de que la lista tenga tamaño no queda desplazado: se acomoda acá.
    // Si cambió el ancho, los renglones se vuelven a medir porque el texto se parte en otro lugar.
    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (IsHandleCreated && Width != _anchoMedido && Items.Count > 0)
            RefreshItems();
        _anchoMedido = Width;
        if (Items.Count > 0)
            TopIndex = Items.Count - 1;
    }

    protected override void OnMeasureItem(MeasureItemEventArgs e)
    {
        base.OnMeasureItem(e);
        if (e.Index < 0 || e.Index >= Items.Count || Items[e.Index] is not EntradaBitacora entrada)
            return;

        var origen = AnchoDe($"[{entrada.Origen}] ");
        var alto = TextRenderer.MeasureText(entrada.Mensaje, Font, new Size(AnchoMensaje(origen), int.MaxValue), Formato).Height;
        e.ItemHeight = Math.Max(AltoMinimo, alto + 6);
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        using (var fondo = new SolidBrush(BackColor))
            e.Graphics.FillRectangle(fondo, e.Bounds);

        if (e.Index < 0 || e.Index >= Items.Count || Items[e.Index] is not EntradaBitacora entrada)
            return;

        var origen = $"[{entrada.Origen}] ";
        var x = e.Bounds.X + Margen;
        var y = e.Bounds.Y + 3;
        TextRenderer.DrawText(e.Graphics, origen, Font, new Point(x, y), Tema.TextoTenue, Formato);
        var ancho = AnchoDe(origen);
        TextRenderer.DrawText(e.Graphics, entrada.Mensaje, Font,
            new Rectangle(x + ancho, y, AnchoMensaje(ancho), e.Bounds.Height - 3), Tema.ColorDe(entrada.Tipo), Formato);
    }

    private int AnchoDe(string texto) => TextRenderer.MeasureText(texto, Font, Size.Empty, TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;

    // Se deja siempre el lugar de la barra de desplazamiento, así el texto no cambia de renglón cuando aparece.
    private int AnchoMensaje(int anchoOrigen) =>
        Math.Max(120, Width - SystemInformation.VerticalScrollBarWidth - 2 * Margen - anchoOrigen);
}
