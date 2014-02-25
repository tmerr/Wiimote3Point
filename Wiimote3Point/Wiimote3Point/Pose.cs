using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wiimote3Point
{
    /// <summary>
    /// Position (X, Y, Z) and orientation (pitch, roll, yaw).
    /// </summary>
    public class Pose
    {
        public double X, Y, Z;
        public double[,] Orientation = new double[3,3];

        public Pose(
            double X, double Y, double Z,
            double[,] orientation)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.Orientation = orientation;
        }

        public override string ToString()
        {
            string formatStr = "{{X={0}, Y={1}, Z={2}, pitch={3}, roll={4}, yaw={5}}}";
            return string.Format(formatStr, X, Y, Z);
        }


    }
}
