using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Wiimote3Point
{
    /// <summary>
    /// Solves the Perspective 3 Point problem using the algorithm described by
    /// 
    /// Kneip, L.; Scaramuzza, D.; Siegwart, R., 
    /// "A novel parametrization of the perspective-three-point problem for a direct computation of absolute camera position and orientation,"
    /// Computer Vision and Pattern Recognition (CVPR), 2011 IEEE Conference on , vol., no., pp.2969,2976, 20-25 June 2011
    ///
    /// The documentation is intended to be used alongside the paper. The naming is the same.
    /// </summary>
    static class P3PMath
    {
        /// <summary>
        /// Solve the perspective 3 point problem.
        /// </summary>
        /// <param name="p1">Position of IR point 1</param>
        /// <param name="p2">Position of IR point 2</param>
        /// <param name="p3">Position of IR point 3</param>
        /// <param name="f1">Unit vector from camera origin toward pixel 1</param>
        /// <param name="f2">Unit vector from camera origin toward pixel 2</param>
        /// <param name="f3">Unit vector from camera origin toward pixel 3</param>
        /// <returns></returns>
        public static List<PositionOrientation> Solve (Vector<double> P1, Vector<double> P2, Vector<double> P3,
                                                       Vector<double> F1, Vector<double> F2, Vector<double> F3)
        {

            // Make transformation matrices.
            // Then transform f3 into world frame t, and p3 into world frame n.
            var T = TransT(F1, F2, F3);
            var N = TransN(P1, P2, P3);
            var f3t = T.Multiply(F3);
            var p3n = N.Multiply(P3 - P1);

            // Find b, the cotan of the angle between f1 and f2.
            double cosbeta = Math.Cos(F1.DotProduct(F2));
            int sign = Math.Sign(cosbeta);
            double b = sign * Math.Sqrt(1 / (1 - Math.Pow(cosbeta, 2)));

            // Get the distance between P1 and P2
            double d12 = (P2 - P1).ToColumnMatrix().L2Norm();

            // Use f3 in world frame t's x vs z slope and y vs z slope.
            var phi1 = f3t[0]/f3t[2];
            var phi2 = f3t[1]/f3t[2];

            // Find polynomial's roots (cos theta).
            var coeffs = ComputeCoefficients(phi1, phi2, b, d12, p3n);
            complex[] complexCoeffs = new complex[5];
            for (int i = 4; i > -1; i--)
            {
                complexCoeffs[i].real = coeffs[i];
                complexCoeffs[i].imag = 0;
            }
            complex[] complexResults = new complex[4];
            SolvePoly(4, complexCoeffs, complexResults);

            // Backsubstitute each root (cos theta) to eventually get the camera center and orientation.
            List<PositionOrientation> positionOrientations = new List<PositionOrientation>();
            foreach (complex result in complexResults)
            {
                // Alpha is the angle between P1P2 and P1C. See (9).
                double numerator = (phi1/phi2)*P3[0] + (result.real * P3[1]) - (d12 * b);
                double denominator = ((phi1/phi2) * result.real * P3[1]) - P3[0] + d12;
                double cotAlpha = numerator/denominator;
                double cosTheta = result.real;
                double sinTheta = Math.Sqrt(1 - result.real * result.real);
                double sinAlpha = Math.Sqrt(1 / (cotAlpha * cotAlpha + 1));
                double cosAlpha = Math.Sqrt(1 - sinAlpha * sinAlpha);

                // Find the camera center in world frame n. See (5).
                DenseVector Cn = new DenseVector(3);
                Cn[0] = d12 * cosAlpha * (sinAlpha * b + cosAlpha);
                Cn[1] = d12 * sinAlpha * cosAlpha * (sinAlpha * b + cosAlpha);
                Cn[2] = d12 * sinAlpha * sinTheta * (sinAlpha * b + cosAlpha);

                // Using transformation matrix Q from n to t we can find the actual camera center. (6)
                DenseMatrix Q = new DenseMatrix(3, 3);
                Q.SetRow(0, new double[] {-cosAlpha, sinAlpha*cosTheta, -sinAlpha*sinTheta});
                Q.SetRow(1, new double[] {sinAlpha, -cosAlpha*cosTheta, -cosAlpha*sinTheta});
                Q.SetRow(2, new double[] { 0, -sinTheta, cosTheta });

                // Find absolute camera center C and orientation R (12) and (13)
                var C = P1 + N.Transpose() * Cn;
                var R = N.Transpose() * Q.Transpose() * T;

                PositionOrientation p = new PositionOrientation(C[0], C[1], C[2], 0, 0, 0);
                positionOrientations.Add(p);
            }

            return positionOrientations;
        }

        /// <summary>
        /// Build transformation matrix from the original camera frame v to intermediate camera frame t.
        /// </summary>
        /// <param name="f1">Unit vector f1</param>
        /// <param name="f2">Unit vector f2</param>
        /// <param name="f3">Unit vector f3</param>
        /// <returns>A 3x3 transformation matrix</returns>
        private static Matrix<double> TransT(Vector<double> f1, Vector<double> f2, Vector<double> f3)
        {
            var M = new DenseMatrix(3, 3);

            var tx = f1;
            M.SetRow(0, f1);

            var tmp = f1.OuterProduct(f2);
            var tz = tmp.Divide(tmp.L2Norm()).CreateVector(3);
            M.SetRow(2, tz);

            var ty = tz.OuterProduct(f1).CreateVector(3);
            M.SetRow(1, ty);

            return M;
        }

        /// <summary>
        /// Build transformation matrix that can turn world points into world frame n via
        /// the formula N * (Pi - P1), where N is the result of this function.
        /// </summary>
        /// <param name="P1">World point 1</param>
        /// <param name="P2">World point 2</param>
        /// <param name="P3">World point 3</param>
        /// <returns>The transformation matrix N</returns>
        private static Matrix<double> TransN(Vector<double> P1, Vector<double> P2, Vector<double> P3)
        {
            var M = new DenseMatrix(3, 3);
            var P12 = P2 - P1;
            var P13 = P3 - P1;

            var nx = P12 / P12.ToColumnMatrix().L2Norm();
            M.SetRow(0, nx);

            var tmp = nx.OuterProduct(P13);
            var nz = tmp.Divide(tmp.L2Norm()).CreateVector(3);
            M.SetRow(2, nz);

            var ny = nz.OuterProduct(nx).CreateVector(3);
            M.SetRow(1, ny);

            return M;
        }

        /// <summary>
        /// Refer to (11).
        /// Compute the coefficients a[i] for the polynomial:
        /// a[4]*(cos theta)^4 + a[3]*(cos theta)^3 + a[2]*(cos theta)^2 + a[1]*(cos theta) + a[0] = 0
        /// Where theta is the angle between
        /// a.) The plane formed by infra red points P1 and P2, and camera center C
        /// b.) The world frame n.
        /// </summary>
        /// <param name="phi1">The slope x vs z of f3</param>
        /// <param name="phi2">The slope y vs z of f3</param>
        /// <param name="b">The tangent of the angle between points P1 and P2 from camera center C</param>
        /// <param name="d12">The distance between P1 and P2</param>
        /// <param name="p3n">The vector of P3 in world frame n</param>
        /// <returns>A list of the five coefficients.</returns>
        private static List<double> ComputeCoefficients(double phi1, double phi2, double b, double d12, Vector<double> p3n)
        {
            double p1 = p3n[0];
            double p2 = p3n[1];
            
            double tmp4 = Math.Pow(p2, 4);
            var a4 = - tmp4*Math.Pow(phi2, 2) - tmp4*Math.Pow(phi1, 2) - tmp4;

            double tmp3 = Math.Pow(p2, 3);
            var a3 =
                + (2 * tmp3 * d12 * b)
                + (2 * tmp3 * Math.Pow(phi2, 2) * d12 * b)
                - (2 * tmp3 * phi1 * phi2 * d12)
                ;

            double tmp2 = Math.Pow(p2, 2);
            var a2 =
                - (phi2 * phi2 * tmp2 * p1 * p1)
                - (phi2 * phi2 * tmp2 * d12 * d12 * b * b)
                - (phi2 * phi2 * tmp2 * d12 * d12)
                + (phi2 * phi2 * tmp4)

                + (phi1 * phi1 * tmp4)
                + (2 * p1 * tmp2 * d12)
                + (2 * phi1 * phi2 * p1 * tmp2 * d12 * b)
                
                - (phi1 * phi1 * phi2 * phi2 * tmp2)
                + (2 * phi2 * phi2 * p1 * tmp2 * d12)
                - (tmp2 * d12 * d12 * b * b)
                - (2 * p1 * p1 * tmp2)
                ;

            var a1 =
                + (2 * p1 * p1 * p2 * d12 * b)
                + (2 * phi1 * phi2 * tmp3 * d12)
                - (2 * phi2 * phi2 * tmp3 * d12 * b)
                - (2 * p1 * p2 * d12 * d12 * b)
                ;

            var a0 =
                - (2 * phi1 * phi2 * p1 * tmp2 * d12 * b)
                + (phi2 * phi2 * tmp2 * d12 * d12)
                + (2 * Math.Pow(p1, 3) * d12)

                - (p1 * p1 * d12 * d12)
                + (phi2 * phi2 * p1 * p1 * tmp2)
                - (Math.Pow(p1, 4))
                - (2 * phi2 * phi2 * p1 * tmp2 * d12)

                + (phi1 * phi1 * p1 * p1 * tmp2)
                + (phi2 * phi2 * tmp2 * d12 * d12 * b * b)
                ;

            return new List<double>(new double[] {a4, a3, a2, a1, a0});
        }

        [StructLayout(LayoutKind.Sequential)]
        struct complex
        {
            public double real;
            public double imag;
        }
   
        /// <summary>
        /// Get the complex roots of a polynomial up to the 4th degree. The 4th degree uses Ferrari's method.
        /// </summary>
        /// <param name="degree">The highest exponent in the polynomial.</param>
        /// <param name="poly">The array of polynomial's coefficients, from the highest degree to lowest.</param>
        /// <param name="results">The array to output the complex roots to.</param>
        /// <returns>The number of elements in the results array.</returns>
        [DllImport("quartic.dll", EntryPoint="solve_poly", CallingConvention = CallingConvention.Cdecl)]
        private static extern int SolvePoly(int degree, complex[] poly, [In, Out] complex[] results);
    }
}
