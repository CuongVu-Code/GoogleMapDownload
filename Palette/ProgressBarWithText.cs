using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GoogleMapDownload.Palette
{
    public class ProgressBarWithText : ProgressBar
    {
        public ProgressBarWithText()
        {
            SetStyle(ControlStyles.UserPaint, true);
        }
        public string CustomText { get; set; } = "";
        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rec = ClientRectangle;
            Graphics g = e.Graphics;
            // Vẽ nền
            g.Clear(BackColor);
            // Vẽ phần progress (dùng ProgressBarRenderer cho đúng theme Windows)
            if (ProgressBarRenderer.IsSupported)
            {
                ProgressBarRenderer.DrawHorizontalBar(g, rec);
                Rectangle clip = new Rectangle(
                    rec.X + 2,
                    rec.Y + 2,
                    (int)((rec.Width - 4) * ((double)Value / Maximum)),
                    rec.Height - 4);
                ProgressBarRenderer.DrawHorizontalChunks(g, clip);
            }
            // Vẽ chữ đè lên, không có nền
            string text = string.IsNullOrEmpty(CustomText)
                ? $"{(Maximum > 0 ? Value * 100 / Maximum : 0)}%"
                : CustomText;
            using (StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center               
            })
            using (Font boldFont = new Font(Font, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(Color.Yellow))
            {
                g.DrawString(text, boldFont, brush, rec, sf);
            }
        }
    }
}
