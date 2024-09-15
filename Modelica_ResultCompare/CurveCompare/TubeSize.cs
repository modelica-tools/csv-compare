// TubeSize.cs
// author: Susanne Walther
// date: 2.12.2014

using System;
using System.Linq;

namespace CurveCompare
{
    /// <summary>
    /// The Class represents measures for the tube size in x- and y-direction: <para>
    /// X (Width), Y (Height). <para/>
    /// The class provides methods for conversion from relative in absolute measures and for calculation of width from height. </para>
    /// </summary>
    public class TubeSize
    {
        private double x, y, baseX, baseY, ratio;
        private Curve reference;
        bool successful;

        /// <summary>
        /// X = Absolute Width = 1/2 Rectangle Width
        /// </summary>
        public double X
        {
            get { return x; }
            set { x = value; }
        }
        /// <summary>
        /// Y = Absolute Height = 1/2 Rectangle Height
        /// </summary>
        public double Y
        {
            get { return y; }
            set { y = value; }
        }
        /// <summary>
        /// Base of relative values in x direction.
        /// </summary>
        public double BaseX
        {
            get { return baseX; }
            set { baseX = value; }
        }
        /// <summary>
        /// Base of relative values in y direction.
        /// </summary>
        public double BaseY
        {
            get { return baseY; }
            set { baseY = value; }
        }
        /// <summary>
        /// Ratio = Y / X
        /// </summary>
        public double Ratio
        {
            get { return ratio; }
            set { ratio = value; }
        }
        /// <summary>
        /// true, if calculation of X and Y successful; <para>
        /// false, if calculation fails.</para>
        /// </summary>
        public bool Successful
        {
            get { return successful; }
            set { successful = value; }
        }
        /// <summary>
        /// Creates an instance of TubeSize.
        /// </summary>
        /// <param name="reference">Reference curve.</param>
        /// <param name="nominalValue">A nominal value handles the case of reference variables that are near-zero
        /// e.g. because of being the result of balance equations affected by small numerical errors, but are meant
        /// to have a much larger order of magnitude.</param>
        /// <param name="formerBaseAndRatio">Base and Ratio are calculated like in the former CSV-Compare, if true;<param>
        public TubeSize(Curve reference, double nominalValue, bool formerBaseAndRatio)
        {
            this.reference = reference;
            if (formerBaseAndRatio)
                SetFormerBaseAndRatio(nominalValue);
            else
                SetStandardBaseAndRatio(nominalValue);
            successful = false;
        }
        /// <summary>
        /// Calculates standard values for BaseX , BaseY and Ratio.
        /// </summary>
        /// <param name="nominalValue">Nominal value (required: greater than zero).</param>
        private void SetStandardBaseAndRatio(double nominalValue)
        {
            // set baseX
            baseX = reference.X.Max() - reference.X.Min(); //reference.X.Max() - reference.X.Min() + Math.Abs(reference.X.Min());
            if (baseX == 0) // nonsense case, no data
                baseX = Math.Abs(reference.X.Max());
            if (baseX == 0) // nonsense case, no data
                baseX = 1;
            // set baseY
            baseY = Math.Max(reference.Y.Max() - reference.Y.Min(), nominalValue);
            // set ratio
            if (baseX != 0)
                ratio = baseY / baseX;
            else
                ratio = 0;
        }
        /// <summary>
        /// Calculates former standard values for BaseX , BaseY and Ratio.
        /// </summary>
        /// <param name="nominalValue">Nominal value (required: greater than zero).</param>
        private void SetFormerBaseAndRatio(double nominalValue)
        {
            // guard against the case with only one time point, for which baseX would be zero
            const double epsilon = 1e-12;
            baseX = Math.Max(Math.Max(reference.X.Max() - reference.X.Min(), Math.Abs(reference.X.Min())), epsilon);
            baseY = Math.Max(Math.Max(reference.Y.Max() - reference.Y.Min(), Math.Abs(reference.Y.Min())), nominalValue);
            ratio = baseY / baseX;
            return;
        }
        /// <summary>
        /// Calculates or sets X and Y.
        /// </summary>
        /// <param name="value">Value.</param>
        /// <param name="axes">States, if value is x (half width of rectangle) or y (half height of rectangle).</param>
        /// <param name="relativity">States, if value is relative or absolute. </param>
        /// <remarks>If value is relative, value have to be in [0,1]. <para>
        /// Calculation fails, if (Ratio = 0 or BaseX = 0 or BaseY = 0).<para/>
        /// If calculation fails, [set Ratio and BaseX and BaseY != 0] or [call Calculate(double x, double y, Relativity relativity) with parameter Relativity.Absolute]</para></remarks>
        /// <exception cref="ArgumentOutOfRangeException">Relative value is out of expected range [0,1].</exception>
        public void Calculate(double value, Axes axes, Relativity relativity)
        {
            successful = false;

            if (ratio > 0)
            {
                if (relativity == Relativity.Relative)
                {
                    if ((value < 0) || (value > 1))
                        throw new ArgumentOutOfRangeException("Relative value is out of expected range [0,1].");

                    if (axes == Axes.Y && baseY > 0)
                    {
                        y = value * baseY;
                        x = y / ratio;
                        successful = true;
                    }
                    else if (axes == Axes.X && baseX > 0)
                    {
                        x = value * baseX;
                        y = ratio * x;
                        successful = true;
                    }
                }
                else if (relativity == Relativity.Absolute)
                {
                    if (axes == Axes.Y)
                    {
                        this.y = value;
                        this.x = value / ratio;
                    }
                    else if (axes == Axes.X)
                    {
                        this.x = value;
                        this.y = value * ratio;
                    }
                    successful = true;
                }
            }
        }
        /// <summary>
        /// Calculates or sets X and Y.
        /// </summary>
        /// <param name="x">x value. Half width of rectangle.</param>
        /// <param name="y">y value. Half height of rectangle.</param>
        /// <param name="relativity">States, if value is relative or absolute.</param>
        /// <remarks>If values are relative, values have to be in [0,1].<para>
        /// Calculation fails, if (BaseX = 0 or BaseY = 0).<para/>
        /// If calculation fails, [call Calculate with parameter Relativity.Absolute] or [set BaseX and BaseY != 0]</para></remarks>
        /// <exception cref="ArgumentOutOfRangeException">Relative value is out of expected range [0,1].</exception>
        public void Calculate(double x, double y, Relativity relativity)
        {
            successful = false;

            if (relativity == Relativity.Relative && baseX != 0 && baseY != 0)
            {
                if ((y < 0) || (y > 1))
                    throw new ArgumentOutOfRangeException("Relative y value is out of expected range [0,1].");
                if ((x < 0) || (x > 1))
                    throw new ArgumentOutOfRangeException("Relative x value is out of expected range [0,1].");

                this.x = x * baseX;
                this.y = y * baseY;
                successful = true;
            }
            else if (relativity == Relativity.Absolute)
            {
                this.x = x;
                this.y = y;
                successful = true;
            }
        }
    }
    /// <summary>
    /// Option for TubeSize.
    /// </summary>
    public enum Axes
    {
        X,
        Y
    };
    /// <summary>
    /// Option for TubeSize.
    /// </summary>
    public enum Relativity
    {
        Relative,
        Absolute
    };
}
