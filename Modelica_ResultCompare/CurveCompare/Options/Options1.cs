// Options1.cs
// author: Susanne Walther
// date: 19.12.2014 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CurveCompare.DataImport;

namespace CurveCompare
{
    /// <summary>
    /// Provides options for setting tube size from one value, visualization and saving log file and image. Calculates TubeSize from one value.
    /// </summary>
    public class Options1 : IOptions
    {
        double val;
        Axes axes;
        bool formerBaseAndRatio;
        double baseX, baseY, ratio;
        private Relativity relativity;
        private Log log;
        private string reportFolder;
        private bool showWindow;
        private Validity showValidity;
        private int drawFastAbove, drawPointsBelow;
        private bool drawLabelNumber;
        
        /// <summary>
        /// Value of TubeSize.
        /// </summary>
        public double Value
        {
            get { return val; }
        }
        /// <summary>
        /// States, if value is x (half width of rectangle) or y (half height of rectangle).
        /// </summary>
        public Axes Axes
        {
            get { return axes; }
        }
        /// <summary>
        /// Base and Ratio are calculated like in the former CSV-Compare
        /// </summary>
        public bool FormerBaseAndRatio
        {
            get { return formerBaseAndRatio; }
        }
        /// <summary>
        /// States, if values are relative or absolute. 
        /// </summary>
        public Relativity Relativity
        {
            get { return relativity; }
            set { relativity = value; }
        }
        /// <summary>
        /// Base of relative values in x direction. (Option for tube size.)
        /// </summary>
        public double BaseX
        {
            get { return baseX; }
            set { baseX = value; }
        }
        /// <summary>
        /// Base of relative values in y direction. (Option for tube size.)
        /// </summary>
        public double BaseY
        {
            get { return baseY; }
            set { baseY = value; }
        }
        /// <summary>
        ///  Ratio = Y / X. (Option for tube size.)
        /// </summary>
        public double Ratio
        {
            get { return ratio; }
            set { ratio = value; }
        }
        /// <summary>
        /// Class Log with full path name of log file.
        /// </summary>
        public Log Log
        {
            get { return log; }
            set { log = value; }
        }
        /// <summary>
        /// Path name of folder for image.
        /// </summary>
        public string ReportFolder
        {
            get { return reportFolder; }
            set { reportFolder = value; }
        }
        /// <summary>
        /// The window with image will be shown, if true; <para> the window with image won't be shown, if false.</para>
        /// </summary>
        public bool ShowWindow
        {
            get { return showWindow; }
            set { showWindow = value; }
        }
        /// <summary>
        /// States the cases of validity, that will be shown. Possible options are: all cases, just valid cases or just invalid cases.
        /// </summary>
        public Validity ShowValidity
        {
            get { return showValidity; }
            set { showValidity = value; }
        }
        /// <summary>
        /// Fast drawing methods will be used if<para>
        /// number of points >= DrawFastAbove.</para>
        /// </summary>
        public int DrawFastAbove
        {
            get { return drawFastAbove; }
            set { drawFastAbove = value; }
        }
        /// <summary>
        /// Points are drawn and lines between them, if number of points &lt; DrawPointsBelow, <para>
        /// elsewise just lines between points are drawn.</para>
        /// </summary>
        public int DrawPointsBelow
        {
            get { return drawPointsBelow; }
            set { drawPointsBelow = value; }
        }
        /// <summary>
        /// States, if the number of a point is drawn in a label near the point
        /// </summary>
        public bool DrawLabelNumber
        {
            get { return drawLabelNumber; }
            set { drawLabelNumber = value; }
        }
        /// <summary>
        /// Creates an instance of Options1.
        /// </summary>
        /// <param name="value">Value of TubeSize.</param>
        /// <param name="axes">States, if value is x (half width of rectangle) or y (half height of rectangle).</param>
        /// /// <remarks>Default values: <para>
        /// Use relative values for calculation of TubeSize: Relativity = Relativity.Relative <para/>
        /// Delimiter = ';' <para/>
        /// Separator = '.' <para/>
        /// Don't save log file: log = new Log()<para/>
        /// Don't save image: reportFolder = ""<para/>
        /// Don't show window: showWindow = false<para/>
        /// Show all (valid and invalid): ShowValidity = Validity.All<para/>
        /// Always use normal drawing methods, never fast drawing methods: drawFastAbove = 0<para/>
        /// Always draw points: drawPointsBelow = Int32.MaxValue
        /// </para></remarks>
        public Options1(double value, Axes axes)
        {
            this.val = value;
            this.axes = axes;
            relativity = Relativity.Relative;
            baseX = Double.NaN;
            baseY = Double.NaN;
            ratio = Double.NaN;
            log = new Log();
            reportFolder = "";
            showWindow = false;
            showValidity = Validity.All;
            drawFastAbove = 0;
            drawPointsBelow = Int32.MaxValue;
            formerBaseAndRatio = false;
            drawLabelNumber = false;
        }
        /// <summary>
        /// Creates an instance of Options1.
        /// </summary>
        /// <param name="value">Value of TubeSize.</param>
        /// <param name="axes">States, if value is x (half width of rectangle) or y (half height of rectangle).</param>
        /// <param name="formerBaseAndRatio">Base and Ratio are calculated like in the former CSV-Compare, if true;<para>
        /// Ratio and Base get standard values, elsewise.</para></param>
        /// /// <remarks>Default values: <para>
        /// Use relative values for calculation of TubeSize: Relativity = Relativity.Relative <para/>
        /// Delimiter = ';' <para/>
        /// Separator = '.' <para/>
        /// Don't save log file: log = new Log()<para/>
        /// Don't save image: reportFolder = ""<para/>
        /// Don't show window: showWindow = false<para/>
        /// Show all (valid and invalid): ShowValidity = Validity.All<para/>
        /// Always use normal drawing methods, never fast drawing methods: drawFastAbove = 0<para/>
        /// Always draw points: drawPointsBelow = Int32.MaxValue
        /// </para></remarks>
        public Options1(double value, Axes axes, bool formerBaseAndRatio)
        {
            this.val = value;
            this.axes = axes;
            relativity = Relativity.Relative;
            baseX = Double.NaN;
            baseY = Double.NaN;
            ratio = Double.NaN;
            log = new Log();
            reportFolder = "";
            showWindow = false;
            showValidity = Validity.All;
            drawFastAbove = 0;
            drawPointsBelow = Int32.MaxValue;
            this.formerBaseAndRatio = formerBaseAndRatio;
            drawLabelNumber = false;
        }
    }
}
