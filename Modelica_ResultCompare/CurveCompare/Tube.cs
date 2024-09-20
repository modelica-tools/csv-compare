// Tube.cs
// authors: Susanne Walther, Sven Ruetz
// date: 19.12.2014

using System;
using System.Collections.Generic;
using CurveCompare.Algorithms;

namespace CurveCompare
{
    /// <summary>
    /// The class provides methods for tube calculation. A tube can be calculated around a reference curve. It can be verified, if a compare curve is inside the tube.
    /// </summary>
    /// <remarks>Usage: <para>
    /// Choose a reference curve (reference) and a relative height for the tube (relativeHeight).
    /// Choose a curve to be compared.<para/>
    /// size = TubeSize(relativeHeight, reference);<para/>
    /// tube = Tube(size);<para/>
    /// calcResult = tube.Calculate(reference);
    /// tubeReport = calcResult.Item1;
    /// success = calcResult.Item2; // Handle success
    /// success = Tube.Validate(compare, tubeReport); // Handle success<para/>
    /// </para></remarks>
    public class Tube
    {
        private TubeSize size;
        private Algorithm algorithm;
        private AlgorithmOptions algorithmOption;

        /// <summary>
        /// Option for the tube calculation algorithm. Defines how the distance between the reference curve and the tube is measured.
        /// </summary>
        public AlgorithmOptions AlgorithmOption
        {
            get { return algorithmOption; }
            set { algorithmOption = value; }
        }
        /// <summary>
        /// Sets standard value for Tube.AlgorithmOption
        /// </summary>
        public Tube(TubeSize size)
        {
            this.size = size;
            algorithmOption = AlgorithmOptions.Rectangle;
        }
        /// <summary>
        /// Calculates the Tube, that consists of 2 Curves: Lower and Upper.
        /// </summary>
        /// <param name="reference">Reference curve.</param>
        /// <returns>Tuple consisting of tube calculation and success state.</returns>
        public Tuple<TubeReport, bool> Calculate(Curve reference)
        {
            if (ChooseAlgorithm())
            {
                TubeReport report = algorithm.Calculate(reference, size);
                return Tuple.Create(report, algorithm.Successful);
            }
            return Tuple.Create(new TubeReport(), false);
        }
        /// <summary>
        /// Validates if compare is inside the tube.
        /// </summary>
        /// <param name="test">Curve, that shall be compared with the reference curve.</param>
        /// <param name="report">Data about tube calculation and comparison.</param>
        /// <returns>Success state.</returns>
        public static bool Validate(Curve test, TubeReport report)
        {
            if (report == null)
                return false;

            if (test != null && report.Lower != null && report.Upper != null && test.ImportSuccessful && report.Lower.ImportSuccessful && report.Upper.ImportSuccessful)
            {
                double[] newLower = InterpolateValues(report.Lower.X, report.Lower.Y, test.X);
                double[] newUpper = InterpolateValues(report.Upper.X, report.Upper.Y, test.X);
                Curve errors;
                int errorCount;

                report.Test = test;
                bool validationSuccess = Compare(newLower, newUpper, test.Y, test.X, out errorCount, out errors);
                report.Errors = errors;

                if (validationSuccess)
                {
                    if (report.Errors.Count == 0)
                        report.Valid = Validity.Valid;
                    else
                        report.Valid = Validity.Invalid;
                }
                else
                {
                    // Error: validation not successful
                    report.Valid = Validity.Undefined;
                }
                return validationSuccess;
            }

            // Error: no data
            report.Valid = Validity.Undefined;
            return false;
        }
        /// <summary>
        /// Compares with tube
        /// </summary>
        /// <param name="lower">Lower tube curve values.</param>
        /// <param name="upper">Upper tube curve values.</param>
        /// <param name="test">Compare curve values.</param>
        /// <param name="time">Time values.</param>
        /// <param name="Errors">Errors.</param>
        /// <returns>Success state.</returns>
        private static bool Compare(double[] lower, double[] upper, double[] test, double[] time, out int errorCount, out Curve errors)
        {
            // --------------------------------------------------------------------------------------------------------------------------------
            // ------------------------------------ Copy and modified from CsvCompare.Range.Validate ------------------------------------------
            // --------------------------------------------------------------------------------------------------------------------------------

            bool successful = true;
            double[] X, Y;
            List<double> errorsTime = new List<double>(time.Length);
            List<double> errorsDif = new List<double>(time.Length);
            errorCount = 0;

            for (int i = 0; i < test.Length && i < upper.Length && i < lower.Length; i++)
            {
                if (test[i] < lower[i] || test[i] > upper[i])
                {
                    errorCount++;
                    try
                    {
                        if (test[i] < lower[i])
                        {
                            errorsTime.Add(time[i]);
                            errorsDif.Add(Math.Abs(lower[i] - test[i]));
                        }
                        else
                        {
                            errorsTime.Add(time[i]);
                            errorsDif.Add(Math.Abs(upper[i] - test[i]));
                        }
                    }
                    catch (Exception)
                    {
                        errorsTime.Add(time[i]);
                        errorsDif.Add(1);
                        successful = false;
                    } // should never happen, just in case something goes wrong
                }
            }
            X = errorsTime.ToArray();
            Y = errorsDif.ToArray();
            errors = new Curve("Errors", X, Y);
            return successful;
        }
        /// <summary>
        /// Interpolates points.
        /// </summary>
        /// <param name="sourceTimeLine">Interpolation source</param>
        /// <param name="targetTimeLine">Target time values.</param>
        /// <param name="sourceValues">Source time values.</param>
        /// <returns>Interpolated values.</returns>
        private static double[] InterpolateValues(double[] sourceTimeLine, double[] sourceValues, double[] targetTimeLine)
        {
            // --------------------------------------------------------------------------------------------------------------------------------
            // ------------------------------------------ Copy from CsvCompare.Range.CalibrateValues ------------------------------------------
            // --------------------------------------------------------------------------------------------------------------------------------

            if (null == sourceValues || sourceValues.Length == 0)
                return sourceValues;

            double[] TargetValues = new double[targetTimeLine.Length];

            int j = 1;
            double x, x0, x1, y0, y1;

            for (int i = 0; i < targetTimeLine.Length; i++)
            {
                if (targetTimeLine[i] > sourceTimeLine[sourceTimeLine.Length - 1])//Prevent extrapolating
                {
                    Array.Resize<double>(ref TargetValues, i);
                    break;
                }

                x = targetTimeLine[i];

                x1 = sourceTimeLine[j];
                y1 = sourceValues[j];

                while (x1 < x && j + 1 < sourceTimeLine.Length && j + 1 < sourceValues.Length)// step source timeline to the current moment
                {
                    j++;

                    x1 = sourceTimeLine[j];
                    y1 = sourceValues[j];
                }

                x0 = sourceTimeLine[j - 1];
                y0 = sourceValues[j - 1];

                if (((x1 - x0) * (x - x0)) != 0)//prevent NaN -> division by zero
                    TargetValues[i] = y0 + (((y1 - y0) / (x1 - x0)) * (x - x0)); // linear interpolation of the source value at the target moment in time
                else
                    TargetValues[i] = y0;
            }

            return TargetValues;
        }
        /// <summary>
        /// Decides which tube calculation algorithm to use.
        /// </summary>
        /// <returns>Success state.</returns>
        private bool ChooseAlgorithm()
        {
            bool successful = true;

            if (algorithmOption == AlgorithmOptions.Rectangle)
                algorithm = new Rectangle();
            else if (algorithmOption == AlgorithmOptions.Ellipse)
                algorithm = new Ellipse();
            //else if (algorithmOption == AlgorithmOptions.Backward)
              //  algorithm = new Backward();
            else
                successful = false;

            return successful;
        }
    }


}
