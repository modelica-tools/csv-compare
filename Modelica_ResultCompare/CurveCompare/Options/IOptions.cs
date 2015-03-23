// IOptions.cs
// author: Susanne Walther
// date: 18.12.2014 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CurveCompare
{
    /// <summary>
    /// Options for tube size, chart and saving.
    /// </summary>
    public interface IOptions
    {
        /// <summary>
        /// States, if values are relative or absolute. (Option for tube size.)
        /// </summary>
        Relativity Relativity { get; set; }
        /// <summary>
        /// Base of relative values in x direction. (Option for tube size.)
        /// </summary>
        double BaseX { get; set; }
        /// <summary>
        /// Base of relative values in y direction. (Option for tube size.)
        /// </summary>
        double BaseY { get; set; }
        /// <summary>
        ///  Ratio = Y / X. (Option for tube size.)
        /// </summary>
        double Ratio { get; set; }
        /// <summary>
        /// Class Log with full path name of log file. <para>
        /// = null or = (new Log()) for not saving a log file.</para>
        /// </summary>
        Log Log { get; set; }
        /// <summary>
        /// Path name of folder for image. 
        /// </summary>
        string ReportFolder { get; set; }
        /// <summary>
        /// The window with image will be shown, if true; <para> the window with image won't be shown, if false.</para>
        /// </summary>
        bool ShowWindow { get; set; } 
        /// <summary>
        /// States the cases of validity, that will be shown. Possible options are: all cases, just valid cases or just invalid cases.
        /// </summary>
        Validity ShowValidity { get; set; }
        /// <summary>
        /// Fast drawing methods will be used if<para>
        /// number of points >= DrawFastAbove.</para>
        /// </summary>
        int DrawFastAbove { get; set; }
        /// <summary>
        /// Points are drawn and lines between them, if number of points &lt; DrawPointsBelow, <para>
        /// elsewise just lines between points are drawn.</para>
        /// </summary>
        int DrawPointsBelow { get; set; }
        /// <summary>
        /// States, if the number of a point is drawn in a label near the point
        /// </summary>
        bool DrawLabelNumber { get; set; }
    }
}
