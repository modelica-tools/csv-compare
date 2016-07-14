// TubeReport.cs
// author: Susanne Walther
// date: 22.12.2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CurveCompare.Algorithms;

namespace CurveCompare
{
    /// <summary>
    /// Collection of return values.
    /// </summary>
    public class TubeReport
    {
        private Validity validity;

        /// <summary>
        /// Model name.
        /// </summary>
        public string ModelName { get; set; }
        /// <summary>
        /// ResultName
        /// </summary>
        public string ResultName { get; set; }
        /// <summary>
        /// = Validity.Valid, if the test curve is inside the tube;
        /// Validity.Invalid, elsewise.
        /// </summary>
        public Validity Valid
        {
            get { return validity; }
            set { validity = value; }
        }
        /// <summary>
        /// Distance from points of test curve to tube in y direction, for all points of test curve outside the tube.
        /// </summary>
        public Curve Errors { get; set; }
        /// <summary>
        /// Upper tube curve.
        /// </summary>
        public Curve Upper { get; set; }
        /// <summary>
        /// Lower tube curve.
        /// </summary>
        public Curve Lower { get; set; }
        /// <summary>
        /// Reference curve.
        /// </summary>
        public Curve Reference { get; set; }
        /// <summary>
        /// Test curve.
        /// </summary>
        public Curve Test { get; set; }
        /// <summary>
        /// Tube size.
        /// </summary>
        public TubeSize Size { get; set; }
        /// <summary>
        /// Option for the tube calculation algorithm. Defines how the distance between the reference curve and the tube is measured.
        /// </summary>
        public AlgorithmOptions Algorithm { get; set; }

        public Step ErrorStep { get; set; }

        public TubeReport()
        {
            validity = Validity.Undefined;
        }
    }

    /// <summary>
    /// Option to separate valid / invalid / both valid and invalid.
    /// </summary>
    public enum Validity
    {
        /// <summary>
        /// Undefined.
        /// </summary>
        Undefined,
        /// <summary>
        /// Show all valid.
        /// </summary>
        Valid,
        /// <summary>
        /// Show all invalid.
        /// </summary>
        Invalid,
        /// <summary>
        /// Show all valid and invalid.
        /// </summary>
        All
    };
    /// <summary>
    /// Classifies the data processing and calculation steps of CurveCompare.
    /// </summary>
    public enum Step
    {
        /// <summary>
        /// Reading data.
        /// </summary>
        DataImport,
        /// <summary>
        /// Tube size calculation.
        /// </summary>
        TubeSize,
        /// <summary>
        /// Tube calculation.
        /// </summary>
        Tube,
        /// <summary>
        /// Validation.
        /// </summary>
        Validation,
        /// <summary>
        /// None of the these steps.
        /// </summary>
        None
    };
}
