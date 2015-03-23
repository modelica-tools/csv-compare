// ChartControl.cs
// author: Susanne Walther
// date: 18.12.2014

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using CurveCompare;
#if GUI
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CurveCompare
{
    /// <summary>
    /// Shows curves in one chart area. Shows error points in another chart area beneath.
    /// </summary>
    public partial class ChartControl : Form
    {
        int drawFastAbove;
        int drawPointsBelow;
        double min, max;
        bool minMaxAssigned = false;
        int intervalCount = 20;
        ToolTip toolTip;
        bool drawLabelNumber = false;    

        /// <summary>
        /// Initializes ChartControl. The method AddLine draws always lines between the points, but not always points.
        /// </summary>
        /// <param name="drawFastAbove">The method AddLine uses fast drawing methods, if and only if <para>
        /// number of points >= drawFastAbove.</para></param>
        /// <param name="drawPointsBelow">The method AddLine draws points, if number of points &lt; drawPointsBelow, <para>
        /// elsewise AddLine draws just lines between points.<para/>
        /// </para></param>
        /// <param name="drawLabelNumber"> States, if the number of a point is drawn in a label near the point</param>
        /// <remarks>For never drawing points set drawPointsBelow = 0.<para>
        /// For always drawing points set drawPointsBelow = Int32.MaxValue.</para></remarks>
        public ChartControl(int drawFastAbove, int drawPointsBelow, bool drawLabelNumber)
            :this(drawFastAbove, drawPointsBelow)
        {
            this.drawLabelNumber = drawLabelNumber;
        }
        /// <summary>
        /// Initializes ChartControl. The method AddLine draws always lines between the points, but not always points.
        /// </summary>
        /// <param name="drawFastAbove">The method AddLine uses fast drawing methods, if and only if <para>
        /// number of points >= drawFastAbove.</para></param>
        /// <param name="drawPointsBelow">The method AddLine draws points, if number of points &lt; drawPointsBelow, <para>
        /// elsewise AddLine draws just lines between points.<para/>
        /// </para></param>
        /// <remarks>For never drawing points set drawPointsBelow = 0.<para>
        /// For always drawing points set drawPointsBelow = Int32.MaxValue.</para></remarks>
        public ChartControl(int drawFastAbove, int drawPointsBelow)
        {
            InitializeComponent();
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();
            
            // Add chart areas
            ChartArea curveChartArea = new ChartArea("curve");
            ChartArea errorChartArea = new ChartArea("error");
            initialize(curveChartArea, 0, 20, 100, 60);
            initialize(errorChartArea, 0, 80, 100, 20); 

            // Set the alignment properties
            errorChartArea.AlignmentOrientation = AreaAlignmentOrientations.Vertical;
            errorChartArea.AlignmentStyle = AreaAlignmentStyles.All;
            errorChartArea.AlignWithChartArea = curveChartArea.Name;

            // Set the alignment type
            errorChartArea.AlignmentStyle = AreaAlignmentStyles.PlotPosition | AreaAlignmentStyles.Cursor | AreaAlignmentStyles.AxesView;
            
            chart1.ChartAreas.Add(curveChartArea);
            chart1.ChartAreas.Add(errorChartArea);

            // Set Antialiasing mode
            chart1.AntiAliasing = AntiAliasingStyles.All;
            chart1.TextAntiAliasingQuality = TextAntiAliasingQuality.High;
            // Set legend style
            chart1.Legends[0].LegendStyle = LegendStyle.Column;
            // Set legend docking
            chart1.Legends[0].Docking = Docking.Top;
            // Set legend alignment
            chart1.Legends[0].Alignment = StringAlignment.Far;

            // context menu
            AddContextMenuAndItems();
            
            toolTip = new System.Windows.Forms.ToolTip();
            
            this.drawFastAbove = drawFastAbove;
            this.drawPointsBelow = drawPointsBelow;
        }
        /// <summary>
        /// Initializes Properties for zoom, scroll, adjustment.
        /// </summary>
        /// <param name="chartArea">chart area</param>
        /// <param name="x">Position.X of chart area</param>
        /// <param name="y">Position.Y of chart area</param>
        /// <param name="width">Position.Width of chart area</param>
        /// <param name="height">Position.Height of chart area</param>
        private void initialize(ChartArea chartArea, float x, float y, float width, float height) 
        {
            // zoom enabled
            chartArea.CursorX.IsUserSelectionEnabled = true;
            chartArea.CursorY.IsUserSelectionEnabled = true;
            chartArea.CursorX.IsUserEnabled = true;
            chartArea.CursorY.IsUserEnabled = true;
            // small zoom possible
            chartArea.CursorX.Interval = 0;
            chartArea.CursorY.Interval = 0;
            // scroll possible
            chartArea.AxisX.ScrollBar.Enabled = true;
            chartArea.AxisY.ScrollBar.Enabled = true;
            // small scroll possible
            chartArea.AxisX.ScaleView.SmallScrollMinSize = 1e-7;
            chartArea.AxisY.ScaleView.SmallScrollMinSize = 1e-7;
            // optimal adjustment of axes to data
            chartArea.AxisX.IsStartedFromZero = false;
            chartArea.AxisY.IsStartedFromZero = false;
            // for alignment
            chartArea.InnerPlotPosition.Auto = true;
            // Set the chart area position
            chartArea.Position.X = x;
            chartArea.Position.Y = y;
            chartArea.Position.Width = width;
            chartArea.Position.Height = height;

        }
        /// <summary>
        /// Adds a title to the chart.
        /// </summary>
        /// <param name="title">Title text.</param>
        public void addTitle(string title) 
        {
            if (!String.IsNullOrWhiteSpace(title))
                chart1.Titles.Add("Title1");
            chart1.Titles["Title1"].Text = title;
        }
        /// <summary>
        /// Adds a set of points to chart. Draws lines between the points.
        /// </summary>
        /// <param name="name">Name, visible in legend.</param>
        /// <param name="curve">Curve with x and y values of points.</param>
        /// <param name="color">Color.</param>
        public void AddLine(string name, Curve curve, Color color)
        {
            if (curve == null)
                return;

            AddLine(name, curve.X, curve.Y, color);
        }
        /// <summary>
        /// Adds a set of points to chart. Draws lines between the points.
        /// </summary>
        /// <param name="name">Name, visible in legend.</param>
        /// <param name="X">x values of the points.</param>
        /// <param name="Y">y values of the points.</param>
        /// <param name="color">Color.</param>
        public void AddLine(string name, List<double> X, List<double> Y, Color color)
        {
            AddLine(name, X.ToArray(), Y.ToArray(), color);
        }
        /// <summary>
        /// Adds a set of points to chart. Draws lines between the points.
        /// </summary>
        /// <param name="name">Name, visible in legend.</param>
        /// <param name="X">x values of the points.</param>
        /// <param name="Y">y values of the points.</param>
        /// <param name="color">Color.</param>
        public void AddLine(string name, double[] X, double[] Y, Color color)
        {
            // if series with this name exists already: return
            if (X == null || Y == null || X.Length == 0 || X.Length != Y.Length || chart1.Series.FindByName(name) != null)
                return;

            bool fast = (X.Length >= drawFastAbove);
            bool showPoints = (X.Length < drawPointsBelow);

            // Add Line
            System.Windows.Forms.DataVisualization.Charting.Series lineSeries = new System.Windows.Forms.DataVisualization.Charting.Series();
            lineSeries.Name = name;
            lineSeries.Color = color;
            if (fast)
                lineSeries.ChartType = SeriesChartType.FastLine;
            else
                lineSeries.ChartType = SeriesChartType.Line;           
            lineSeries.Points.DataBindXY(X, Y);

            if (drawLabelNumber)
            {
                for (int i = 0; i < lineSeries.Points.Count; i++)
                    lineSeries.Points[i].Label = i.ToString();
            }

            chart1.Series.Add(lineSeries);

            // Add Points
            if (showPoints) 
            {
                System.Windows.Forms.DataVisualization.Charting.Series pointSeries = new System.Windows.Forms.DataVisualization.Charting.Series();
                pointSeries.Name = name + "Points";
                pointSeries.IsVisibleInLegend = false;
                pointSeries.Color = color;
                if (fast)
                    pointSeries.ChartType = SeriesChartType.FastPoint;
                else
                    pointSeries.ChartType = SeriesChartType.Point;
                pointSeries.MarkerStyle = MarkerStyle.Cross;
                pointSeries.Points.DataBindXY(X, Y);
                chart1.Series.Add(pointSeries);
            }

            if (!minMaxAssigned)
            {
                min = X[0];
                max = X[X.Length - 1];
                minMaxAssigned = true;
            }
            else
            {
                min = Math.Min(min, X[0]);
                max = Math.Max(max, X[X.Length - 1]);
                minMaxAssigned = true;
            }

            chart1.ChartAreas["curve"].AxisX.Minimum = min;
            chart1.ChartAreas["curve"].AxisX.Maximum = max;
            chart1.ChartAreas["curve"].AxisX.Interval = (max - min)/intervalCount;
        }
        /// <summary>
        /// Adds a set of points to the error chart area.
        /// </summary>
        /// <param name="name">Name, visible in legend.</param>
        /// <param name="errors">Error points, x and y values.</param>
        public void AddErrors(string name, Curve errors)
        {
            if (errors == null)
                return;

            AddErrors(name, errors.X, errors.Y);
        }
        /// <summary>
        /// Adds a set of points to the error chart area.
        /// </summary>
        /// <param name="name">Name, visible in legend.</param>
        /// <param name="errors">Error points, x and y values.</param>
        public void AddErrors(string name, List<double> X, List<double> Y)
        { 
            AddErrors(name, X.ToArray(), Y.ToArray());
        }
        /// <summary>
        /// Adds a set of points to the error chart area.
        /// </summary>
        /// <param name="name">Name, visible in legend.</param>
        /// <param name="X">x values of error points.</param>
        /// <param name="Y">y values of error points.</param>
        public void AddErrors(string name, double[] X, double[] Y)
        {
            if (X == null || Y == null || chart1.Series.FindByName(name) != null)
                return;

            bool fast = (X.Length > drawFastAbove);
            
            // Create a data series
            System.Windows.Forms.DataVisualization.Charting.Series series = new System.Windows.Forms.DataVisualization.Charting.Series();            

            // Set name
            series.Name = name;

            // Set color
            series.Color = Color.FromKnownColor(KnownColor.Red);

            // Set series chart type
            if (fast)
                series.ChartType = SeriesChartType.FastPoint;
            else
                series.ChartType = SeriesChartType.Point;

            // Set Style
            series.MarkerStyle = MarkerStyle.Cross;
                       
            // Add data points to the series
            series.Points.DataBindXY(X, Y);
           
            // Add series to the chart
            chart1.Series.Add(series);
            chart1.Series[name].ChartArea = "error";

            if (!minMaxAssigned)
            {
                min = X[0];
                max = X[X.Length - 1];
                minMaxAssigned = true;
            }
            else
            {
                min = Math.Min(min, X[0]);
                max = Math.Max(max, X[X.Length - 1]);
                minMaxAssigned = true;
            }
            
            chart1.ChartAreas["error"].AxisX.Minimum = min;
            chart1.ChartAreas["error"].AxisX.Maximum = max;
            chart1.ChartAreas["error"].AxisX.Interval = (max - min) / intervalCount;
        }
        /// <summary>
        /// Saves the chart as image to a file.
        /// </summary>
        /// <param name="filename">Full path of file.</param>
        /// <param name="format">Image format.</param>
        public void saveAsImage(string filename, System.Drawing.Imaging.ImageFormat format) 
        {
            chart1.SaveImage(filename, format);
        }
        /// <summary>
        /// Add context menu.
        /// </summary>
        private void AddContextMenuAndItems()
        {
            // --------------------------------------------
            // context menu for chart1
            // --------------------------------------------
            ContextMenu contextMenuC1 = new ContextMenu();
            // Add menu item Zoom Out
            MenuItem menuItemZoomOutC1 = new MenuItem();
            menuItemZoomOutC1.Text = "Zoom One Step Out";
            contextMenuC1.MenuItems.Add(menuItemZoomOutC1);
            menuItemZoomOutC1.Click += new System.EventHandler(this.menuItemZoomOutC1_Click);
            // Add menu item Zoom Reset
            MenuItem menuItemZoomResetC1 = new MenuItem();
            menuItemZoomResetC1.Text = "Zoom Reset";
            contextMenuC1.MenuItems.Add(menuItemZoomResetC1);
            menuItemZoomResetC1.Click += new System.EventHandler(this.menuItemZoomResetC1_Click);
            chart1.ContextMenu = contextMenuC1;
        }
        /// <summary>
        /// Reset one zoom step.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemZoomOutC1_Click(object sender, System.EventArgs e)
        {
            chart1.ChartAreas["curve"].AxisX.ScaleView.ZoomReset();
            chart1.ChartAreas["curve"].AxisY.ScaleView.ZoomReset();
            chart1.ChartAreas["error"].AxisX.ScaleView.ZoomReset();
            chart1.ChartAreas["error"].AxisY.ScaleView.ZoomReset();
        }
        /// <summary>
        /// Reset all zoom steps.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuItemZoomResetC1_Click(object sender, System.EventArgs e)
        {
            chart1.ChartAreas["curve"].AxisX.ScaleView.ZoomReset(0);
            chart1.ChartAreas["curve"].AxisY.ScaleView.ZoomReset(0);
            chart1.ChartAreas["error"].AxisX.ScaleView.ZoomReset(0);
            chart1.ChartAreas["error"].AxisY.ScaleView.ZoomReset(0);
        }
        /// <summary>
        /// Close the window on enter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chart1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                this.Close();
        }
        /// <summary>
        /// Shows Tooltip with x and y value of position of mouse click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chart1_Click(object sender, EventArgs e)
        {
            double x = chart1.ChartAreas["curve"].CursorX.Position;
            double y = chart1.ChartAreas["curve"].CursorY.Position;
            toolTip.Show("(" + x.ToString() + "; " + y.ToString() + ")", chart1, 10000);
        }
    }
}
#endif