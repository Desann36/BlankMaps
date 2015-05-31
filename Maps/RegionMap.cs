using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maps
{
    public class RegionMap : Map
    {
        public RegionMap(string name, Bitmap map, Bitmap mask, List<Region> regions)
            :base(name, map, mask, regions)
        {

        }

        public bool? RegionClick(int x, int y, Color regionColor)
        {
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
