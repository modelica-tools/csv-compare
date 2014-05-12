using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace CsvCompare
{
    /// Calculates a tube around the given characteristic curve
    public class Range
    {
        #region private members
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

        #endregion

        public static bool RelativeErrors = true;

        /// This method generates the upper curve of the tube.
        /// 
        /// @para x The time values for the base curve
        /// @para y The y values of the base curve
        /// @para x_high An empty, initialized list that is to be filled with the time values for the upper tube curve
        /// @para y_high An empty, initialized list that is to be filled with the y values for the upper tube curve
        [System.Runtime.ConstrainedExecution.ReliabilityContract(System.Runtime.ConstrainedExecution.Consistency.MayCorruptProcess, System.Runtime.ConstrainedExecution.Cer.Success)]
        private void GenerateHighTube(System.Array x, System.Array y, List<double> xHigh, List<double> yHigh)
        {
            int index = _lmh.Count - 1; // = _li0h.Count - 1 = _li1h.Count - 1 = xHigh.Count - 1 = yHigh.Count - 1 > 0

            double m1 = (double)_lmh[index];
            double m2 = (double)_lmh[index - 1];
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

        /// This method generates the lower curve of the tube.
        /// 
        /// @para x The time values for the base curve
        /// @para y The y values of the base curve
        /// @para x_low An empty, initialized list that is to be filled with the time values for the lower tube curve
        /// @para y_low An empty, initialized list that is to be filled with the y values for the lower tube curve
        /// @see documentation of the algorithm inside the GenerateHighTube method
        [System.Runtime.ConstrainedExecution.ReliabilityContract(System.Runtime.ConstrainedExecution.Consistency.MayCorruptProcess, System.Runtime.ConstrainedExecution.Cer.Success)]
        private void GenerateLowTube(System.Array x, System.Array y, List<double> xLow, List<double> yLow)
        {
            int index = _lml.Count - 1; // = _li0l.Count - 1 = _li1l.Count - 1 = xLow.Count - 1 = yLow.Count - 1 > 0
            double m1 = (double)_lml[index];
            double m2 = (double)_lml[index - 1];
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

                while (((double)xLow[index]) <= ((double)xLow[index - 1]))
                {
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

        ///This method generates tubes around a given curve
        /// @para x The time values for the base curve
        /// @para y The y values of the base curve
        /// @para x_low An empty, initialized list that is to be filled with the time values for the lower tube curve
        /// @para y_low An empty, initialized list that is to be filled with the y values for the lower tube curve
        /// @para x_high An empty, initialized list that is to be filled with the time values for the upper tube curve
        /// @para y_high An empty, initialized list that is to be filled with the y values for the upper tube curve
        /// @para r
        public void CalculateTubes(System.Array x, System.Array y, List<double> xHigh, List<double> yHigh, List<double> xLow, List<double> yLow, double r)
        {
            // set tStart and tStop
            _dtStart = (double)x.GetValue(0);
            _dtStop = (double)x.GetValue(x.Length - 1);
            _dXMinStep = ((_dtStop - _dtStart) + Math.Abs(_dtStart)) * _dXRelEps;

            // Initialize lists (upper tube)
            _lmh = new ArrayList();	// slope in i
            _li0h = new ArrayList();	// i
            _li1h = new ArrayList();	// i - 1

            // Initialize lists (lower tube)
            _lml = new ArrayList();		// slope in i
            _li0l = new ArrayList();	// i
            _li1l = new ArrayList();	// i-1

            // calculate the tubes delta
            _dDelta = r * (_dtStop - _dtStart);

            // calculate S:
            double max = (double)y.GetValue(0);
            double min = (double)y.GetValue(0);

            for (int i = 1; i < x.Length; i++)
            {
                double Y = (double)y.GetValue(i);
                if (Y > max)
                {
                    max = Y;
                }
                if (Y < min)
                {
                    min = Y;
                }
            }
//            _dS = Math.Abs(4 * (max - min) / (Math.Abs(_dtStop - _dtStart)));
            _dS = Math.Abs(((max - min) + Math.Abs(min)) / (Math.Abs(_dtStop - _dtStart)));
            
            if (_dS < 0.0004 / (Math.Abs(_dtStop - _dtStart)))
            {
                _dS = 0.0004 / (Math.Abs(_dtStop - _dtStart));
            }

            bool bJump = false;

            // Begin calculation for the tubes
            for (int i = 1; i < x.Length; i++)
            {
                // get current value
                _dX1 = (double)x.GetValue(i);
                _dY1 = (double)y.GetValue(i);

                // get previous value
                _dX2 = (double)x.GetValue(i - 1);
                _dY2 = (double)y.GetValue(i - 1);

                // catch jumps
                bJump = false;
                if ((_dX1 <= _dX2) && (_dY1 == _dY2) && (xHigh.Count == 0))
                    continue;
                if ((_dX1 <= _dX2) && (_dY1 == _dY2))
                {
                    _dX1 = Math.Max((double)x.GetValue((int)_li1l[_li1l.Count - 1]) + _dXMinStep, Math.Max((double)x.GetValue((int)_li1h[_li1h.Count - 1]) + _dXMinStep, _dX1));
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
        }

        ///  This method validates the values of the given compare curve to be inside the lower and upper tube.
        /// 
        /// @para refLow Array with y values of lower tube
        /// @param refHigh Array with y values of lower tube
        /// @para comp Array with values to be validated
        /// @returns number of errors found
        public static int Validate(List<double> refLow, List<double> refHigh, List<double> comp, List<double> time, ref List<double> errorsTime, ref List<double> errorsDif)
        {
            int iErrors = 0;

            for (int i = 0; i < comp.Count && i < refHigh.Count && i < refLow.Count; i++)
            {
                errorsTime.Add(time[i]);

                if (comp[i] < refLow[i] || comp[i] > refHigh[i])
                {
                    iErrors++; //Count as error if current value is bigger than value of high tube and smaller than value of lowtube

                    if (RelativeErrors)// Show relative error difference (unset via commandline)
                    {
                        try
                        {
                            if (comp[i] < refLow[i])
                                errorsDif.Add(Math.Abs(refLow[i] - comp[i]));
                            else
                                errorsDif.Add(Math.Abs(refHigh[i] + comp[i]));
                        }
                        catch (Exception) { errorsDif.Add(1); } // should never happen, just in case something goes wrong
                    }
                    else
                        errorsDif.Add(1);
                }
                else
                    errorsDif.Add(0);
            }

            return iErrors;
        }

        /// This method calibrates the given timeline and its values to a target timeline
        /// @para SourceTimeLine timeline of the source values
        /// @para TargetTimeLine timeline of the target values
        /// @para SourceValues array with the source values
        /// @returns array containing the interpolated target values
        public static double[] CalibrateValues(double[] sourceTimeLine, double[] targetTimeLine, double[] sourceValues)
        {
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

                while (x1 < x && j + 1 < sourceTimeLine.Length && j + 1 < sourceValues.Length)// step source timline to the current moment
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
    }
}