using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maps 
{
    public class DistanceMap : Map
    {
        public Double HorizontalLength
        {
            get;
            set;
        }

        public DistanceMap(string name, Bitmap map, Bitmap mask, List<Region> regions, double horizontalLength)
            :base(name, map, mask, regions)
        {
            this.HorizontalLength = horizontalLength;
        }
    }
}
