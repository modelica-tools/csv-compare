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

        private int _iRecursions = 0;
        private int _iRecursionLimit = 500; //maximal deepness of recursion -> works around stack overflows
        private bool _bBigAngle; // true if angle is bigger than 180°

        #endregion

        public static bool RelativeErrors = true;

        /// This method generates the upper curve of the tube.
        /// 
        /// @para x The time values for the base curve
        /// @para y The y values of the base curve
        /// @para x_high An empty, initialized list that is to be filled with the time values for the upper tube curve
        /// @para y_high An empty, initialized list that is to be filled with the y values for the upper tube curve
        /// @para index
        /// @param m1 slope before
        /// @param m2 slope after
        /// @param i Counter
        /// @param Value 
        [System.Runtime.ConstrainedExecution.ReliabilityContract(System.Runtime.ConstrainedExecution.Consistency.MayCorruptProcess, System.Runtime.ConstrainedExecution.Cer.Success)]
        private void GenerateHighTube(System.Array x, System.Array y, List<double> xHigh, List<double> yHigh, int index, double m1, double m2, int i, bool value)
        {
            _iRecursions++;
            if (_iRecursions > _iRecursionLimit)
                return; // Stop recursion to prevent stanck overflow

            _dSlopeDif = Math.Abs(m1 - m2);		// (3.2.6.2)

            if ((_dSlopeDif == 0) || ((_dSlopeDif < 2e-15 * Math.Max(Math.Abs(m1), Math.Abs(m2))) && (i - (int)_li1h[_li1h.Count - 1] < 100)))
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
                double X3 = (double)x.GetValue((int)_li0h[index - 1]);
                double Y3 = (double)y.GetValue((int)_li0h[index - 1]);
                double X4 = (double)x.GetValue((int)_li1h[index - 1]);
                double Y4 = (double)y.GetValue((int)_li1h[index - 1]);

                // write slope to the list of slopes
                if (X3 == X4)
                    _lmh[index - 1] = (Y3 - Y4);
                else
                    _lmh[index - 1] = (Y3 - Y4) / (X3 - X4);

            }
            // If difference is too big:	( 3.2.6.4)
            else
            {
                int iAct = index;
                // if pass relates to different values: (3.2.6.7.3.5.2.1)
                if (value == true)
                {
                    _dX2 = (double)x.GetValue((int)_li1h[index]);
                    _dY2 = (double)y.GetValue((int)_li1h[index]);
                }

                xHigh[index] = _dX2 - (_dDelta * (m1 + m2) / (Math.Sqrt((m2 * m2) + (_dS * _dS)) + Math.Sqrt((m1 * m1) + (_dS * _dS))));
                if (m1 * m2 < 0)
                    yHigh[index] = _dY2 + (_dDelta * (m1 * Math.Sqrt((m2 * m2) + (_dS * _dS)) - m2 * Math.Sqrt((m1 * m1) + (_dS * _dS)))) / (m1 - m2);
                else
                    yHigh[index] = _dY2 + (_dS * _dS * _dDelta * (m1 + m2) / (m1 * Math.Sqrt((m2 * m2) + (_dS * _dS)) + m2 * Math.Sqrt((m1 * m1) + (_dS * _dS))));

                // If teh juncture of the current iterval is bevore the last saved one (3.2.6.7)
                if (((double)xHigh[index]) <= ((double)xHigh[index - 1]))
                {
                    // If the angle is smaller than 180: (3.2.6.7.1)
                    if (m2 < m1)
                    {
                        // consolidating the current and the previous interval (3.2.6.7.3)
                        _li0h.RemoveAt(index - 1);
                        _li1h.RemoveAt(index);
                        _lmh.RemoveAt(index);
                        xHigh.RemoveAt(index);
                        yHigh.RemoveAt(index);

                        // if the saved interval is the 1st interval (3.2.6.7.3.5.1)
                        if ((int)_li1h[index - 1] == 0)
                        {
                            double X3 = (double)x.GetValue(0);
                            double Y3 = (double)y.GetValue(0);
                            double X4 = (double)x.GetValue((int)_li0h[index - 1]);
                            double Y4 = (double)y.GetValue((int)_li0h[index - 1]);

                            if (X4 == X3)
                                m1 = (Y4 - Y3);
                            else
                                m1 = (Y4 - Y3) / (X4 - X3);

                            _lmh[index - 1] = m1;
                            xHigh[index - 1] = X3 - _dDelta;
                            yHigh[index - 1] = Y3 - m1 * _dDelta + _dS * _dDelta * Math.Sqrt((m1 * m1) / (_dS * _dS) + 1);
                        }

                            // if it is not the first:	(3.2.6.7.3.5.2.)
                        else
                        {
                            double X3 = (double)x.GetValue((int)_li1h[index - 1]);
                            double Y3 = (double)y.GetValue((int)_li1h[index - 1]);
                            double X4 = (double)x.GetValue((int)_li0h[index - 1]);
                            double Y4 = (double)y.GetValue((int)_li0h[index - 1]);

                            if (X4 == X3)
                                _lmh[index - 1] = (Y4 - Y3);
                            else
                                _lmh[index - 1] = (Y4 - Y3) / (X4 - X3);

                            GenerateHighTube(x, y, xHigh, yHigh, index - 1, (double)_lmh[index - 1], (double)_lmh[index - 2], i, true);

                        }
                        iAct = index - 1;
                    }
                    // the angle is bigger than 180:
                    else if (m2 > m1)	// (3.2.6.7.2)	check slope
                    {
                        _bBigAngle = true;
                        // the saved and the previously saved interval are consolidated

                        _li0h.RemoveAt(index - 2);
                        _li1h.RemoveAt(index - 1);
                        _lmh.RemoveAt(index - 1);
                        xHigh.RemoveAt(index - 1);
                        yHigh.RemoveAt(index - 1);

                        // If it is the first interval
                        if ((int)_li1h[index - 2] == 0)
                        {
                            double X3 = (double)x.GetValue(0);
                            double Y3 = (double)y.GetValue(0);
                            double X4 = (double)x.GetValue((int)_li0h[index - 2]);
                            double Y4 = (double)y.GetValue((int)_li0h[index - 2]);

                            if (X4 == X3)
                                m1 = (Y4 - Y3);
                            else
                                m1 = (Y4 - Y3) / (X4 - X3);

                            _lmh[index - 2] = m1;

                            xHigh[index - 2] = X3 - _dDelta;
                            yHigh[index - 2] = Y3 - m1 * _dDelta + _dS * _dDelta * Math.Sqrt((m1 * m1) / (_dS * _dS) + 1);
                        }
                        else
                        {
                            double X3 = (double)x.GetValue((int)_li1h[index - 2]);
                            double Y3 = (double)y.GetValue((int)_li1h[index - 2]);
                            double X4 = (double)x.GetValue((int)_li0h[index - 2]);
                            double Y4 = (double)y.GetValue((int)_li0h[index - 2]);

                            if (X4 == X3)
                                _lmh[index - 2] = (Y4 - Y3);
                            else
                                _lmh[index - 2] = (Y4 - Y3) / (X4 - X3);

                            // go to 3.2.6.2 for this and the previous interval
                            GenerateHighTube(x, y, xHigh, yHigh, index - 2, (double)_lmh[index - 2], (double)_lmh[index - 3], i, true);
                        }
                        iAct = index - 2;
                    }
                }
                while (_bBigAngle == true)
                {
                    if (iAct >= xHigh.Count - 1)
                        _bBigAngle = false;
                    else
                        GenerateHighTube(x, y, xHigh, yHigh, iAct + 1, (double)_lmh[iAct + 1], (double)_lmh[iAct], i, true);
                }

            }
        }

        /// This method generates the lower curve of the tube.
        /// 
        /// @para x The time values for the base curve
        /// @para y The y values of the base curve
        /// @para x_low An empty, initialized list that is to be filled with the time values for the lower tube curve
        /// @para y_low An empty, initialized list that is to be filled with the y values for the lower tube curve
        /// @para index
        /// @param m1 slope before
        /// @param m2 slope after
        /// @param i Counter
        /// @param Value
        /// @see documentation of the algorithm inside the GenerateHighTube method
        [System.Runtime.ConstrainedExecution.ReliabilityContract(System.Runtime.ConstrainedExecution.Consistency.MayCorruptProcess, System.Runtime.ConstrainedExecution.Cer.Success)]
        private void GenerateLowTube(System.Array x, System.Array y, List<double> xLow, List<double> yLow, int index, double m1, double m2, int i, bool value)
        {
            _iRecursions++;
            if (_iRecursions > _iRecursionLimit)
                return; // Stop recursion to prevent stanck overflow

            _dSlopeDif = Math.Abs(m1 - m2);

            if ((_dSlopeDif == 0) || ((_dSlopeDif < 2e-15 * Math.Max(Math.Abs(m1), Math.Abs(m2))) && (i - (int)_li1l[_li1l.Count - 1] < 100)))
            {
                _li1l.RemoveAt(index);
                _li0l.RemoveAt(index - 1);
                _lml.RemoveAt(index);
                xLow.RemoveAt(index);
                yLow.RemoveAt(index);

                double X3 = (double)x.GetValue((int)_li0l[index - 1]);
                double Y3 = (double)y.GetValue((int)_li0l[index - 1]);
                double X4 = (double)x.GetValue((int)_li1l[index - 1]);
                double Y4 = (double)y.GetValue((int)_li1l[index - 1]);

                if (X3 == X4)
                    _lml[index - 1] = (Y3 - Y4);
                else
                    _lml[index - 1] = (Y3 - Y4) / (X3 - X4);
            }
            else
            {
                int iAct = index;
                if (value == true)
                {
                    _dX2 = (double)x.GetValue((int)_li1l[index]);
                    _dY2 = (double)y.GetValue((int)_li1l[index]);
                }

                xLow[index] = _dX2 + (_dDelta * (m1 + m2) / (Math.Sqrt((m2 * m2) + (_dS * _dS)) + Math.Sqrt((m1 * m1) + (_dS * _dS))));
                if (m1 * m2 < 0)
                    yLow[index] = _dY2 - (_dDelta * (m1 * Math.Sqrt((m2 * m2) + (_dS * _dS)) - m2 * Math.Sqrt((m1 * m1) + (_dS * _dS)))) / (m1 - m2);
                else
                    yLow[index] = _dY2 - (_dS * _dS * _dDelta * (m1 + m2) / (m1 * Math.Sqrt((m2 * m2) + (_dS * _dS)) + m2 * Math.Sqrt((m1 * m1) + (_dS * _dS))));

                if (((double)xLow[index]) <= ((double)xLow[index - 1]))
                {
                    if (m2 > m1)
                    {
                        _li0l.RemoveAt(index - 1);
                        _li1l.RemoveAt(index);
                        _lml.RemoveAt(index);
                        xLow.RemoveAt(index);
                        yLow.RemoveAt(index);

                        if ((int)_li1l[index - 1] == 0)
                        {
                            double X3 = (double)x.GetValue(0);
                            double Y3 = (double)y.GetValue(0);
                            double X4 = (double)x.GetValue((int)_li0l[index - 1]);
                            double Y4 = (double)y.GetValue((int)_li0l[index - 1]);

                            m1 = (Y4 - Y3) / (X4 - X3);

                            _lml[index - 1] = m1;
                            xLow[index - 1] = X3 - _dDelta;
                            yLow[index - 1] = Y3 - m1 * _dDelta - _dS * _dDelta * Math.Sqrt((m1 * m1) / (_dS * _dS) + 1);
                        }
                        else
                        {
                            double X3 = (double)x.GetValue((int)_li1l[index - 1]);
                            double Y3 = (double)y.GetValue((int)_li1l[index - 1]);
                            double X4 = (double)x.GetValue((int)_li0l[index - 1]);
                            double Y4 = (double)y.GetValue((int)_li0l[index - 1]);

                            if (X4 == X3) 
                                _lml[index - 1] = (Y4 - Y3);
                            else
                                _lml[index - 1] = (Y4 - Y3) / (X4 - X3);

                            GenerateLowTube(x, y, xLow, yLow, index - 1, (double)_lml[index - 1], (double)_lml[index - 2], i, true);
                        }
                        iAct = index - 1;
                    }
                    else if (m2 < m1)
                    {
                        _bBigAngle = true;

                        _li0l.RemoveAt(index - 2);
                        _li1l.RemoveAt(index - 1);
                        _lml.RemoveAt(index - 1);
                        xLow.RemoveAt(index - 1);
                        yLow.RemoveAt(index - 1);

                        if ((int)_li1l[index - 2] == 0)
                        {
                            double X3 = (double)x.GetValue(0);
                            double Y3 = (double)y.GetValue(0);
                            double X4 = (double)x.GetValue((int)_li0l[index - 2]);
                            double Y4 = (double)y.GetValue((int)_li0l[index - 2]);

                            m1 = (Y4 - Y3) / (X4 - X3);

                            _lml[index - 2] = m1;

                            xLow[index - 2] = X3 - _dDelta;
                            yLow[index - 2] = Y3 - m1 * _dDelta - _dS * _dDelta * Math.Sqrt((m1 * m1) / (_dS * _dS) + 1);
                        }
                        else
                        {
                            double X3 = (double)x.GetValue((int)_li1l[index - 2]);
                            double Y3 = (double)y.GetValue((int)_li1l[index - 2]);
                            double X4 = (double)x.GetValue((int)_li0l[index - 2]);
                            double Y4 = (double)y.GetValue((int)_li0l[index - 2]);

                            if (X4 == X3)
                                _lml[index - 2] = (Y4 - Y3);
                            else
                                _lml[index - 2] = (Y4 - Y3) / (X4 - X3);

                            GenerateLowTube(x, y, xLow, yLow, index - 2, (double)_lml[index - 2], (double)_lml[index - 3], i, true);
                        }
                        iAct = index - 2;
                    }
                }

                while (_bBigAngle == true)
                {
                    if (iAct >= xLow.Count - 1)
                        _bBigAngle = false;
                    else
                        GenerateLowTube(x, y, xLow, yLow, iAct + 1, (double)_lml[iAct + 1], (double)_lml[iAct], i, true);
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
        /// <param name="number"></param>
        public void CalculateTubes(System.Array x, System.Array y, List<double> xHigh, List<double> yHigh, List<double> xLow, List<double> yLow, double r, int number)
        {
            // set tStart and tStop
            _dtStart = (double)x.GetValue(0);
            _dtStop = (double)x.GetValue(x.Length - 1 - number);

            double mJumP = 1; // slope for jumps

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

            for (int i = 1; i < x.Length - number; i++)
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
            _dS = Math.Abs(4 * (max - min) / (Math.Abs(_dtStop - _dtStart)));

            if (_dS < 0.0004 / (Math.Abs(_dtStop - _dtStart)))
            {
                _dS = 0.0004 / (Math.Abs(_dtStop - _dtStart));
            }

            // Begin calculation for the tubes
            for (int i = 1; i < x.Length - number; i++)
            {
                // get current value
                _dX1 = (double)x.GetValue(i);
                _dY1 = (double)y.GetValue(i);

                // get previous value
                _dX2 = (double)x.GetValue(i - 1);
                _dY2 = (double)y.GetValue(i - 1);

                // catch jumps
                if (_dX1 == _dX2)
                {
                    if (_dY1 - _dY2 < 0)
                        _dCurrentSlope = mJumP * -1;
                    else if (_dY1 - _dY2 == 0)
                        _dCurrentSlope = 0;
                    else
                        _dCurrentSlope = mJumP;
                    //double dx1, dx2;
                    //if (i < 1)
                    //{
                    //    dx1 = 0;
                    //    dx2 = (double)x.GetValue(i + 1) + (((double)x.GetValue(i + 1) + X2) / 5);
                    //}
                    //else if (i > x.Length + 1)
                    //{
                    //    dx1 = (double)x.GetValue(i - 1) + ((X1 -(double)x.GetValue(i + 1)) / 5);
                    //    dx2 = 0;
                    //}
                    //else
                    //{
                    //    dx1 = (double)x.GetValue(i - 1) - ((X1 - (double)x.GetValue(i - 1)) / 10);
                    //    dx2 = (double)x.GetValue(i + 1) + (((double)x.GetValue(i + 1) + X2) / 10);
                    //}
                }
                else
                    _dCurrentSlope = (_dY1 - _dY2) / (_dX1 - _dX2); // calculate current slope ( 3.2.6.1)               

                if (i == 1) // 1st interval (3.2.5)
                {
                    // initial values upper tube

                    xHigh.Add(_dX2 - _dDelta);
                    yHigh.Add(_dY2 - _dCurrentSlope * _dDelta + _dS * _dDelta * Math.Sqrt((_dCurrentSlope * _dCurrentSlope) / (_dS * _dS) + 1));

                    _li0h.Add(i);
                    _li1h.Add(i - 1);
                    _lmh.Add(_dCurrentSlope);

                    // initial values lower tube
                    xLow.Add(_dX2 - _dDelta);
                    yLow.Add(_dY2 - _dCurrentSlope * _dDelta - _dS * _dDelta * Math.Sqrt((_dCurrentSlope * _dCurrentSlope) / (_dS * _dS) + 1));

                    _li0l.Add(i);
                    _li1l.Add(i - 1);
                    _lml.Add(_dCurrentSlope);
                }
                else	// if not 1st interval (3.2.6)
                {
                    // fill lists with new values, set X and Y to arbitrary value (3.2.6.1)
                    _li0h.Add(i);
                    _li1h.Add(i - 1);
                    _lmh.Add(_dCurrentSlope);

                    xHigh.Add(1);
                    yHigh.Add(1);

                    _li0l.Add(i);
                    _li1l.Add(i - 1);
                    _lml.Add(_dCurrentSlope);

                    xLow.Add(1);
                    yLow.Add(1);

                    _iRecursions = 0;

                    // begin procedure for upper tube
                    GenerateHighTube(x, y, xHigh, yHigh, _li1h.Count - 1, _dCurrentSlope, (double)_lmh[_li1h.Count - 2], i, false);

                    // get current value from base curve
                    _dX1 = (double)x.GetValue(i);
                    _dY1 = (double)y.GetValue(i);

                    // get previous value from base curve
                    _dX2 = (double)x.GetValue(i - 1);
                    _dY2 = (double)y.GetValue(i - 1);

                    // catch jump
                    if (_dX1 == _dX2)
                        if (_dY1 - _dY2 < 0)
                            _dCurrentSlope = mJumP * -1;
                        else if (_dY1 - _dY2 == 0)
                            _dCurrentSlope = 0;
                        else
                            _dCurrentSlope = mJumP;
                    else
                        _dCurrentSlope = (_dY1 - _dY2) / (_dX1 - _dX2); // calculate current slope ( 3.2.6.1)                   

                    _iRecursions = 0;
                    // begin procedure for lower tube
                    GenerateLowTube(x, y, xLow, yLow, _li1l.Count - 1, _dCurrentSlope, (double)_lml[_li1l.Count - 2], i, false);
                }

            }

            // calculate terminal value
            // upper tube
            _dX1 = (double)x.GetValue((int)_li1h[_li1h.Count - 1]);
            _dY1 = (double)y.GetValue((int)_li1h[_li1h.Count - 1]);
            _dX2 = (double)x.GetValue((int)_li0h[_li0h.Count - 1]);
            _dY2 = (double)y.GetValue((int)_li0h[_li0h.Count - 1]);

            _dCurrentSlope = (_dY2 - _dY1) / (_dX2 - _dX1);

            xHigh.Add(_dX2 + _dDelta);
            yHigh.Add(_dY2 + _dCurrentSlope * _dDelta + _dS * _dDelta * Math.Sqrt((_dCurrentSlope * _dCurrentSlope) / (_dS * _dS) + 1));

            // lower tube
            _dX1 = (double)x.GetValue((int)_li1l[_li1l.Count - 1]);
            _dY1 = (double)y.GetValue((int)_li1l[_li1l.Count - 1]);
            _dX2 = (double)x.GetValue((int)_li0l[_li0l.Count - 1]);
            _dY2 = (double)y.GetValue((int)_li0l[_li0l.Count - 1]);

            _dCurrentSlope = (_dY2 - _dY1) / (_dX2 - _dX1);

            xLow.Add(_dX2 + _dDelta);
            yLow.Add(_dY2 + _dCurrentSlope * _dDelta - _dS * _dDelta * Math.Sqrt((_dCurrentSlope * _dCurrentSlope) / (_dS * _dS) + 1));
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

            for (int i = 2; i < comp.Count - 1 && i < refHigh.Count - 1 && i < refLow.Count - 1; i++)
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