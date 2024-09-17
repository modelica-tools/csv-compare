// Ellipse.cs
// authors: Susanne Walther, Dr. Uwe Schnabel
// date: 4.12.2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace CurveCompare.Algorithms
{
    /// <summary>
    ///  Represents an algorithm, that calculates a lower and an upper tube curve. Around each point of reference curve imagine an ellipse.
    ///  The upper tube curve is above all ellipses, and the lower tube curve is beneath all ellipses.
    /// </summary>
    public class Ellipse : Algorithm
    {
        #region private members

        private bool successful;

        // List used for the upper tube
        private ArrayList _lmh, _li0h, _li1h;

        // List used for the lower tube
        private ArrayList _lml, _li0l, _li1l;

        private double _dtStart, _dtStop;
        private double _dX1, _dY1, _dX2, _dY2;

        private double _dCurrentSlope, _dSlopeDif;

        private double _dDelta;
        private double _dS;
        private double _dXRelEps = 1e-15;
        private double _dXMinStep;
        //private bool _bAllIntervals;

        #endregion

        public static bool RelativeErrors = true;

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
        /// <param name="reference">Reference curve with x and y values..</param>
        /// <param name="size">Size of tube.</param>
        /// <param name="minX">Min abscissa value.</param>
        /// <param name="maxX">Max abscissa value.</param>
        /// <returns>Collection of return values.</returns>
        public override TubeReport Calculate(Curve reference, TubeSize size, double minX, double maxX)
        {
            /*
            ///This method generates tubes around a given curve.
            /// @param x The time values for the base curve
            /// @param y The y values of the base curve
            /// @param x_low An empty, initialized list that is to be filled with the time values for the lower tube curve
            /// @param y_low An empty, initialized list that is to be filled with the y values for the lower tube curve
            /// @param x_high An empty, initialized list that is to be filled with the time values for the upper tube curve
            /// @param y_high An empty, initialized list that is to be filled with the y values for the upper tube curve
            /// @param r relative tolerance argument
            /// @param bAllIntervals used all intervals
             * */
            double[] x = reference.X.ToArray();
            double[] y = reference.Y.ToArray();
            _dDelta = size.X;
            List<double> xHigh = new List<double>();
            List<double> yHigh = new List<double>();
            List<double> xLow = new List<double>();
            List<double> yLow = new List<double>();

            TubeReport report = new TubeReport();
            successful = false;

            if (reference != null && size != null)
            {
                report.Reference = reference;
                report.Size = size;
                report.Algorithm = AlgorithmOptions.Ellipse;

                /// SBR: Always assume the algorithm works correctly.
                //_bAllIntervals = true;
                // set tStart and tStop
                _dtStart = x[0];
                _dtStop = x[x.Length - 1];
                _dXMinStep = ((_dtStop - _dtStart) + Math.Abs(_dtStart)) * _dXRelEps;

                // Initialize lists (upper tube)
                _lmh = new ArrayList(x.Length);	// slope in i
                _li0h = new ArrayList(x.Length);	// i
                _li1h = new ArrayList(x.Length);	// i - 1

                // Initialize lists (lower tube)
                _lml = new ArrayList(x.Length);		// slope in i
                _li0l = new ArrayList(x.Length);	// i
                _li1l = new ArrayList(x.Length);	// i-1

                // calculate the tubes delta
                //_dDelta = r * (Math.Abs(_dtStop - _dtStart) + Math.Abs(_dtStart));

                // calculate S:
                double max = y[0];
                double min = y[0];

                xHigh.Clear();
                yHigh.Clear();
                xLow.Clear();
                yLow.Clear();

                if (xHigh.Capacity < x.Length) xHigh.Capacity = x.Length;
                if (yHigh.Capacity < x.Length) yHigh.Capacity = x.Length;
                if (xLow.Capacity < x.Length) xLow.Capacity = x.Length;
                if (yLow.Capacity < x.Length) yLow.Capacity = x.Length;

                for (int i = 1; i < y.Length; i++)
                {
                    try
                    {
                        double Y = y[i];
                        if (Y > max)
                        {
                            max = Y;
                        }
                        if (Y < min)
                        {
                            min = Y;
                        }
                    }
                    catch (IndexOutOfRangeException) { break; }
                }
                //            _dS = Math.Abs(4 * (max - min) / (Math.Abs(_dtStop - _dtStart)));
                _dS = (Math.Abs(max - min) + Math.Abs(min)) / (Math.Abs(_dtStop - _dtStart) + Math.Abs(_dtStart));

                if (_dS < 0.0004 / (Math.Abs(_dtStop - _dtStart) + Math.Abs(_dtStart)))
                {
                    _dS = 0.0004 / (Math.Abs(_dtStop - _dtStart) + Math.Abs(_dtStart));
                }

                bool bJump = false;

                // Begin calculation for the tubes
                for (int i = 1; i < x.Length; i++)
                {
                    try
                    {
                        // get current value
                        _dX1 = x[i];
                        _dY1 = y[i];

                        // get previous value
                        _dX2 = x[i - 1];
                        _dY2 = y[i - 1];
                    }
                    catch (IndexOutOfRangeException) { break; }

                    // catch jumps
                    bJump = false;
                    if ((_dX1 <= _dX2) && (_dY1 == _dY2) && (xHigh.Count == 0))
                        continue;
                    if ((_dX1 <= _dX2) && (_dY1 == _dY2))
                    {
                        _dX1 = Math.Max(x[(int)_li1l[_li1l.Count - 1]] + _dXMinStep, Math.Max(x[(int)_li1h[_li1h.Count - 1]] + _dXMinStep, _dX1));
                        x.SetValue(_dX1, i);
                        _dCurrentSlope = (double)_lmh[_lmh.Count - 1];
                    }
                    else
                    {
                        if (_dX1 <= _dX2)
                        {
                            bJump = true;
                            _dX1 = _dX2 + _dXMinStep;
                            x.SetValue(_dX1, i);
                        }
                        _dCurrentSlope = (_dY1 - _dY2) / (_dX1 - _dX2); // calculate current slope ( 3.2.6.1)
                    }

                    // fill lists with new values: values upper tube
                    _li0h.Add(i);
                    _li1h.Add(i - 1);
                    _lmh.Add(_dCurrentSlope);

                    // fill lists with new values: values lower tube
                    _li0l.Add(i);
                    _li1l.Add(i - 1);
                    if ((_dX1 <= _dX2) && (_dY1 == _dY2))
                        _dCurrentSlope = (double)_lml[_lml.Count - 1];
                    _lml.Add(_dCurrentSlope);

                    if (xHigh.Count == 0) // 1st interval (3.2.5)
                    {
                        if (bJump)
                        {
                            // initial values upper tube
                            _li0h[0] = i - 1;
                            _lmh[0] = 0.0;
                            xHigh.Add(_dX2 - _dDelta - _dXMinStep);
                            yHigh.Add(_dY2 + _dDelta * _dS);
                            _li0h.Add(i);
                            _li1h.Add(i - 1);
                            _lmh.Add(_dCurrentSlope);
                            xHigh.Add(_dX2 - _dDelta * _dCurrentSlope / (_dS + Math.Sqrt((_dCurrentSlope * _dCurrentSlope) + (_dS * _dS))));
                            yHigh.Add(_dY2 + _dDelta * _dS);

                            // initial values lower tube
                            _li0l[0] = i - 1;
                            _lml[0] = 0.0;
                            xLow.Add(_dX2 - _dDelta - _dXMinStep);
                            yLow.Add(_dY2 - _dDelta * _dS);
                            _li0l.Add(i);
                            _li1l.Add(i - 1);
                            _lml.Add(_dCurrentSlope);
                            xLow.Add(_dX2 + _dDelta * _dCurrentSlope / (_dS + Math.Sqrt((_dCurrentSlope * _dCurrentSlope) + (_dS * _dS))));
                            yLow.Add(_dY2 - _dDelta * _dS);
                        }
                        else
                        {       // initial values upper tube
                            xHigh.Add(_dX2 - _dDelta);
                            yHigh.Add(_dY2 - _dCurrentSlope * _dDelta + _dDelta * Math.Sqrt((_dCurrentSlope * _dCurrentSlope) + (_dS * _dS)));

                            // initial values lower tube
                            xLow.Add(_dX2 - _dDelta);
                            yLow.Add(_dY2 - _dCurrentSlope * _dDelta - _dDelta * Math.Sqrt((_dCurrentSlope * _dCurrentSlope) + (_dS * _dS)));
                        }
                    }
                    else	// if not 1st interval (3.2.6)
                    {
                        // fill lists with new values, set X and Y to arbitrary value (3.2.6.1)
                        xHigh.Add(1);
                        yHigh.Add(1);

                        xLow.Add(1);
                        yLow.Add(1);

                        // begin procedure for upper tube
                        GenerateHighTube(x, y, xHigh, yHigh);

                        // begin procedure for lower tube
                        GenerateLowTube(x, y, xLow, yLow);
                    }

                }

                // calculate terminal value
                _dX2 = (double)x.GetValue(x.Length - 1);
                if (bJump)
                {
                    _dY2 = (double)y.GetValue(y.Length - 1);
                    // upper tube
                    _dCurrentSlope = (double)_lmh[_lmh.Count - 1];

                    xHigh.Add(_dX2 - _dDelta * _dCurrentSlope / (_dS + Math.Sqrt((_dCurrentSlope * _dCurrentSlope) + (_dS * _dS))));
                    yHigh.Add(_dY2 + _dDelta * _dS);
                    xHigh.Add(_dX2 + _dDelta + _dXMinStep);
                    yHigh.Add(_dY2 + _dDelta * _dS);

                    // lower tube
                    _dCurrentSlope = (double)_lml[_lml.Count - 1];

                    xLow.Add(_dX2 + _dDelta * _dCurrentSlope / (_dS + Math.Sqrt((_dCurrentSlope * _dCurrentSlope) + (_dS * _dS))));
                    yLow.Add(_dY2 - _dDelta * _dS);
                    xLow.Add(_dX2 + _dDelta + _dXMinStep);
                    yLow.Add(_dY2 - _dDelta * _dS);
                }
                else
                {
                    // upper tube
                    _dX1 = (double)xHigh[xHigh.Count - 1];
                    _dY1 = (double)yHigh[yHigh.Count - 1];

                    _dCurrentSlope = (double)_lmh[_lmh.Count - 1];

                    xHigh.Add(_dX2 + _dDelta);
                    yHigh.Add(_dY1 + _dCurrentSlope * (_dX2 + _dDelta - _dX1));

                    // lower tube
                    _dX1 = (double)xLow[xLow.Count - 1];
                    _dY1 = (double)yLow[yLow.Count - 1];

                    _dCurrentSlope = (double)_lml[_lml.Count - 1];

                    xLow.Add(_dX2 + _dDelta);
                    yLow.Add(_dY1 + _dCurrentSlope * (_dX2 + _dDelta - _dX1));
                }
                report.Lower = new Curve("Lower", xLow.ToArray(), yLow.ToArray());
                report.Upper = new Curve("Upper", xHigh.ToArray(), yHigh.ToArray());
                if (report.Lower.ImportSuccessful && report.Upper.ImportSuccessful)
                    successful = true;
            }
            return report;
        }
        /// <summary>
        /// Calculates the upper tube curve.
        /// </summary>
        /// <param name="x">x values of the reference curve.</param>
        /// <param name="y">y values of the reference curve.</param>
        /// <param name="xHigh">An empty, initialized list that is to be filled with the time values for the upper tube curve</param>
        /// <param name="yHigh">An empty, initialized list that is to be filled with the y values for the upper tube curve</param>
        [System.Runtime.ConstrainedExecution.ReliabilityContract(System.Runtime.ConstrainedExecution.Consistency.MayCorruptProcess, System.Runtime.ConstrainedExecution.Cer.Success)]
        private void GenerateHighTube(System.Array x, System.Array y, List<double> xHigh, List<double> yHigh)
        {
            int index = _lmh.Count - 1; // = _li0h.Count - 1 = _li1h.Count - 1 = xHigh.Count - 1 = yHigh.Count - 1 > 0

            double m1 = (double)_lmh[index];
            double m2;

            if (index <= 0)//Catch OutOfRangeExeception when setting m2
            {
                m2 = 0;
                index = 1;
            }
            else
                m2 = (double)_lmh[index - 1];

            _dSlopeDif = Math.Abs(m1 - m2);		// (3.2.6.2)

            if ((_dSlopeDif == 0) || ((_dSlopeDif < 2e-15 * Math.Max(Math.Abs(m1), Math.Abs(m2))) && ((int)_li0h[_li0h.Count - 1] - (int)_li1h[_li1h.Count - 2] < 100)))
            {
                // new accumulated value of the saved interval is the terminal
                // value of the current interval
                // after that dismiss the current interval (3.2.6.3.2.2.)


                _li1h.RemoveAt(index);		// beginning of the current interval
                _li0h.RemoveAt(index - 1);		// end of the current interval
                _lmh.RemoveAt(index);			// slope of the current interval
                xHigh.RemoveAt(index);		// X_ current interval
                yHigh.RemoveAt(index);		// Y_ current interval

                // calculation of the new slope (3.2.6.3.3)
                double X3 = (double)x.GetValue((int)_li0h[index - 1]);  // = _dX1
                double Y3 = (double)y.GetValue((int)_li0h[index - 1]);  // = _dY1
                double X4 = (double)x.GetValue((int)_li1h[index - 1]);  // < X3
                double Y4 = (double)y.GetValue((int)_li1h[index - 1]);

                // write slope to the list of slopes
                _lmh[index - 1] = (Y3 - Y4) / (X3 - X4);

            }
            // If difference is too big:	( 3.2.6.4)
            else
            {
                xHigh[index] = _dX2 - (_dDelta * (m1 + m2) / (Math.Sqrt((m2 * m2) + (_dS * _dS)) + Math.Sqrt((m1 * m1) + (_dS * _dS))));
                if (m1 * m2 < 0)
                    yHigh[index] = _dY2 + (_dDelta * (m1 * Math.Sqrt((m2 * m2) + (_dS * _dS)) - m2 * Math.Sqrt((m1 * m1) + (_dS * _dS)))) / (m1 - m2);
                else
                    yHigh[index] = _dY2 + (_dS * _dS * _dDelta * (m1 + m2) / (m1 * Math.Sqrt((m2 * m2) + (_dS * _dS)) + m2 * Math.Sqrt((m1 * m1) + (_dS * _dS))));

                if ((((double)xHigh[index]) == ((double)xHigh[index - 1])) && (((double)yHigh[index]) != ((double)yHigh[index - 1])))
                {
                    xHigh[index] = xHigh[index - 1] + _dXMinStep;
                    yHigh[index] = _dY2 + m1 * (xHigh[index] - _dX2) + _dDelta * Math.Sqrt((m1 * m1) + (_dS * _dS));
                    _lmh[index - 1] = (yHigh[index] - yHigh[index - 1]) / _dXMinStep;
                }

                // If the juncture of the current interval is before the last saved one (3.2.6.7)
                while (((double)xHigh[index]) <= ((double)xHigh[index - 1]))
                {
                    //_bAllIntervals = false;
                    // consolidating the current and the previous interval (3.2.6.7.3)
                    _li0h.RemoveAt(index - 1);
                    _li1h.RemoveAt(index - 1);
                    _lmh.RemoveAt(index - 1);
                    xHigh.RemoveAt(index);
                    yHigh.RemoveAt(index);
                    index--;

                    // if the saved interval is the 1st interval (3.2.6.7.3.5.1)
                    if (index == 0)
                    {
                        double X3 = (double)x.GetValue(0);

                        xHigh[index] = X3 - _dDelta;
                        yHigh[index] = _dY2 + m1 * (xHigh[index] - _dX2) + _dDelta * Math.Sqrt((m1 * m1) + (_dS * _dS));
                        break;	/// SBR: wenn i==0 wollen wir nicht weitergehen, sonst exception in While Bedingung!
                    }
                    // if it is not the first:	(3.2.6.7.3.5.2.)
                    else
                    {
                        m2 = (double)_lmh[index - 1];
                        double X3 = (double)xHigh[index - 1];
                        double Y3 = (double)yHigh[index - 1];

                        xHigh[index] = (m2 * X3 - m1 * _dX2 + _dY2 - Y3 + _dDelta * Math.Sqrt((m1 * m1) + (_dS * _dS))) / (m2 - m1);
                        yHigh[index] = (m2 * m1 * (X3 - _dX2) + m2 * (_dY2 + _dDelta * Math.Sqrt((m1 * m1) + (_dS * _dS))) - m1 * Y3) / (m2 - m1);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the upper tube curve.
        /// </summary>
        /// <param name="x">x values of the reference curve.</param>
        /// <param name="y">y values of the reference curve.</param>
        /// <param name="xLow">An empty, initialized list that is to be filled with the time values for the lower tube curve</param>
        /// <param name="yLow">An empty, initialized list that is to be filled with the y values for the lower tube curve</param>
        [System.Runtime.ConstrainedExecution.ReliabilityContract(System.Runtime.ConstrainedExecution.Consistency.MayCorruptProcess, System.Runtime.ConstrainedExecution.Cer.Success)]
        private void GenerateLowTube(System.Array x, System.Array y, List<double> xLow, List<double> yLow)
        {
            int index = _lml.Count - 1; // = _li0l.Count - 1 = _li1l.Count - 1 = xLow.Count - 1 = yLow.Count - 1 > 0

            double m1 = (double)_lml[index];
            double m2;

            if (index <= 0) //Catch OutOfRangeExeception when setting m2
            {
                m2 = 0;
                index = 1;
            }
            else
                m2 = (double)_lml[index - 1];

            _dSlopeDif = Math.Abs(m1 - m2);

            if ((_dSlopeDif == 0) || ((_dSlopeDif < 2e-15 * Math.Max(Math.Abs(m1), Math.Abs(m2))) && ((int)_li0l[_li0l.Count - 1] - (int)_li1l[_li1l.Count - 2] < 100)))
            {

                _li1l.RemoveAt(index);
                _li0l.RemoveAt(index - 1);
                _lml.RemoveAt(index);
                xLow.RemoveAt(index);
                yLow.RemoveAt(index);

                double X3 = (double)x.GetValue((int)_li0l[index - 1]);  // = _dX1
                double Y3 = (double)y.GetValue((int)_li0l[index - 1]);  // = _dY1
                double X4 = (double)x.GetValue((int)_li1l[index - 1]);  // < X3
                double Y4 = (double)y.GetValue((int)_li1l[index - 1]);

                _lml[index - 1] = (Y3 - Y4) / (X3 - X4);
            }
            else
            {
                xLow[index] = _dX2 + (_dDelta * (m1 + m2) / (Math.Sqrt((m2 * m2) + (_dS * _dS)) + Math.Sqrt((m1 * m1) + (_dS * _dS))));
                if (m1 * m2 < 0)
                    yLow[index] = _dY2 - (_dDelta * (m1 * Math.Sqrt((m2 * m2) + (_dS * _dS)) - m2 * Math.Sqrt((m1 * m1) + (_dS * _dS)))) / (m1 - m2);
                else
                    yLow[index] = _dY2 - (_dS * _dS * _dDelta * (m1 + m2) / (m1 * Math.Sqrt((m2 * m2) + (_dS * _dS)) + m2 * Math.Sqrt((m1 * m1) + (_dS * _dS))));

                if ((((double)xLow[index]) == ((double)xLow[index - 1])) && (((double)yLow[index]) != ((double)yLow[index - 1])))
                {
                    xLow[index] = xLow[index - 1] + _dXMinStep;
                    yLow[index] = _dY2 + m1 * (xLow[index] - _dX2) - _dDelta * Math.Sqrt((m1 * m1) + (_dS * _dS));
                    _lml[index - 1] = (yLow[index] - yLow[index - 1]) / _dXMinStep;
                }

                while (index > 0 && ((double)xLow[index]) <= ((double)xLow[index - 1]))
                {
                    //_bAllIntervals = false;
                    _li0l.RemoveAt(index - 1);
                    _li1l.RemoveAt(index - 1);
                    _lml.RemoveAt(index - 1);
                    xLow.RemoveAt(index);
                    yLow.RemoveAt(index);
                    index--;

                    if (index == 0)
                    {
                        double X3 = (double)x.GetValue(0);

                        xLow[index] = X3 - _dDelta;
                        yLow[index] = _dY2 + m1 * (xLow[index] - _dX2) - _dDelta * Math.Sqrt((m1 * m1) + (_dS * _dS));
                    }
                    else
                    {
                        m2 = (double)_lml[index - 1];
                        double X3 = (double)xLow[index - 1];
                        double Y3 = (double)yLow[index - 1];

                        xLow[index] = (m2 * X3 - m1 * _dX2 + _dY2 - Y3 - _dDelta * Math.Sqrt((m1 * m1) + (_dS * _dS))) / (m2 - m1);
                        yLow[index] = (m2 * m1 * (X3 - _dX2) + m2 * (_dY2 - _dDelta * Math.Sqrt((m1 * m1) + (_dS * _dS))) - m1 * Y3) / (m2 - m1);
                    }
                }
            }
        }
    }
}
