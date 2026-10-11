namespace TrackSpender;

/// <summary>Minimal bar chart: one bar per day (month view) or per month (year view).</summary>
public sealed class BarChart : Control
{
    private double[] _values = Array.Empty<double>();
    private string[] _labels = Array.Empty<string>();

    public BarChart()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    public void SetData(double[] values, string[] labels)
    {
        _values = values;
        _labels = labels;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.Clear(BackColor);
        if (_values.Length == 0) return;

        const int labelHeight = 18, top = 8;
        var area = new RectangleF(8, top, Width - 16, Height - top - labelHeight);
        float slot = area.Width / _values.Length;
        double max = Math.Max(_values.Max(), 0.01);

        using var barBrush = new SolidBrush(Color.FromArgb(0x1F, 0x6F, 0x5C));
        using var textBrush = new SolidBrush(ForeColor);
        using var axis = new Pen(Color.Silver);
        using var center = new StringFormat { Alignment = StringAlignment.Center };

        for (int i = 0; i < _values.Length; i++)
        {
            float h = (float)(_values[i] / max * area.Height);
            if (h > 0)
                g.FillRectangle(barBrush, area.Left + i * slot + slot * 0.15f, area.Bottom - h, slot * 0.7f, h);
            if (slot >= 22 || i % 2 == 0)
                g.DrawString(_labels[i], Font, textBrush, area.Left + i * slot + slot / 2, area.Bottom + 2, center);
        }
        g.DrawLine(axis, area.Left, area.Bottom, area.Right, area.Bottom);
    }
}
