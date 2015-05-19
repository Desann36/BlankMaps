using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maps
{
    class Region
    {
        public string Name
        {
            get;
            set;
        }
        public Color Color
        {
            get;
            set;
        }

        public override string ToString()
        {
            return String.Format("Name: {0}, Color: ({1}, {2}, {3})", this.Name, this.Color.R, this.Color.G, this.Color.B);
        }
    }
}
