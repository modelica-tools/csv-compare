// Algorithm.cs
// author: Susanne Walther
// date: 22.12.2014

namespace CurveCompare.Algorithms
{
    /// <summary>
    /// Represents an algorithm, that calculates a lower and an upper tube curve.
    /// </summary>
    public class Algorithm
    {
        /// <summary>
        /// true, if calculation of tube successful; <para>
        /// false, if calculation fails.</para>
        /// </summary>
        public virtual bool Successful { get; set; }
        /// <summary>
        /// Calculates a lower and an upper tube curve.
        /// </summary>
        /// <param name="reference">Reference curve with x and y values.</param>
        /// <param name="size">Size of tube.</param>
        /// <param name="minX">Min abscissa value.</param>
        /// <param name="maxX">Max abscissa value.</param>
        /// <returns>Collection of return values.</returns>
        public virtual TubeReport Calculate(Curve reference, TubeSize size, double minX, double maxX)
        {
            return new TubeReport();
        }
    }

    /// <summary>
    /// Option for the tube calculation algorithm. Defines how the distance between the reference curve and the tube is measured.
    /// </summary>
    /// <remarks>Standard = Max</remarks>
    public enum AlgorithmOptions
    {
        /// <summary>
        /// Weighted Maximum Norm:<para>
        /// Rectangles around the points of reference curve.<para/>
        /// For curves, that are monotonic in x.</para>
        /// </summary>
        Rectangle,
        /// <summary>
        /// Weighted Euclidean Norm:<para>
        /// Ellipses around the points of reference curve. <para/>
        /// For curves, that are monotonic in x.</para>
        /// </summary>
        Ellipse,
        /*
        /// <summary>
        /// Curves, that are not monotonic in x, i.e. that can go backward in x direction
        /// </summary>
        Backward
         * */
    };
}
