// Options2.cs
// author: Susanne Walther
// date: 18.12.2014 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CurveCompare.DataImport;

namespace CurveCompare
{
    /// <summary>
    /// Options for CurveCompare incl. TubeSize. Calculates TubeSize from two values.
    /// </summary>
    public class Options2 : IOptions
    {
        double x, y;
        double baseX, baseY, ratio;
        private Relativity relativity;
        private Log log;
        private string reportFolder;
        private bool showWindow;
        private Validity showValidity;
        private int drawFastAbove, drawPointsBelow;
        private bool drawLabelNumber;
        
        /// <summary>
        /// x value of TubeSize. Half width of rectangle. 
        /// </summary>
        public double X
        {
            get { return x; }
        }
        /// <summary>
        ///  y value of TubeSize. Half height of rectangle.
        /// </summary>
        public double Y
        {
            get { return y; }
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
        ///  Creates an instance of Options2. Sets default values.
        /// </summary>
        /// <param name="x">x value of TubeSize. Half width of rectangle. </param>
        /// <param name="y">y value of TubeSize. Half height of rectangle.</param>
        /// <remarks>Default values: <para>
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
        public Options2(double x, double y)
        {
            this.x = x;
            this.y = y;
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
            drawLabelNumber = false;
        }    
    }
}
