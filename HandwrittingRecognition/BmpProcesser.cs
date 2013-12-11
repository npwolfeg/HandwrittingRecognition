using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;

namespace HandwrittingRecognition
{
    static class BmpProcesser
    {
        public static Rectangle getBounds(Bitmap sourceBMP)
        {
            int[] result = new int[4];
            result[0] = sourceBMP.Width;
            result[1] = 0;
            result[2] = sourceBMP.Height;
            result[3] = 0;
            for (int i = 0; i < sourceBMP.Width; i++)
                for (int j = 0; j < sourceBMP.Height; j++)
                {
                    if (sourceBMP.GetPixel(i, j).R < 255)
                    {
                        if (i < result[0])
                            result[0] = i;
                        if (i > result[1])
                            result[1] = i;
                        if (j < result[2])
                            result[2] = j;
                        if (j > result[3])
                            result[3] = j;
                    }
                }
            Rectangle rect = new Rectangle(result[0], result[2], result[1] - result[0], result[3] - result[2]);
            return rect;
        }

        public static HashSet<Point> getConnectedPicture(Point e, Bitmap bmpSource)
        {
            Color c = bmpSource.GetPixel(e.X, e.Y);
            Bitmap bmp = new Bitmap(bmpSource.Width, bmpSource.Height);
            HashSet<Point> pts = new HashSet<Point>();
            HashSet<Point> result = new HashSet<Point>();
            pts.Add(e);
            result.Add(e);
            while (pts.Count > 0)
            {
                Point p = pts.First();
                int x = p.X;
                int y = p.Y;
                bmp.SetPixel(x, y, c);
                for (int i = -1; i < 2; i++)
                    for (int j = -1; j < 2; j++)
                    {
                        int a = x + i;
                        int b = y + j;
                        if (a > -1 && a < bmp.Width && b > -1 && b < bmp.Height && bmpSource.GetPixel(a, b) == c && bmp.GetPixel(a, b) != c)
                        {
                            pts.Add(new Point(a, b));
                            result.Add(new Point(a, b));
                        }
                    }
                pts.Remove(p);
            }
            return result;
        }

        public static Bitmap FillBackGround(Point e, Bitmap bmpSource, int partition,Color fillColor)
        {
            int c;
            Bitmap bmp = new Bitmap(bmpSource);
            HashSet<Point> pts = new HashSet<Point>();
            HashSet<Point> result = new HashSet<Point>();
            pts.Add(e);
            result.Add(e);
            while (pts.Count > 0)
            {
                Point p = pts.First();
                int x = p.X;
                int y = p.Y;
                c = bmpSource.GetPixel(x, y).R;
                bmp.SetPixel(x, y, fillColor);
                for (int i = -1; i < 2; i++)
                    for (int j = -1; j < 2; j++)
                    {
                        int a = x + i;
                        int b = y + j;
                        if (a > -1 && a < bmp.Width && b > -1 && b < bmp.Height && bmpSource.GetPixel(a, b).R > c - partition && bmpSource.GetPixel(a, b).R < c + partition && !result.Contains(new Point(a, b)))
                        {
                            pts.Add(new Point(a, b));
                            result.Add(new Point(a, b));
                        }
                    }
                pts.Remove(p);
            }
            return bmp;
        }

        public static List<HandwrittenDigit> getDigitsList(Bitmap bigBitmap, BackgroundWorker bw)
        {
            int progress, maxProgress;
            progress = 0;
            maxProgress = bigBitmap.Width * bigBitmap.Height;
            List<HandwrittenDigit> digits = new List<HandwrittenDigit>();
            Rectangle rect;
            HashSet<Point> pts;
            Bitmap newBigBitmap = new Bitmap(bigBitmap);
            for (int i = 0; i < newBigBitmap.Width; i++)
                for (int j = 0; j < newBigBitmap.Height; j++)
                {
                    progress++;
                    bw.ReportProgress((int)((float)progress / maxProgress * 100));
                    if (newBigBitmap.GetPixel(i, j).R < 255)
                    {
                        pts = BmpProcesser.getConnectedPicture(new Point(i, j), newBigBitmap);

                        Bitmap bmp = new Bitmap(newBigBitmap.Width, newBigBitmap.Height);
                        BmpProcesser.fillWhite(bmp);
                        foreach (Point p in pts)
                        {
                            bmp.SetPixel(p.X, p.Y, Color.FromArgb(255, 0, 0, 0));
                            newBigBitmap.SetPixel(p.X, p.Y, Color.FromArgb(255, 255, 255, 255));
                        }
                        rect = BmpProcesser.getBounds(bmp);
                        digits.Add(new HandwrittenDigit(rect, pts));

                        bmp = BmpProcesser.copyPartOfBitmap(bmp, rect);
                        bmp = BmpProcesser.normalizeBitmap(bmp, 100, 100);
                        digits.Last().bmp = new Bitmap(bmp);
                    }
                }
            return digits;
        }

        public static List<Rectangle> getRectList(Bitmap bigBitmap)
        {
            //List<HandwrittenDigit> digits = new List<HandwrittenDigit>();
            List<Rectangle> result = new List<Rectangle>();
            HashSet<Point> pts;
            Bitmap newBigBitmap = new Bitmap(bigBitmap);
            for (int i = 0; i < newBigBitmap.Width; i++)
                for (int j = 0; j < newBigBitmap.Height; j++)
                {
                    //progressBar1.Value++;
                    if (newBigBitmap.GetPixel(i, j).R < 255)
                    {
                        pts = BmpProcesser.getConnectedPicture(new Point(i, j), newBigBitmap);

                        Bitmap bmp = new Bitmap(newBigBitmap.Width, newBigBitmap.Height);
                        BmpProcesser.fillWhite(bmp);
                        foreach (Point p in pts)
                        {
                            bmp.SetPixel(p.X, p.Y, Color.FromArgb(255, 0, 0, 0));
                            newBigBitmap.SetPixel(p.X, p.Y, Color.FromArgb(255, 255, 255, 255));
                        }
                        result.Add(BmpProcesser.getBounds(bmp));
                    }
                }
            return result;
        }

        public static Bitmap renew(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                    if (bmp.GetPixel(i, j).R < 255)
                        bmp.SetPixel(i,j,Color.FromArgb(255,0,0,0));
            return bmp;
        }

        public static Bitmap renew(Bitmap bmp, int partition)
        {
            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                    if (bmp.GetPixel(i, j).R < partition)
                        bmp.SetPixel(i, j, Color.FromArgb(255, 0, 0, 0));
                    else
                        bmp.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
            return bmp;
        }

        public static Bitmap copyPartOfBitmap(Bitmap bmp, Rectangle cloneRect)
        {
            System.Drawing.Imaging.PixelFormat format = bmp.PixelFormat;
            Bitmap result;
            if (cloneRect.Width <= 0 || cloneRect.Height <= 0)
                result = (Bitmap)bmp.Clone();
            else
                result = bmp.Clone(cloneRect, format);
            return result;
        }

        public static Bitmap normalizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            return ResizeBitmap(copyPartOfBitmap(sourceBMP, getBounds(sourceBMP)), width, height); 
        }

        private static Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
                g.DrawImage(sourceBMP, 0, 0, width, height);
            return result;
        }

        public static Bitmap GrayScale(Bitmap Bmp)
        {
            int rgb;
            Color c;
            Bitmap tempBmp = new Bitmap(Bmp);

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)((.299 * c.R + .587 * c.G + .114 * c.B));
                    tempBmp.SetPixel(x, y, Color.FromArgb(255, rgb, rgb, rgb));
                }
            return tempBmp;
        }

        public static Bitmap GrayScale(Bitmap Bmp, int partition)
        {
            int rgb;
            Color c;
            Bitmap tempBmp = new Bitmap(Bmp);

            for (int y = 0; y < Bmp.Height; y++)
                for (int x = 0; x < Bmp.Width; x++)
                {
                    c = Bmp.GetPixel(x, y);
                    rgb = (int)((.299 * c.R + .587 * c.G + .114 * c.B));
                    if (rgb > partition)
                        rgb = 255;
                    else
                        rgb = 0;
                    tempBmp.SetPixel(x, y, Color.FromArgb(255, rgb, rgb, rgb));
                }
            return tempBmp;
        }

        /*public static Bitmap preprocessBitmap(Bitmap bmp)
        {
            //Bitmap result = GrayScale(bmp,100);
            Bitmap result = ResizeBitmap(bmp, 100, 100);
            result = normalizeBitmap(result, 100, 100);
            result = renew(result);
            return result;
        }*/
          
        public static bool lineIsEmpty(Bitmap bmp, int x)
        {
            bool result = true;
            if (x < bmp.Width && x > 0)
            {
                for (int i = 0; i < bmp.Height; i++)
                    if (bmp.GetPixel(x, i).A > 0)
                        result = false;
            }
            return result;
        }

        static public bool isAlpha(Bitmap bmp)
        {
            for (int i = 0; i < bmp.Width; i++)
                for (int j = 0; j < bmp.Height; j++)
                {
                    if (bmp.GetPixel(i, j).A < 255)
                        return true;
                }
            return false;
        }

        public static void fillWhite(Bitmap bmp)
        {
            SolidBrush b = new SolidBrush(Color.White);
            using (Graphics g = Graphics.FromImage(bmp))
                g.FillRectangle(b, 0, 0, bmp.Width, bmp.Height);
        }

        static public void DrawGrid(Bitmap bmp, int width, int height)
        {
             for (int i = 0; i < bmp.Width; i++)
                 using (Graphics g = Graphics.FromImage(bmp))
                    g.DrawLine(new Pen(Color.Orange, 2), i * 100, 0, i * 100, bmp.Height);
            for (int i = 0; i < bmp.Height; i++)
                using (Graphics g = Graphics.FromImage(bmp))
                    g.DrawLine(new Pen(Color.Orange, 2), 0, i * 100, bmp.Width, i * 100);
        }

        static public Bitmap smooth(Bitmap bmp, int width, int height)
        {
            Bitmap result = new Bitmap(bmp.Width, bmp.Height);

            for(int i=0;i<result.Width;i++)
                for (int j = 0; j < result.Height; j++)
                {
                    int count = 0;
                    int average = 0;
                    for(int x=-width/2;x<=width/2;x++)
                        for (int y = -height / 2; y <= height / 2; y++)
                        {
                            int a = x + i;
                            int b = y + j;
                            if (a > -1 && a < result.Width && b > -1 && b < result.Height)
                            {
                                count++;
                                average += bmp.GetPixel(a, b).R;
                            }
                        }
                    //if (count != 0)
                        average = average / count;
                    result.SetPixel(i, j, Color.FromArgb(255, average, average, average));
                }

            return result;
        }

        static public Rectangle connectRects(Rectangle rect1, Rectangle rect2)
        {
            Rectangle result = new Rectangle();
            result.X = Math.Min (rect1.X, rect2.X);
            result.Y = Math.Min (rect1.Y, rect2.Y);
            result.Width = Math.Max(rect1.Right, rect2.Right) - result.X;
            result.Height = Math.Max(rect1.Bottom, rect2.Bottom) - result.Y;
            return result;            
        }

        static public Bitmap FromAlphaToRGB(Bitmap bitmap)
        {
            Bitmap result = new Bitmap(bitmap.Width, bitmap.Height);
            for (int i = 0; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                {
                    int temp = 255 - bitmap.GetPixel(i, j).A;
                    result.SetPixel(i, j, Color.FromArgb(255, temp, temp, temp));
                }
            return result;
        }


    }
}
