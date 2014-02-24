using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wiimote3Point
{
    /// <summary>
    /// Position (X, Y, Z) and orientation (pitch, roll, yaw).
    /// </summary>
    public class PositionOrientation
    {
        public double X, Y, Z, pitch, roll, yaw;

        public PositionOrientation(
            double X, double Y, double Z,
            double pitch, double roll, double yaw)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            this.pitch = pitch;
            this.roll = roll;
            this.yaw = yaw;
        }

        public override string ToString()
        {
            string formatStr = "{{X={0}, Y={1}, Z={2}, pitch={3}, roll={4}, yaw={5}}}";
            return string.Format(formatStr, X, Y, Z, pitch, roll, yaw);
        }


    }
}
