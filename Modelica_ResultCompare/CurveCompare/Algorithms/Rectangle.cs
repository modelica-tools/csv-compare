// Rectangle.cs
// authors: Susanne Walther, Dr. Uwe Schnabel
// date: 15.05.2015

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// for visualization with ChartControl
#if GUI
using System.Windows.Forms;
#endif
using System.Drawing;
using System.Threading;

namespace CurveCompare.Algorithms
{
    /// <summary>
    /// Represents an algorithm, that calculates a lower and an upper tube curve. Around each point of reference curve imagine a rectangle.
    /// The upper tube curve is above all rectangles, and the lower tube curve is beneath all rectangles.
    /// </summary>
    public class Rectangle : Algorithm
    {
        private bool successful;

        /// <summary>
        /// true, if calculation of tube successful; <para>
        /// false, if calculation fails.</para>
        /// </summary>
        override public bool Successful
        {
            get { return successful; }
            set { successful = value; }
        }
        /// <summary>
        /// Calculates a lower and an upper tube curve.
        /// </summary>
        /// <param name="reference">Reference curve with x and y values.</param>
        /// <param name="size">Size of tube.</param>
        /// <returns>Collection of return values.</returns>
        public override TubeReport Calculate(Curve reference, TubeSize size)
        {
            TubeReport report = new TubeReport();
            successful = false;

            if (reference != null && size != null)
            {
                report.Reference = reference;
                report.Size = size;
                report.Algorithm = AlgorithmOptions.Rectangle;
                report.Lower = CalculateLower(reference, size);
                report.Upper = CalculateUpper(reference, size);
                if (report.Lower.ImportSuccessful && report.Upper.ImportSuccessful)
                    successful = true;
            }
            return report;
        }
        /// <summary>
        /// Calculates the lower tube curve.
        /// </summary>
        /// <param name="reference">Reference curve.</param>
        /// <param name="size">Tube size.</param>
        /// <returns>Lower tube curve.</returns>
        private static Curve CalculateLower(Curve reference, TubeSize size)
        {
            Curve lower;                                            // lower tube curve
            List<double> LX = new List<double>(2 * reference.Count); // x-values of lower tube curve
            List<double> LY = new List<double>(2 * reference.Count); // y-values of lower tube curve

            // -------------------------------------------------------------------------------------------------------------
            // ---------------------------------- 1. Add corner points of the rectangle  -----------------------------------
            // -------------------------------------------------------------------------------------------------------------

            double m0, m1;                                           // slopes before and after point i of reference curve
            double s0, s1;                                           // signum of slopes of reference curve: 1 - increasing, 0 - constant, -1 - decreasing
            // machine accuracy, the least positive floating point number, with 1 + epsilon > 1, is ~ 2.2e-16
            double epsilon = 1e-15;
            int b;

            // -------------------------------------------------------------------------------------------------------------
            // 1.1. Start: Rectangle with center (x,y) = (reference.X[0], reference.Y[0])
            // -------------------------------------------------------------------------------------------------------------

            // ignore identical point at the beginning
            b = 0;
            while ((reference.X[b] - reference.X[b + 1] == 0) && (reference.Y[b] - reference.Y[b + 1] == 0))
                b++;
            // slopes of reference curve (initialization)
            s0 = Math.Sign(reference.Y[b + 1] - reference.Y[b]);
            if (reference.X[b + 1] != reference.X[b])
                m0 = (reference.Y[b + 1] - reference.Y[b]) / (reference.X[b + 1] - reference.X[b]);
            else
                if (s0 > 0)
                    m0 = Double.PositiveInfinity;
                else
                    m0 = Double.NegativeInfinity;

            // add point down left
            LX.Add(reference.X[b] - size.X);
            LY.Add(reference.Y[b] - size.Y);

            if (s0 == 1)
            {
                // add point down right
                LX.Add(reference.X[b] + size.X);
                LY.Add(reference.Y[b] - size.Y);
            }

            // -------------------------------------------------------------------------------------------------------------
            // 1.2. Iteration: Rectangle with center (x,y) = (reference.X[i], reference.Y[i])
            // -------------------------------------------------------------------------------------------------------------
            for (int i = b + 1; i < reference.Count - 1; i++)
            {
                // ignore identical points
                if ((reference.X[i] - reference.X[i + 1] == 0) && (reference.Y[i] - reference.Y[i + 1] == 0))
                    continue;

                // slopes of reference curve
                s1 = Math.Sign(reference.Y[i + 1] - reference.Y[i]);
                if (reference.X[i + 1] - reference.X[i] != 0)
                    m1 = (reference.Y[i + 1] - reference.Y[i]) / (reference.X[i + 1] - reference.X[i]);
                else
                    if (s1 > 0)
                        m1 = Double.PositiveInfinity;
                    else
                        m1 = Double.NegativeInfinity;

                // add no point for equal slopes of reference curve
                if (!(m0 == m1))
                {
                    if ((s0 != -1) && (s1 != -1))
                    {
                        // add point down right
                        LX.Add(reference.X[i] + size.X);
                        LY.Add(reference.Y[i] - size.Y);
                    }
                    else if ((s0 != 1) && (s1 != 1))
                    {
                        // add point down left
                        LX.Add(reference.X[i] - size.X);
                        LY.Add(reference.Y[i] - size.Y);
                    }
                    else if ((s0 == -1) && (s1 == 1))
                    {
                        // add point down left
                        LX.Add(reference.X[i] - size.X);

                        LY.Add(reference.Y[i] - size.Y);
                        // add point down right
                        LX.Add(reference.X[i] + size.X);
                        LY.Add(reference.Y[i] - size.Y);
                    }
                    else if ((s0 == 1) && (s1 == -1))
                    {
                        // add point down right
                        LX.Add(reference.X[i] + size.X);
                        LY.Add(reference.Y[i] - size.Y);
                        // add point down left
                        LX.Add(reference.X[i] - size.X);
                        LY.Add(reference.Y[i] - size.Y);
                    }
                    // remove the last added points in case of equal slopes equal 0 of tube curve
                    int last = LY.Count - 1;
                    if (reference.Y[i + 1] - size.Y == LY[last])
                    {
                        // remove two points, if two points were added at last
                        // (last - 2 >= 0, because start point + two added points)
                        if (s0 * s1 == -1 && LY[last - 2] == LY[last])
                        {
                            LX.RemoveAt(last);
                            LY.RemoveAt(last);
                            LX.RemoveAt(last - 1);
                            LY.RemoveAt(last - 1);
                        }
                        // remove one point, if one point was added at last
                        // (last - 1 >= 0, because start point + one added point)
                        else if (s0 * s1 != -1 && LY[last - 1] == LY[last])
                        {
                            LX.RemoveAt(last);
                            LY.RemoveAt(last);
                        }
                    }
                }
                s0 = s1;
                m0 = m1;
            }
            // -------------------------------------------------------------------------------------------------------------
            // 1.3. End: Rectangle with center (x,y) = (reference.X[reference.Count - 1], reference.Y[reference.Count - 1])
            // -------------------------------------------------------------------------------------------------------------
            if (s0 == -1)
            {
                // add point down left
                LX.Add(reference.X[reference.Count - 1] - size.X);
                LY.Add(reference.Y[reference.Count - 1] - size.Y);

                // add point top right
                LX.Add(reference.X[reference.Count - 1] + size.X);
                // take into account the slope between last two points for Y value,
                // to avoid false positive for curves with hight derivative twords the end
                LY.Add(LY.Last() + (size.X * m0 * 2));
            }
            else
            {
                // add point down right
                LX.Add(reference.X[reference.Count - 1] + size.X);
                LY.Add(reference.Y[reference.Count - 1] - size.Y);
            }
            // -------------------------------------------------------------------------------------------------------------
            // -------------- 2. Remove points and add intersection points in case of backward order -----------------------
            // -------------------------------------------------------------------------------------------------------------

            removeLoop(LX, LY, true);

            lower = new Curve("Lower", LX.ToArray(), LY.ToArray());
            return lower;
        }

        /// <summary>
        /// Calculates the upper tube curve.
        /// </summary>
        /// <param name="reference">Reference curve.</param>
        /// <param name="size">Tube size.</param>
        /// <returns>Upper tube curve.</returns>
        private static Curve CalculateUpper(Curve reference, TubeSize size)
        {
            Curve upper;                                             // upper tube curve
            List<double> UX = new List<double>(2 * reference.Count); // x-values of upper tube curve
            List<double> UY = new List<double>(2 * reference.Count); // y-values of upper tube curve

            // ---------------------------------------------------------------------------------------------------------
            // ---------------------------------- 1. Add corner points of the rectangle  -------------------------------
            // ---------------------------------------------------------------------------------------------------------

            double m0, m1;                                           // slopes before and after point i of reference curve
            double s0, s1;                                           // signum of slopes of reference curve: 1 - increasing, 0 - constant, -1 - decreasing
            // machine accuracy, the least positive floating point number, with 1 + epsilon > 1, is ~ 2.2e-16
            double epsilon = 1e-15;
            int b;

            // ---------------------------------------------------------------------------------------------------------
            // 1.1. Start: Rectangle with center (x,y) = (reference.X[0], reference.Y[0])
            // ---------------------------------------------------------------------------------------------------------

            // ignore identical point at the beginning
            b = 0;
            while ((reference.X[b] - reference.X[b + 1] == 0) && (reference.Y[b] - reference.Y[b + 1] == 0))
                b++;
            // slopes of reference curve (initialization)
            s0 = Math.Sign(reference.Y[b + 1] - reference.Y[b]);
            if (reference.X[b + 1] != reference.X[b])
                m0 = (reference.Y[b + 1] - reference.Y[b]) / (reference.X[b + 1] - reference.X[b]);
            else
                if (s0 > 0)
                    m0 = Double.PositiveInfinity;
                else
                    m0 = Double.NegativeInfinity;

            // add point top left
            UX.Add(reference.X[b] - size.X);
            UY.Add(reference.Y[b] + size.Y);

            if (s0 == -1)
            {
                // add point top right
                UX.Add(reference.X[b] + size.X);
                UY.Add(reference.Y[b] + size.Y);
            }

            // ---------------------------------------------------------------------------------------------------------
            // 1.2. Iteration: Rectangle with center (x,y) = (reference.X[i], reference.Y[i])
            // ---------------------------------------------------------------------------------------------------------
            for (int i = b + 1; i < reference.Count - 1; i++)
            {
                // ignore identical points
                if ((reference.X[i] - reference.X[i + 1] == 0) && (reference.Y[i] - reference.Y[i + 1] == 0))
                    continue;

                // slopes of reference curve
                s1 = Math.Sign(reference.Y[i + 1] - reference.Y[i]);
                if (reference.X[i + 1] - reference.X[i] != 0)
                    m1 = (reference.Y[i + 1] - reference.Y[i]) / (reference.X[i + 1] - reference.X[i]);
                else
                    if (s1 > 0)
                        m1 = Double.PositiveInfinity;
                    else
                        m1 = Double.NegativeInfinity;

                // add no point for equal slopes of reference curve
                if (!(m0 == m1))
                {
                    if ((s0 != -1) && (s1 != -1))
                    {
                        // add point top left
                        UX.Add(reference.X[i] - size.X);
                        UY.Add(reference.Y[i] + size.Y);
                    }
                    else if ((s0 != 1) && (s1 != 1))
                    {
                        // add point top right
                        UX.Add(reference.X[i] + size.X);
                        UY.Add(reference.Y[i] + size.Y);
                    }
                    else if ((s0 == 1) && (s1 == -1))
                    {
                        // add point top left
                        UX.Add(reference.X[i] - size.X);
                        UY.Add(reference.Y[i] + size.Y);
                        // add point top right
                        UX.Add(reference.X[i] + size.X);
                        UY.Add(reference.Y[i] + size.Y);
                    }
                    else if ((s0 == -1) && (s1 == 1))
                    {
                        // add point top right
                        UX.Add(reference.X[i] + size.X);
                        UY.Add(reference.Y[i] + size.Y);
                        // add point top left
                        UX.Add(reference.X[i] - size.X);
                        UY.Add(reference.Y[i] + size.Y);
                    }
                    // remove the last added points in case of equal slopes equal 0 of tube curve
                    int last = UY.Count - 1;
                    if (reference.Y[i + 1] + size.Y == UY[last])
                    {
                        // remove two points, if two points were added at last
                        // (last - 2 >= 0, because start point + two added points)
                        if (s0 * s1 == -1 && UY[last - 2] == UY[last])
                        {
                            UX.RemoveAt(last);
                            UY.RemoveAt(last);
                            UX.RemoveAt(last - 1);
                            UY.RemoveAt(last - 1);
                        }
                        // remove one point, if one point was added at last
                        // (last - 1 >= 0, because start point + one added point)
                        else if (s0 * s1 != -1 && UY[last - 1] == UY[last])
                        {
                            UX.RemoveAt(last);
                            UY.RemoveAt(last);
                        }
                    }
                }
                s0 = s1;
                m0 = m1;
            }
            // -------------------------------------------------------------------------------------------------------------
            // 1.3. End: Rectangle with center (x,y) = (reference.X[reference.Count - 1], reference.Y[reference.Count - 1])
            // -------------------------------------------------------------------------------------------------------------
            if (s0 == 1)
            {
                // add point top left
                UX.Add(reference.X[reference.Count - 1] - size.X);
                UY.Add(reference.Y[reference.Count - 1] + size.Y);

                // add point top right
                UX.Add(reference.X[reference.Count - 1] + size.X);
                // take into account the slope between last two points for Y value,
                // to avoid false positive for curves with hight derivative twords the end
                UY.Add(UY.Last() + (size.X * m0 * 2));
            }
            else
            {
                // add point top right
                UX.Add(reference.X[reference.Count - 1] + size.X);
                UY.Add(reference.Y[reference.Count - 1] + size.Y);
            }

            // ---------------------------------------------------------------------------------------------------------
            // -------------- 2. Remove points and add intersection points in case of backward order -------------------
            // ---------------------------------------------------------------------------------------------------------

            removeLoop(UX, UY, false);

            upper = new Curve("Upper", UX.ToArray(), UY.ToArray());
            return upper;
        }
        /// <summary>
        ///  Remove points and add intersection points in case of backward order
        /// </summary>
        /// <param name="X">x values of curve</param>
        /// <param name="Y">y values of curve</param>
        /// <param name="lower">if true, algorithm for lower tube curve is used;<para>
        /// if false, algorithm for upper tube curve is used</para></param>
        /// <return>Number of loops, that are removed.</return>
        private static int removeLoop(List<double> X, List<double> Y, bool lower)
        {
            // Visualization of working of removeLoop
            bool visualize = false;
            List<double> XLoops = new List<double>(X);
            List<double> YLoops = new List<double>(Y);
            int j = 1;
            int countLoops = 0;

#if GUI
            // Visualization
            if (visualize)
            {
                ChartControl chartControl = new ChartControl(600, Int32.MaxValue, true);
                chartControl.AddLine("Tube curve with loop", X, Y, Color.FromKnownColor(KnownColor.Cyan));
                Application.Run(chartControl);
            }
#endif
            while (j < X.Count - 2)
            {
                // Find backward segment (j, j + 1)
                if (X[j + 1] < X[j])
                {
                    countLoops++;

                    // ----------------------------------------------------------------------------------------------------
                    // 1. Find i,k, such that i <= j < j + 1 <= k - 1 and segment (i - 1, i) intersect segment (k - 1, k)
                    // ----------------------------------------------------------------------------------------------------

                    int i, k, iPrevious;
                    double y;
                    // for calculation and adding of intersection point
                    bool addPoint = true;
                    double ix = 0;
                    double iy = 0;
                    int kMax;

                    i = j;
                    iPrevious = i;
                    // Find initial value for i = i_s, such that X[i_s - 1] <= X[j + 1] < X[i_s]
                    // it holds: i element of interval (i_s, j)
                    while (X[j + 1] < X[i - 1]) // X[j + 1] < X[i - 1] => (i - 1 > 0 && Y[i - 2] <= Y[i - 1]) (in case of lower)
                        i--;

                    // j + 1 < k <= kMax
                    kMax = j + 1;
                    while (X[kMax] < X[j] && kMax < Y.Count - 1)
                        kMax++;

                    // initial value for k
                    k = j + 1;
                    y = Y[i - 1]; // X[i - 1] <= X[j + 1] == X[k] < X[i] && in case of lower: y == Y[i - 1] <= Y[i] <= Y[j] == Y[j + 1] == Y[k]

                    // Find k
                    while (((lower && y < Y[k]) || (!lower && Y[k] < y)) && k < kMax)// y < Y[k] => k < X.Count
                    {
                        iPrevious = i;
                        k++;
                        //while ((X[i] < X[k] || (X[i] == X[k] && Y[i] < Y[k])) && i < j)
                        while ((X[i] < X[k] || (lower && X[i] == X[k] && Y[i] < Y[k] && !(k + 1 < X.Count && X[k] == X[k + 1] && Y[k + 1] < Y[k])) || (!lower && X[i] == X[k] && Y[i] > Y[k] && !(k + 1 < X.Count && X[k] == X[k + 1] && Y[k + 1] > Y[k]))) && i < j)
                                i++;
                        // it holds X[i - 1] < X[k] <= X[i], particularly X[i] != X[i - 1]
                        // for i < j and X[i - 1] < X[k] it holds X[i - 1] < X[k] <= X[i], particularly X[i] != X[i - 1]
                        // linear interpolation of (x, y) = (X[k], y) on segment (i - 1, i)
                        if (X[i] - X[i - 1] != 0)
                            y = (Y[i] - Y[i - 1]) / (X[i] - X[i - 1]) * (X[k] - X[i - 1]) + Y[i - 1];
                        else
                            y = Y[i];
                    }
                    // k located: intersection point is on segment (k - 1, k)
                    // i approximately located: intersection point is on polygonal line (iPrevoius - 1, i)
                    // Regular case
                    if (iPrevious > 1)
                        i = iPrevious - 1;
                    // Special case handling: assure, that i - 1 >= 0
                    else
                        i = iPrevious;
                    if (X[k] != X[k - 1])
                        // linear interpolation of (x, y) = (X[i], y) on segment (k - 1, k)
                        y = (Y[k] - Y[k - 1]) / (X[k] - X[k - 1]) * (X[i] - X[k - 1]) + Y[k - 1];
                    // it holds Y[i] = Y[iPrevious - 1] < Y[k - 1]
                    // Find i
                    while ((X[k] != X[k - 1] && ((lower && Y[i] < y) || (!lower && y < Y[i]))) || (X[k] == X[k - 1] && X[i] < X[k]))
                    {
                        i++;
                        if (X[k] != X[k - 1])
                            // linear interpolation of (x, y) = (X[i], y) on segment (k - 1, k)
                            y = (Y[k] - Y[k - 1]) / (X[k] - X[k - 1]) * (X[i] - X[k - 1]) + Y[k - 1];
                    }
                    // ----------------------------------------------------------------------------------------------------
                    // 2. Calculate intersection point (ix, iy) of segments (i - 1, i) and (k - 1, k)
                    // ----------------------------------------------------------------------------------------------------
                    double a1 = 0;
                    double a2 = 0;

                    // both branches vertical
                    if (X[i] == X[i - 1] && X[k] == X[k - 1])
                    {
                        // add no point; check if case occur: slopes have different signs
                        addPoint = false;
                    }
                    // case i-branch vertical
                    else if (X[i] == X[i - 1])
                    {
                        ix = X[i];
                        iy = Y[k - 1] + ((X[i] - X[k - 1]) * (Y[k] - Y[k - 1])) / (X[k] - X[k - 1]);
                    }
                    // case k-branch vertical
                    else if (X[k] == X[k - 1])
                    {
                        ix = X[k];
                        iy = Y[i - 1] + ((X[k] - X[i - 1]) * (Y[i] - Y[i - 1])) / (X[i] - X[i - 1]);
                    }
                    // common case
                    else
                    {
                        a1 = (Y[i] - Y[i - 1]) / (X[i] - X[i - 1]); // slope of segment (i - 1, i)
                        a2 = (Y[k] - Y[k - 1]) / (X[k] - X[k - 1]); // slope of segment (k - 1, k)
                        // common case: no equal slopes
                        if (a1 != a2)
                        {
                            ix = (a1 * X[i - 1] - a2 * X[k - 1] - Y[i - 1] + Y[k - 1]) / (a1 - a2);

                            if (Math.Abs(a1) > Math.Abs(a2))
                                // calculate y on segment (k - 1, k)
                                iy = a2 * (ix - X[k - 1]) + Y[k - 1];
                            else
                                // calculate y on segment (i - 1, i)
                                iy = a1 * (ix - X[i - 1]) + Y[i - 1];
                        }
                        else
                            // case equal slopes: add no point
                            addPoint = false;
                    }
                    // ----------------------------------------------------------------------------------------------------
                    // 3. Delete points i until (including) k - 1
                    // ----------------------------------------------------------------------------------------------------
                    int count = k - i;
                    X.RemoveRange(i, count);
                    Y.RemoveRange(i, count);
                    // ----------------------------------------------------------------------------------------------------
                    // 4. Add intersection point
                    // ----------------------------------------------------------------------------------------------------
                    // add intersection point, if it isn`t already there
                    if (addPoint && (X[i] != ix || Y[i] != iy))
                    {
                        X.Insert(i, ix);
                        Y.Insert(i, iy);
                    }
                    // ----------------------------------------------------------------------------------------------------
                    // 5. set j = i
                    // ----------------------------------------------------------------------------------------------------
                    j = i;
                    // ----------------------------------------------------------------------------------------------------
                    // 6. Delete points that are doubled
                    // ----------------------------------------------------------------------------------------------------
                    if (X[i - 1] == X[i] && Y[i - 1] == Y[i])
                    {
                        X.RemoveAt(i);
                        Y.RemoveAt(i);
                        j = i - 1;
                    }
                }
                j++;
            }
#if GUI
            // Visualization
            if (visualize)
            {
                ChartControl control = new ChartControl(500, Int32.MaxValue, true);
                control.AddLine("Tube curve with loop", XLoops, YLoops, Color.FromKnownColor(KnownColor.Cyan));
                control.AddLine("Tube curve without loop", X, Y, Color.FromKnownColor(KnownColor.Red));
                Application.Run(control);
            }
#endif
            return countLoops;
        }
    }
}
