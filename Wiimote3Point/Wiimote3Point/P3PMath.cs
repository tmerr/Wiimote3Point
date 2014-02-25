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
        public static List<Pose> Solve (Vector<double> P1, Vector<double> P2, Vector<double> P3,
                                                       Vector<double> F1, Vector<double> F2, Vector<double> F3)
        {
            // Make transformation matrices.
            // Then transform f3 into world frame t, and p3 into world frame n.
            Matrix<double> T = TransT(F1, F2, F3);
            Matrix<double> N = TransN(P1, P2, P3);
            Vector<double> f3t = T.Multiply(F3);
            Vector<double> p3n = N.Multiply(P3 - P1);
            
            // Find b, the cotan of the angle between f1 and f2.
            double cosbeta = F1.DotProduct(F2);
            int sign = Math.Sign(cosbeta);
            double b = sign * Math.Sqrt((1 / (1 - cosbeta*cosbeta)) - 1);
            
            // Get the distance between P1 and P2
            double d12 = (P2 - P1).Norm(2);

            // Use f3 in world frame t's x vs z slope and y vs z slope.
            var phi1 = f3t[0]/f3t[2];
            var phi2 = f3t[1]/f3t[2];

            // Find polynomial's roots (cos theta).
            List<double> coeffs = ComputeCoefficients(phi1, phi2, b, d12, p3n);
            complex[] complexCoeffs = new complex[5];
            for (int i = 0; i < 5; i++)
            {
                complexCoeffs[i].real = coeffs[i];
                complexCoeffs[i].imag = 0;
            }
            complex[] complexResults = new complex[4];
            SolvePoly(4, complexCoeffs, complexResults);

            // Backsubstitute each root (cos theta) to eventually get the camera center and orientation.
            List<Pose> positionOrientations = new List<Pose>();
            foreach (complex result in complexResults)
            {
                //if (result.real < result.imag) {continue;}

                // Alpha is the angle between P1P2 and P1C. See (9).
                double cosTheta = result.real;
                double numerator = (phi1/phi2)*p3n[0] + (cosTheta * p3n[1]) - (d12 * b);
                double denominator = ((phi1/phi2) * cosTheta * p3n[1]) - p3n[0] + d12;
                double cotAlpha = numerator/denominator;
                double sinTheta = Math.Sqrt(1 - cosTheta * cosTheta);
                double sinAlpha = Math.Sqrt(1 / (cotAlpha * cotAlpha + 1));
                double cosAlpha = Math.Sign(cotAlpha) * Math.Sqrt(1 - sinAlpha * sinAlpha);

                // Find the camera center in world frame n. See (5).
                DenseVector Cn = new DenseVector(3);
                Cn[0] = d12 * cosAlpha * (sinAlpha * b + cosAlpha);
                Cn[1] = d12 * sinAlpha * cosTheta * (sinAlpha * b + cosAlpha);
                Cn[2] = d12 * sinAlpha * sinTheta * (sinAlpha * b + cosAlpha);

                // Using transformation matrix Q from n to t we can find the actual camera center. (6)
                DenseMatrix Q = new DenseMatrix(3, 3);
                Q.SetRow(0, new double[] {-cosAlpha, -sinAlpha*cosTheta, -sinAlpha*sinTheta});
                Q.SetRow(1, new double[] {sinAlpha, -cosAlpha*cosTheta, -cosAlpha*sinTheta});
                Q.SetRow(2, new double[] { 0, -sinTheta, cosTheta });

                // Find absolute camera center C and orientation R (12) and (13)
                Vector<double> C = P1 + (N.Transpose() * Cn);
                Matrix<double> R = N.Transpose() * Q.Transpose() * T;
                Pose p = new Pose(C[0], C[1], C[2], R.ToArray());
                if (!double.IsNaN(p.X))
                {
                    positionOrientations.Add(p);
                }
            }

            return positionOrientations;
        }

        private static Vector<double> Cross(Vector<double> left, Vector<double> right)
        {
            if (left.Count != 3 || right.Count != 3)
            {
                string msg = "Vectors must have a length of 3.";
                throw new Exception(msg);
            }
            Vector<double> result = new DenseVector(3);
            result[0] = left[1] * right[2] - left[2] * right[1];
            result[1] = left[2] * right[0] - left[0] * right[2];
            result[2] = left[0] * right[1] - left[1] * right[0];
            return result;
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
            var tz = Cross(f1, f2).Normalize(2);
            var ty = Cross(tz, tx);

            M.SetRow(0, tx);
            M.SetRow(1, ty);
            M.SetRow(2, tz);

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

            var nx = P12.Normalize(2);
            var nz = Cross(nx, P13).Normalize(2);
            var ny = Cross(nz, nx);

            M.SetRow(0, nx);
            M.SetRow(1, ny);
            M.SetRow(2, nz);
            
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
            var a4 = - tmp4* phi2* phi2 - tmp4* phi1 * phi1 - tmp4;

            double tmp3 = Math.Pow(p2, 3);
            var a3 =
                + (2 * tmp3 * d12 * b)
                + (2 * tmp3 * phi2 * phi2 * d12 * b)
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
