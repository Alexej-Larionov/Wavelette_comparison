using System.Drawing;
using System.Windows.Forms;

public class ColoredProgressBar : ProgressBar
{
    public ColoredProgressBar()
    {
        this.SetStyle(ControlStyles.UserPaint, true);
        this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Rectangle rec = e.ClipRectangle;

        rec.Width = (int)(rec.Width * ((double)Value / Maximum)) - 4;
        if (ProgressBarRenderer.IsSupported)
            ProgressBarRenderer.DrawHorizontalBar(e.Graphics, e.ClipRectangle);
        rec.Height = rec.Height - 4;
        e.Graphics.FillRectangle(new SolidBrush(Color.Green), 2, 2, rec.Width, rec.Height);
    }
}
