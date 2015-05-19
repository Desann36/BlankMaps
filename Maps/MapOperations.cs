using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Maps
{
    static class MapOperations
    {
        public static Bitmap FloodFill(Bitmap bitmap, Point p, Color color)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int[] bits = new int[data.Stride / 4 * data.Height];

            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bits, 0, bits.Length);

            int replacementColor = color.ToArgb();
            int targetColor = Color.White.ToArgb();
            int pointColor = bits[p.X + p.Y * data.Stride / 4];
            LinkedList<Point> pointsToCheck = new LinkedList<Point>();

            if (pointColor == targetColor)
            {
                pointsToCheck.AddLast(p);
                while (pointsToCheck.Any())
                {
                    Point point = pointsToCheck.First.Value;
                    pointsToCheck.RemoveFirst();

                    int col = bits[point.X + point.Y * data.Stride / 4];

                    if (col == targetColor)
                    {
                        bits[point.X + point.Y * data.Stride / 4] = replacementColor;
                        if (point.X > 0)
                        {
                            if (bits[(point.X - 1) + point.Y * data.Stride / 4] == targetColor)
                            {
                                pointsToCheck.AddLast(new Point(point.X - 1, point.Y));
                            }
                        }
                        if (point.X < width - 1)
                        {
                            if (bits[(point.X + 1) + point.Y * data.Stride / 4] == targetColor)
                            {
                                pointsToCheck.AddLast(new Point(point.X + 1, point.Y));
                            }
                        }
                        if (point.Y > 0)
                        {
                            if (bits[point.X + (point.Y - 1) * data.Stride / 4] == targetColor)
                            {
                                pointsToCheck.AddLast(new Point(point.X, point.Y - 1));
                            }
                        }
                        if (point.Y < height - 1)
                        {
                            if (bits[point.X + (point.Y + 1) * data.Stride / 4] == targetColor)
                            {
                                pointsToCheck.AddLast(new Point(point.X, point.Y + 1));
                            }
                        }
                    }
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(bits, 0, data.Scan0, bits.Length);
            bitmap.UnlockBits(data);
            return bitmap;
        }
    }
}
