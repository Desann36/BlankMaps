using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maps
{
    public class Map
    {
        public Map(string name, Bitmap map, Bitmap mask, List<Region> regions)
        {
            this.Name = name;
            this.MapToDraw = map;
            this.Mask = mask;
            this.Regions = regions;
        }

        public String Name
        {
            get;
            set;
        }

        public Bitmap MapToDraw
        {
            get;
            set;
        }

        public Bitmap Mask
        {
            get;
            set;
        }

        public List<Region> Regions
        {
            get;
            private set;
        }

        public Point pointInRegion(Color regionColor)
        {
            int col = regionColor.ToArgb();

            BitmapData data = Mask.LockBits(
                new Rectangle(0, 0, Mask.Width, Mask.Height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb);

            int[] bits = new int[data.Stride / 4 * data.Height];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, bits, 0, bits.Length);

            for (int y = 0; y < Mask.Height; y++)
            {
                for (int x = 0; x < Mask.Width; x++)
                {
                    if (col == bits[x + y * data.Stride / 4])
                    {
                        Mask.UnlockBits(data);
                        return new Point(x, y);
                    }
                }
            }

            Mask.UnlockBits(data);
            return new Point(-1, -1);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
