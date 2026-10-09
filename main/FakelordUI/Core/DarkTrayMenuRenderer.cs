using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace FakelordUI.Core
{
    /// <summary>
    /// FakeLord Dark & Cyberpunk temalı modern Windows Forms ContextMenuStrip renk tablosu.
    /// </summary>
    public class DarkTrayColorTable : ProfessionalColorTable
    {
        // Ana menü arka planı (#0E131F - derin kurşuni/gece tonu)
        public override Color ToolStripDropDownBackground => Color.FromArgb(14, 19, 31);
        public override Color MenuStripGradientBegin => Color.FromArgb(14, 19, 31);
        public override Color MenuStripGradientEnd => Color.FromArgb(14, 19, 31);

        // Menü kenarlığı (#232D42 - zarif modern border)
        public override Color MenuBorder => Color.FromArgb(35, 45, 66);
        public override Color MenuItemBorder => Color.Transparent;

        // Hover & Seçim durumları (#1E283C - hover pill)
        public override Color MenuItemSelected => Color.FromArgb(30, 40, 60);
        public override Color MenuItemSelectedGradientBegin => Color.FromArgb(30, 40, 60);
        public override Color MenuItemSelectedGradientEnd => Color.FromArgb(30, 40, 60);
        public override Color MenuItemPressedGradientBegin => Color.FromArgb(38, 50, 75);
        public override Color MenuItemPressedGradientEnd => Color.FromArgb(38, 50, 75);

        // Sol ikon şeridi (Gutter kaldırıldığı için menüyle aynı renk)
        public override Color ImageMarginGradientBegin => Color.FromArgb(14, 19, 31);
        public override Color ImageMarginGradientMiddle => Color.FromArgb(14, 19, 31);
        public override Color ImageMarginGradientEnd => Color.FromArgb(14, 19, 31);
        public override Color ImageMarginRevealedGradientBegin => Color.FromArgb(14, 19, 31);
        public override Color ImageMarginRevealedGradientMiddle => Color.FromArgb(14, 19, 31);
        public override Color ImageMarginRevealedGradientEnd => Color.FromArgb(14, 19, 31);

        // Ayırıcı çizgi rengi (#242E43)
        public override Color SeparatorDark => Color.FromArgb(36, 46, 67);
        public override Color SeparatorLight => Color.Transparent;
    }

    /// <summary>
    /// FakeLord Sistem Tepsisi için özel geliştirilmiş, anti-aliased, yuvarlatılmış köşeli ve modern karanlık tema renderer'ı.
    /// </summary>
    public class DarkTrayMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkTrayMenuRenderer() : base(new DarkTrayColorTable())
        {
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(Color.FromArgb(14, 19, 31));
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using var pen = new Pen(Color.FromArgb(35, 45, 66), 1);
            var rect = new Rectangle(0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            e.Graphics.DrawRectangle(pen, rect);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            if (!e.Item.Enabled) return;

            if (e.Item.Selected || e.Item.Pressed)
            {
                var rect = new Rectangle(4, 2, e.Item.Width - 8, e.Item.Height - 4);
                using var brush = new SolidBrush(e.Item.Pressed ? Color.FromArgb(38, 50, 75) : Color.FromArgb(30, 40, 60));

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Yuvarlatılmış modern seçim hapı (Hover pill)
                using var path = CreateRoundedRectanglePath(rect, 4);
                e.Graphics.FillPath(brush, path);

                // Sol tarafta Discord mavisi / neon çizgi aksanı
                Color accentColor = Color.FromArgb(88, 101, 242); // #5865F2 Discord Blurple
                if (e.Item.Tag is string tag)
                {
                    if (tag == "stop_btn") accentColor = Color.FromArgb(237, 66, 69); // Kırmızı
                    else if (tag == "status_active") accentColor = Color.FromArgb(87, 242, 135); // Yeşil
                }

                using var accentBrush = new SolidBrush(accentColor);
                var accentRect = new Rectangle(4, 5, 3, e.Item.Height - 10);
                using var accentPath = CreateRoundedRectanglePath(accentRect, 1);
                e.Graphics.FillPath(accentBrush, accentPath);
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Öğe etiketine göre özel metin renkleri
            Color textColor;
            if (!e.Item.Enabled)
            {
                textColor = Color.FromArgb(115, 126, 142); // Devre dışı / alt metin tonu
            }
            else if (e.Item.Tag is string tag)
            {
                if (tag == "status_active") textColor = Color.FromArgb(87, 242, 135); // #57F287 Canlı Yeşil
                else if (tag == "stop_btn") textColor = Color.FromArgb(248, 113, 113); // #F87171 Vurgulu Kırmızı
                else if (tag == "header_brand") textColor = Color.FromArgb(129, 140, 248); // #818CF8 Discord İndigo
                else textColor = Color.FromArgb(235, 240, 248); // #EBF0F8
            }
            else
            {
                textColor = Color.FromArgb(235, 240, 248);
            }

            e.TextColor = textColor;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            int y = e.Item.Height / 2;
            using var pen = new Pen(Color.FromArgb(32, 41, 60), 1);
            e.Graphics.DrawLine(pen, 10, y, e.Item.Width - 10, y);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = e.Item?.Selected == true ? Color.FromArgb(129, 140, 248) : Color.FromArgb(148, 163, 184);
            base.OnRenderArrow(e);
        }

        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            if (d > rect.Width) d = rect.Width;
            if (d > rect.Height) d = rect.Height;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
