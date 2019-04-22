using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace _64Inject
{
    public class BootImage : IDisposable
    {
        private bool disposed = false;

        private Bitmap _frame;
        private Bitmap _titleScreen;

        public Bitmap Frame
        {
            set
            {
                if (_frame != null)
                    _frame.Dispose();
                _frame = value;
            }
            get { return _frame; }
        }
        public Bitmap TitleScreen
        {
            set
            {
                if (_titleScreen != null)
                    _titleScreen.Dispose();
                _titleScreen = value;
            }
            get { return _titleScreen; }
        }
        public string NameLine1;
        public string NameLine2;
        public int Released;
        public int Players;
        public bool Longname;
        public bool IsDefault;

        public BootImage()
        {
            _frame = null;
            _titleScreen = null;
            NameLine1 = null;
            NameLine2 = null;
            Released = 0;
            Players = 0;
            Longname = false;
            IsDefault = true;
        }

        ~BootImage()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (Frame != null)
                        Frame.Dispose();
                    if (TitleScreen != null)
                        TitleScreen.Dispose();
                }
                disposed = true;
            }
        }

        public Bitmap Create()
        {
            Bitmap img = new Bitmap(1280, 720);
            Graphics g = Graphics.FromImage(img);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            GraphicsPath nl1 = new GraphicsPath();
            GraphicsPath nl2 = new GraphicsPath();
            GraphicsPath r = new GraphicsPath();
            GraphicsPath p = new GraphicsPath();

            Font font = new Font("Trebuchet MS", 10.0F, FontStyle.Regular, GraphicsUnit.Point);
            Rectangle rectangleNL1 = Longname ? new Rectangle(578, 313, 640, 50) : new Rectangle(578, 340, 640, 50);
            Rectangle rectangleNL2 = new Rectangle(578, 368, 640, 50);
            Rectangle rectangleR = new Rectangle(586, 450, 250, 40);
            Rectangle rectangleP = new Rectangle(586, 496, 200, 40);
            Rectangle rectangleTS = new Rectangle(131, 249, 400, 300);
            SolidBrush brush = new SolidBrush(Color.FromArgb(32, 32, 32));
            Pen outlineBold = new Pen(Color.FromArgb(222, 222, 222), 5.0F);
            Pen shadowBold = new Pen(Color.FromArgb(190, 190, 190), 7.0F);
            Pen outline = new Pen(Color.FromArgb(222, 222, 222), 4.0F);
            Pen shadow = new Pen(Color.FromArgb(190, 190, 190), 6.0F);
            StringFormat format = new StringFormat();

            g.Clear(Color.White);

            if (Frame == null)
            {
                GraphicsPath vc = new GraphicsPath();
                GraphicsPath n = new GraphicsPath();
                GraphicsPath sf = new GraphicsPath();
                GraphicsPath sfi = new GraphicsPath();

                Font fontVC = new Font("Arial", 10.0F, FontStyle.Regular, GraphicsUnit.Point);
                Font fontN64 = new Font("Arial Black", 10.0F, FontStyle.Regular, GraphicsUnit.Point);
                Rectangle rectangleVC = new Rectangle(60, 105, 400, 50);
                Rectangle rectangleN = new Rectangle(575, 239, 350, 60);
                Rectangle rectangle64 = new Rectangle(902, 234, 72, 60);
                Rectangle rectangle64I = new Rectangle(1075, 645, 150, 40);
                SolidBrush brushVC = new SolidBrush(Color.FromArgb(147, 149, 152));
                SolidBrush brushN = new SolidBrush(Color.FromArgb(42, 65, 152));
                SolidBrush brush64 = new SolidBrush(Color.FromArgb(230, 0, 18));
                SolidBrush brush64I = new SolidBrush(Color.FromArgb(213, 213, 213));
                Pen outline64I = new Pen(Color.FromArgb(150, 150, 150), 2.0F);

                g.Clear(Color.FromArgb(226, 226, 226));
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 200)), 61, 192, 1162, 421);
                g.FillRectangle(new SolidBrush(Color.FromArgb(226, 226, 226)), 66, 197, 1152, 411);

                vc.AddString("Virtual Console", fontVC.FontFamily,
                    (int)(FontStyle.Bold | FontStyle.Italic),
                    g.DpiY * 37.4F / 72.0F, rectangleVC, format);
                g.FillPath(brushVC, vc);

                n.AddString("NINTENDO", fontN64.FontFamily,
                    (int)(FontStyle.Bold),
                    g.DpiY * 40.0F / 72.0F, rectangleN, format);
                g.FillPath(brushN, n);

                sf.AddString("64", fontN64.FontFamily,
                    (int)(FontStyle.Bold),
                    g.DpiY * 31.0F / 72.0F, rectangle64, format);
                g.FillPath(brush64, sf);

                sfi.AddString("64Inject", font.FontFamily,
                    (int)(FontStyle.Regular),
                    g.DpiY * 26.0F / 72.0F, rectangle64I, format);
                g.DrawPath(outline64I, sfi);
                g.FillPath(brush64I, sfi);
            }

            if (TitleScreen != null)
                g.DrawImage(TitleScreen, rectangleTS);
            else
                g.FillRectangle(new SolidBrush(Color.Black), rectangleTS);

            if (Frame != null)
                g.DrawImage(Frame, new Rectangle(0, 0, 1280, 720));

            if (NameLine1 != null && NameLine2 != null)
            { 
                if (Longname)
                {
                    nl1.AddString(NameLine1, font.FontFamily,
                        (int)(FontStyle.Bold),
                        g.DpiY * 37.0F / 72.0F, rectangleNL1, format);
                    g.DrawPath(shadowBold, nl1);
                    g.DrawPath(outlineBold, nl1);
                    g.FillPath(brush, nl1);
                    nl2.AddString(NameLine2, font.FontFamily,
                        (int)(FontStyle.Bold),
                        g.DpiY * 37.0F / 72.0F, rectangleNL2, format);
                    g.DrawPath(shadowBold, nl2);
                    g.DrawPath(outlineBold, nl2);
                    g.FillPath(brush, nl2);
                }
                else
                {
                    nl1.AddString(NameLine1, font.FontFamily,
                        (int)(FontStyle.Bold),
                        g.DpiY * 37.0F / 72.0F, rectangleNL1, format);
                    g.DrawPath(shadowBold, nl1);
                    g.DrawPath(outlineBold, nl1);
                    g.FillPath(brush, nl1);
                }
            }

            if (Released > 1995)
            {
                r.AddString("Released: " + Released.ToString(), font.FontFamily,
                    (int)(FontStyle.Regular),
                    g.DpiY * 25.0F / 72.0F, rectangleR, format);
                g.DrawPath(shadow, r);
                g.DrawPath(outline, r);
                g.FillPath(brush, r);
            }

            if (Players > 0)
            {
                string pStr;

                if (Players == 4)
                    pStr = "1-4";
                else if (Players == 3)
                    pStr = "1-3";
                else if (Players == 2)
                    pStr = "1-2";
                else
                    pStr = "1";

                p.AddString("Players: " + pStr, font.FontFamily,
                    (int)(FontStyle.Regular),
                    g.DpiY * 25.0F / 72.0F, rectangleP, format);
                g.DrawPath(shadow, p);
                g.DrawPath(outline, p);
                g.FillPath(brush, p);
            }

            return img;
        }
    }
}
