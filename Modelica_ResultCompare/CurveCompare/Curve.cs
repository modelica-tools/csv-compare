// Curve.cs
// author: Susanne Walther
// date: 4.12.2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CurveCompare.DataImport;

namespace CurveCompare
{
    /// <summary>
    /// The class represents a curve. Provides information about that curve.
    /// </summary>
    public class Curve
    {
        private double[] x, y;
        private int count;
        private bool importSuccessful;

        /// <summary>
        /// Name of the curve.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// List of x-values of points (x,y)
        /// </summary>
        public double[] X
        {
            get { return x; }
        }
        /// <summary>
        /// List of y-values of points (x,y)
        /// </summary>
        public double[] Y
        {
            get { return y; }
        }
        /// <summary>
        /// = X.Length = Y.Length
        /// </summary>
        public int Count
        {
            get { return count; }
        }
        /// <summary>
        /// true, if members of class Curve are filled successfully and <para>
        /// (X != null &amp;&amp; Y != null &amp;&amp; X.Length == Y.Length == Count > 0)<para/>
        /// false, otherwise.</para>
        /// </summary>
        public bool ImportSuccessful
        {
            get { return importSuccessful; }
        }
        /// <summary>
        /// Creates an empty Curve with members = null.
        /// </summary>
        public Curve()
        {
            importSuccessful = false;
        }
        /// <summary>
        /// Creates a Curve.
        /// </summary>
        /// <param name="name">Name of the curve.</param>
        /// <param name="x">List of x-values.</param>
        /// <param name="y">List of y-values.</param>
        public Curve(string name, double[] x, double[] y)
        {
            importSuccessful = false;
            if (x != null && y != null)
            {
                if (x.Length > 0 && y.Length > 0)
                {
                    this.Name = name;

                    // Error treatment: If arrays have different Length, trim the longer array. 
                    int diff = x.Length - y.Length;
                    if (diff > 0)
                        Array.Resize<double>(ref x, y.Length);
                    else if (diff < 0)
                        Array.Resize<double>(ref y, x.Length);

                    this.x = x;
                    this.y = y;
                    count = x.Length;
                    if (this.x != null && this.y != null && count > 0 && this.x.Length == count && this.y.Length == count)
                        importSuccessful = true;
                }
            }
        }
        /// <summary>
        /// Identifies, if x values of points are monotonic.
        /// </summary>
        /// <returns></returns>
        public bool MonotoneInX() 
        {
            bool monotone = true;

            if (x == null || x.Length == 0)
                return false;

            for (int i = 1; i < x.Length; i++)
            {
                if (x[i - 1] > x[i])
                    monotone = false;
            }

            return monotone;
        }
    }
}
