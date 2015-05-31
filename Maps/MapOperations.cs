using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static double NearestPixel(Bitmap bitmap, System.Drawing.Point startingPoint, Color targetColor)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int[] bits = new int[data.Stride / 4 * data.Height];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bits, 0, bits.Length);

            int[] processed = new int[data.Stride / 4 * data.Height];
            for (int i = 0; i < processed.Length; i++)
            {
                processed[i] = -1;
            }

            int color = targetColor.ToArgb();
            int maxHalfSide = MaxHalfSide(width, height, startingPoint);
            int halfSide = 0;
            double distance = double.MaxValue;
            bool noDiscoveredPoint = true;
            Point p;
            int count = 0;

            LinkedList<System.Drawing.Point> pointsToCheck = new LinkedList<System.Drawing.Point>();
            pointsToCheck.AddLast(startingPoint);
            processed[startingPoint.X + startingPoint.Y * data.Stride / 4] = 100;

            int[] neighbourhood = { -1, 0, -1, -1, 0, -1, 1, -1, 1, 0, 1, 1, 0, 1, -1, 1 };

            while (halfSide < maxHalfSide && pointsToCheck.Any())
            {
                count++;
                p = pointsToCheck.First.Value;
                int pX = p.X;
                int pY = p.Y;
                pointsToCheck.RemoveFirst();

                for (int i = 0; i < 8; i++)
                {
                    int x = neighbourhood[i * 2];
                    int y = neighbourhood[i * 2 + 1];

                    if (pX + x >= 0 && pX + x < width && pY + y >= 0 && pY + y < height &&
                       processed[(pX + x) + (pY + y) * data.Stride / 4] != 100)
                    {
                        pointsToCheck.AddLast(new System.Drawing.Point(pX + x, pY + y));
                        processed[(pX + x) + (pY + y) * data.Stride / 4] = 100;
                    }
                }

                if (Math.Abs(pX - startingPoint.X) == halfSide + 1 || Math.Abs(pY - startingPoint.Y) == halfSide + 1)
                {
                    halfSide = halfSide + 1;
                }

                if (bits[pX + pY * data.Stride / 4] == color)
                {
                    double dist = Math.Sqrt(Math.Pow(startingPoint.X - pX, 2) + Math.Pow(startingPoint.Y - pY, 2));
                    if (dist < distance)
                    {
                        distance = dist;
                    }
                }

                if (distance < double.MaxValue && noDiscoveredPoint)
                {
                    if (halfSide * 2 < maxHalfSide)
                    {
                        maxHalfSide = halfSide*2;
                    }
                    noDiscoveredPoint = false;
                }
            }

            bitmap.UnlockBits(data);
            return distance;
        }

        private static int MaxHalfSide(int width, int height, System.Drawing.Point point)
        {
            int maxHalfSide = 0;

            if (width - point.X > point.X)
            {
                maxHalfSide = width - point.X;
            }
            else
            {
                maxHalfSide = point.X;
            }

            if (height - point.Y > point.Y && height - point.Y > maxHalfSide)
            {
                maxHalfSide = height - point.Y;
            }
            else if (point.Y >= height - point.Y && point.Y > maxHalfSide)
            {
                maxHalfSide = point.Y;
            }

            return maxHalfSide;
        }

        public static Bitmap PaintRegion(Bitmap map, Bitmap mask, Color regionColor, Color color)
        {
            BitmapData dataMap = map.LockBits(
                new Rectangle(0, 0, map.Width, map.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int[] bitsMap = new int[dataMap.Stride / 4 * dataMap.Height];
            System.Runtime.InteropServices.Marshal.Copy(dataMap.Scan0, bitsMap, 0, bitsMap.Length);

            BitmapData dataMask = mask.LockBits(
                new Rectangle(0, 0, mask.Width, mask.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int[] bitsMask = new int[dataMask.Stride / 4 * dataMask.Height];
            System.Runtime.InteropServices.Marshal.Copy(dataMask.Scan0, bitsMask, 0, bitsMask.Length);

            int rc = regionColor.ToArgb();
            int c = color.ToArgb();

            for (int i = 0; i < bitsMap.Length; i++)
            {
                if (bitsMask[i] == rc)
                {
                    bitsMap[i] = c;
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(bitsMap, 0, dataMap.Scan0, bitsMap.Length);
            map.UnlockBits(dataMap);
            mask.UnlockBits(dataMask);
            return map;
        }
    }
}
