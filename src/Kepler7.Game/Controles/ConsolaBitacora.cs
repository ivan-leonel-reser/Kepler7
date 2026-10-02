using Kepler7.Core.Comun;

namespace Kepler7.Game.Controles;

/// <summary>Lista de entradas de bitácora pintadas con el color de su tipo.</summary>
internal class ConsolaBitacora : ListBox
{
    public ConsolaBitacora()
    {
        DrawMode = DrawMode.OwnerDrawFixed;
        ItemHeight = 22;
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

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        using (var fondo = new SolidBrush(BackColor))
            e.Graphics.FillRectangle(fondo, e.Bounds);

        if (e.Index < 0 || e.Index >= Items.Count || Items[e.Index] is not EntradaBitacora entrada)
            return;

        var origen = $"[{entrada.Origen}] ";
        var x = e.Bounds.X + 6;
        var y = e.Bounds.Y + 3;
        TextRenderer.DrawText(e.Graphics, origen, Font, new Point(x, y), Tema.TextoTenue);
        x += TextRenderer.MeasureText(e.Graphics, origen, Font, Size.Empty, TextFormatFlags.NoPadding).Width;
        TextRenderer.DrawText(e.Graphics, entrada.Mensaje, Font, new Point(x, y), Tema.ColorDe(entrada.Tipo));
    }
}
