﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maps
{
    public class Region
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
            return Name;
        }
    }
}
