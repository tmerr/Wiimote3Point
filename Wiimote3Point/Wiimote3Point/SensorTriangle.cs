using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

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
        public Vector<double> P1, P2, P3;

        /// <summary>
        /// Construct the sensor triangle assuming it is isosceles, and lying on the world's XZ axis
        /// with the midpoint of the base at the world's origin.
        /// </summary>
        /// <param name="width">The width of the triangle.</param>
        /// <param name="height">The height of the triangle.</param>
        public SensorTriangle(double width, double height)
        {
            P1 = new DenseVector(new double[] {-width / 2, 0, 0} );
            P2 = new DenseVector(new double[] {width / 2, 0, 0});
            P3 = new DenseVector(new double[] {0, height, 0});
        }

        public SensorTriangle(Vector<double> P1, Vector<double> P2, Vector<double> P3)
        {
            this.P1 = P1;
            this.P2 = P2;
            this.P3 = P3;
        }
    }
}
