using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wiimote3Point
{
    /// <summary>
    /// A wiimote sensor bar except with 3 LEDs instead of 2.
    /// The position of each point is relative to the world origin.
    /// </summary>
    public class SensorTriangle
    {
        /// <summary>
        /// The points on the triangle relative to the world origin.
        /// </summary>
        public PointD p1, p2, p3;

        /// <summary>
        /// Construct the sensor triangle assuming it is isosceles, and lying on the world's XZ axis
        /// with the midpoint of the base at the world's origin.
        /// </summary>
        /// <param name="width">The width of the triangle.</param>
        /// <param name="height">The height of the triangle.</param>
        public SensorTriangle(double width, double height)
        {
            p1 = new PointD(-width / 2, 0, 0);
            p2 = new PointD(width / 2, 0, 0);
            p3 = new PointD(0, 0, height);
        }

        public SensorTriangle(PointD p1, PointD p2, PointD p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
    }
}
