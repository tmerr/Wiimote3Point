using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wiimote3Point
{
    struct PointD
    {
        public double X, Y, Z;

        public PointD(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
    }
}
