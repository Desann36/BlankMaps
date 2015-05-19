using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maps
{
    class RegionMap : Map
    {
        public RegionMap(string name, Bitmap map, Bitmap mask, List<Region> regions)
            :base(name, map, mask, regions)
        {

        }

        public bool? RegionClick(double nx, double ny, Color regionColor)
        {
            int x = (int) (nx * (double) this.Mask.Width);
            int y = (int) (ny * (double) this.Mask.Height);
            Color pointColorx = Mask.GetPixel(x, y);
            int pointColor = pointColorx.ToArgb();
            int col = regionColor.ToArgb();
            bool isColorOfRegion = false;

            foreach (var region in Regions)
            {
                if(region.Color.ToArgb() == pointColor)
                {
                    isColorOfRegion = true;
                    break;
                }
            }

            if(!isColorOfRegion)
            {
                return null;
            }
            else if (pointColor == col)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
