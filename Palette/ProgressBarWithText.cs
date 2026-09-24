using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class ProgressBarWithText : ProgressBar
{
    public ProgressBarWithText()
    {
        SetStyle(ControlStyles.UserPaint, true);
    }

    public string CustomText { get; set; } = "";
    public Color TextColor { get; set; } = Color.Red;
    public Color BarColorStart { get; set; } = Color.DeepSkyBlue;
    public Color BarColorEnd { get; set; } = Color.DodgerBlue;
    public Color BorderColor { get; set; } = Color.Gray;
    public LinearGradientMode GradientDirection { get; set; } = LinearGradientMode.Vertical;

    protected override void OnPaint(PaintEventArgs e)
    {
        Rectangle rec = ClientRectangle;
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Nền
        g.Clear(BackColor);

        // Viền
        using (Pen borderPen = new Pen(BorderColor))
        {
            g.DrawRectangle(borderPen, 0, 0, rec.Width - 1, rec.Height - 1);
        }

        // Phần đã tải (fill gradient)
        double percent = Maximum > 0 ? (double)Value / Maximum : 0;
        int fillWidth = (int)((rec.Width - 2) * percent);

        if (fillWidth > 0)
        {
            Rectangle fillRect = new Rectangle(1, 1, fillWidth, rec.Height - 2);

            // LinearGradientBrush yêu cầu rect có Width/Height > 0
            using (LinearGradientBrush gradBrush = new LinearGradientBrush(
                fillRect,
                BarColorStart,
                BarColorEnd,
                GradientDirection))
            {
                g.FillRectangle(gradBrush, fillRect);
            }
        }

        // Chữ đè lên
        string text = string.IsNullOrEmpty(CustomText)
            ? $"{(int)(percent * 100)}%"
            : CustomText;

        using (StringFormat sf = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        })
        using (Font boldFont = new Font(Font, FontStyle.Bold))
        using (SolidBrush textBrush = new SolidBrush(TextColor))
        {
            g.DrawString(text, boldFont, textBrush, rec, sf);
        }
    }
}