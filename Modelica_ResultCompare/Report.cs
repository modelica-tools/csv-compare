using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NPlot;

namespace CsvCompare
{
    ///The Chart class holds information of the jquery plots that are rendered in the html report
    public class Chart
    {
        private List<Series> _series;
        private Guid _guid = Guid.NewGuid();
        private double _dMin, _dMax;
        private double _lDeltaError;
        
        public Guid Id { get { return _guid; } }
        public string Title { get; set; }
        public string LabelX { get; set; }
        public string LabelY { get; set; }
        public List<Series> Series { get { return _series; } }
        public int Errors { get; set; }
        public Chart(){ _series = new List<Series>(); }
        public double MinValue { get { return _dMin; } set { _dMin = value; } }
        public double MaxValue { get { return _dMax; } set { _dMax = value; } }
        public double DeltaError {
            get { return _lDeltaError; }
            set { _lDeltaError = value; }
        }
        public bool UseBitmap { get; set; }

        public string RenderChart()
        {
            StringBuilder sb;

            if (!GetHeaderTable(out sb))//no charts? skip the rest
                return sb.ToString();

            sb.AppendLine("<script class=\"code\" type=\"text/javascript\">");
            sb.AppendLine("    $(document).ready(function(){");
            sb.Append("        var data = [");
            //sValues 
            int i = 0;
            foreach (Series s in this.Series)
            {
                if (string.IsNullOrEmpty(s.ArrayString) || s.Title == "ERRORS")
                    continue;

                sb.Append(s.ArrayString);
                if (i < this.Series.Count)
                    sb.Append(",");
                i++;
            }
            if (sb.ToString().EndsWith(","))
                sb = sb.Remove(sb.Length - 1, 1);

            //Add some tolerance for graph scaling and remeber values to get equal scaling for graph and error graph
            if (!Double.IsNaN(_dMin) && !Double.IsNaN(_dMax))
            {
                double d1 = _dMin;
                double d2 = _dMax;
                _dMin = d1 - (d1 + d2 * 5 / 100);
                _dMax = d2 + (d1 + d2 * 5 / 100);
            }

            sb.AppendLine("];");
            sb.AppendLine("        var plot1 = $.jqplot ('" + _guid.ToString() + @"', data, {");
            sb.AppendLine("        seriesDefaults: {show: true, xaxis: 'xaxis', yaxis: 'yaxis', lineWidth: 1, shadow: false, showLine: true, showMarker: false,},");
            sb.Append("        series:[");
            i = 0;
            foreach (Series s in this.Series)
            {
                if (string.IsNullOrEmpty(s.ArrayString))
                    continue; 
                
                sb.AppendFormat("{{color:'#{0}', label:'{1}'}}", ColorToHexString(s.Color), s.Title);
                if (i<this.Series.Count)
                    sb.Append(",");
                i++;
            }
            if (sb.ToString().EndsWith(","))
                sb = sb.Remove(sb.Length - 1, 1);

            sb.AppendLine("], title: '" + this.Title + "',");
            sb.AppendLine("    grid: {");
            sb.AppendLine("            drawGridLines: false,        // wether to draw lines across the grid or not.");
            sb.AppendLine("            gridLineColor: '#cccccc',    // *Color of the grid lines.");
            sb.AppendLine("            background: '#ffffff',      // CSS color spec for background color of grid.");
            sb.AppendLine("            borderColor: '#000000',     // CSS color spec for border around grid.");
            sb.AppendLine("            borderWidth: 1.0,           // pixel width of border around grid.");
            sb.AppendLine("            shadow: false,               // draw a shadow for grid.");
            sb.AppendLine("        },");
            sb.AppendLine("    cursor: {");
            sb.AppendLine("            show: true,");
            sb.AppendLine("            tooltipLocation:'sw',");
            sb.AppendLine("            showVerticalLine:true,");
            sb.AppendLine("            zoom:true,");
            sb.AppendLine("          },");
            sb.AppendLine("    legend: {show: true },");
            sb.AppendLine("          // You can specify options for all axes on the plot at once with");
            sb.AppendLine("          // the axesDefaults object.  Here, we're using a canvas renderer");
            sb.AppendLine("          // to draw the axis label which allows rotated text.");
            sb.AppendLine("          axesDefaults: {");
            sb.AppendLine("            labelRenderer: $.jqplot.CanvasAxisLabelRenderer");
            sb.AppendLine("          },");
            sb.AppendLine("          // An axes object holds options for all axes.");
            sb.AppendLine("          // Allowable axes are xaxis, x2axis, yaxis, y2axis, y3axis, ...");
            sb.AppendLine("          // Up to 9 y axes are supported.");
            sb.AppendLine("          axes: {");
            sb.AppendLine("            // options for each axis are specified in seperate option objects.");
            sb.AppendLine("            xaxis: {");
            sb.AppendFormat("              label: \"{0}\",", this.LabelX).AppendLine();
            //Min and Max might break the scaling of the graph if the value format is "smaller" than the tick format
            //if (!Double.IsNaN(_dMin) && !Double.IsNaN(_dMax))
            //{
            //    sb.AppendLine("            min:" + _dMin.ToString(CultureInfo.CreateSpecificCulture("en-US")) + ",");
            //    sb.AppendLine("            max:" + _dMax.ToString(CultureInfo.CreateSpecificCulture("en-US")) + ",");
            //}
            sb.AppendLine("              // Turn off \"padding\".  This will allow data point to lie on the");
            sb.AppendLine("              // edges of the grid.  Default padding is 1.2 and will keep all");
            sb.AppendLine("              // points inside the bounds of the grid.");
            sb.AppendLine("              pad: 0, tickOptions: {format: '%.5g'}");
            sb.AppendLine("            },");
            sb.AppendLine("            yaxis: {");
            sb.AppendFormat("              label: \"{0}\",", this.LabelY).AppendLine();
            sb.AppendLine("              tickOptions: {format: '%.5g'}");
            sb.AppendLine("            }");
            sb.AppendLine("          }");
            sb.AppendLine("        });");
            sb.AppendLine("    });");
            sb.AppendLine("    </script>");
            sb.AppendLine("    <p></p>");
            sb.AppendFormat("    <div id=\"{0}\" style=\"height:480px; width:640px;\"></div>", _guid);

            if (this.Errors > 0)
            {
                sb.AppendLine("<script class=\"code\" type=\"text/javascript\">");
                sb.AppendLine("    $(document).ready(function(){");
                sb.Append("        var data_err = [");

                sb.Append((from s in this.Series where s.Title == "ERRORS" select s).Single().ArrayString);

                sb.AppendLine("];");
                sb.AppendLine("        var plot2 = $.jqplot ('" + _guid.ToString() + @"_errors', data_err, {");
                sb.AppendLine("        seriesDefaults: {show: true, xaxis: 'xaxis', yaxis: 'yaxis', lineWidth: 1, shadow: false, showLine: true, showMarker: false,},");
                sb.Append("        series:[");

                sb.AppendFormat("{{color:'#{0}', label:'ERROR'}}", ColorToHexString(Color.Red));

                sb.AppendLine("], title: '',");
                sb.AppendLine("    grid: {");
                sb.AppendLine("            drawGridLines: false,        // wether to draw lines across the grid or not.");
                sb.AppendLine("            gridLineColor: '#cccccc',    // *Color of the grid lines.");
                sb.AppendLine("            background: '#ffffff',      // CSS color spec for background color of grid.");
                sb.AppendLine("            borderColor: '#000000',     // CSS color spec for border around grid.");
                sb.AppendLine("            borderWidth: 1.0,           // pixel width of border around grid.");
                sb.AppendLine("            shadow: false,               // draw a shadow for grid.");
                sb.AppendLine("        },");
                sb.AppendLine("    cursor: {");
                sb.AppendLine("            show: true,");
                sb.AppendLine("            tooltipLocation:'sw',");
                sb.AppendLine("            showVerticalLine:true,");
                sb.AppendLine("            zoom:true");
                sb.AppendLine("          },");
                sb.AppendLine("    legend: {show: false },");
                sb.AppendLine("          // You can specify options for all axes on the plot at once with");
                sb.AppendLine("          // the axesDefaults object.  Here, we're using a canvas renderer");
                sb.AppendLine("          // to draw the axis label which allows rotated text.");
                sb.AppendLine("          axesDefaults: {");
                sb.AppendLine("            labelRenderer: $.jqplot.CanvasAxisLabelRenderer");
                sb.AppendLine("          },");
                sb.AppendLine("          // An axes object holds options for all axes.");
                sb.AppendLine("          // Allowable axes are xaxis, x2axis, yaxis, y2axis, y3axis, ...");
                sb.AppendLine("          // Up to 9 y axes are supported.");
                sb.AppendLine("          axes: {");
                sb.AppendLine("            // options for each axis are specified in seperate option objects.");
                sb.AppendLine("            xaxis: {");
                sb.AppendLine("              label: \"time\",");
                //Min and Max might break the scaling of the graph if the value format is "smaller" than the tick format
                //if (!Double.IsNaN(_dMin) && !Double.IsNaN(_dMax))
                //{
                //    sb.AppendLine("            min:" + _dMin.ToString(CultureInfo.CreateSpecificCulture("en-US")) + ",");
                //    sb.AppendLine("            max:" + _dMax.ToString(CultureInfo.CreateSpecificCulture("en-US")) + ",");
                //}
                sb.AppendLine("              // Turn off \"padding\".  This will allow data point to lie on the");
                sb.AppendLine("              // edges of the grid.  Default padding is 1.2 and will keep all");
                sb.AppendLine("              // points inside the bounds of the grid.");
                sb.AppendLine("              pad: 0, tickOptions: {format: '%.5g'}");
                sb.AppendLine("            },");
                sb.AppendLine("            yaxis: {");
                sb.AppendLine("              label: \"error\"");
                sb.AppendLine("            }");
                sb.AppendLine("          }");
                sb.AppendLine("        });");
                sb.AppendLine("    });");
                sb.AppendLine("    </script>");
                sb.AppendLine("    <p></p>");
                sb.AppendFormat("    <div id=\"{0}_errors\" style=\"height:120px; width:640px;\"></div>", _guid);
            }
            AddLinkToTop(sb);

            return sb.ToString();
        }

        public string RenderBitmap(string path)
        {
            path = Path.GetDirectoryName(path);

            string Filename = string.Format("{0}.png", this.Title);
            string ImagePath = Path.Combine(path, @"img", Filename);

            StringBuilder sb;
            
            if (!GetHeaderTable(out sb))//no charts? skip the rest
                return sb.ToString();

            NPlot.Bitmap.PlotSurface2D npSurface;
            Font AxisFont;
            Font TickFont;
            NPlot.Grid p;
            InitPlot(out npSurface, out AxisFont, out TickFont, out p, 700, 500);

            foreach (Series s in this.Series)
            {
                if (s.Title.ToUpperInvariant() == "ERRORS")
                    continue;
                else
                {
                    NPlot.LinePlot npPlot = new LinePlot();
                    //Weight:
                    npPlot.AbscissaData = s.XAxis;
                    npPlot.OrdinateData = s.YAxis;
                    npPlot.Label = s.Title;
                    npPlot.Color = s.Color;
                    npSurface.Add(npPlot, NPlot.PlotSurface2D.XAxisPosition.Bottom, NPlot.PlotSurface2D.YAxisPosition.Left);
                }
            }
            SetAxes(npSurface, AxisFont, TickFont);
            double iMin = npSurface.XAxis1.WorldMin;
            double iMax = npSurface.XAxis1.WorldMax;

            if (!Directory.Exists(Path.GetDirectoryName(ImagePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(ImagePath));

            //Save image and add it to the report
            npSurface.Bitmap.Save(ImagePath, System.Drawing.Imaging.ImageFormat.Png);
            sb.AppendFormat("<img src=\"img/{0}\" alt=\"{1}\" />", Filename, this.Title);

            //Generate error graph if needed
            if (this.Errors > 0)
            {
                Filename = "errors." + Filename;
                ImagePath = Path.Combine(path, "img", Filename);
                npSurface.Clear();

                InitPlot(out npSurface, out AxisFont, out TickFont, out p, 700, 300);

                foreach (Series s in this.Series)
                {
                    if (s.Title.ToUpperInvariant() == "ERRORS")
                    {
                        NPlot.LinePlot npPlot = new LinePlot();
                        npPlot.AbscissaData = s.XAxis;
                        npPlot.OrdinateData = s.YAxis;
                        npPlot.Label = s.Title;
                        npPlot.Color = s.Color;

                        npSurface.Add(npPlot, NPlot.PlotSurface2D.XAxisPosition.Bottom, NPlot.PlotSurface2D.YAxisPosition.Left);
                        break;
                    }
                }
                //Set min/max to same values as main plot
                npSurface.XAxis1.WorldMin = iMin;
                npSurface.XAxis1.WorldMax = iMax;
                SetAxes(npSurface, AxisFont, TickFont);

                //Save image and add it to the report
                npSurface.Bitmap.Save(ImagePath, System.Drawing.Imaging.ImageFormat.Png);
                sb.AppendFormat("<img src=\"img/{0}\" alt=\"{1}\" />", Filename, this.Title);

            }
            AddLinkToTop(sb);

            return sb.ToString();
        }

        private static void AddLinkToTop(StringBuilder sb)
        {
            sb.AppendLine("<p style=\"width: 100%; text-align: right;\"><a href=\"#top\">[Back to top]</a></p>");
        }

        private bool GetHeaderTable(out StringBuilder sb)
        {
            sb = new StringBuilder();
            sb.AppendFormat("<a id=\"a{0}\"/>", _guid);

            if (null == this.Series || this.Series.Count == 0)
            {
                sb.AppendLine("<table class=\"info\">");
                sb.AppendFormat("	<tr><td class=\"header\">Value:</td><td>{0}</td></tr>", this.Title).AppendLine();
                sb.AppendLine("	<tr><td class=\"header\">Errors:</td><td>Exception during validation, skipping!</td></tr>");
                sb.AppendLine("</table>");
                sb.AppendLine("<p style=\"width: 100%; text-align: right;\"><a href=\"#top\">[Back to top]</a></p>");

                return false;
            }

            sb.AppendLine("<table class=\"info\">");
            sb.AppendFormat("	<tr><td class=\"header\">Value:</td><td>{0}</td></tr>", this.Title).AppendLine();

            if (this.Errors > 0)
                sb.AppendFormat("	<tr class=\"error\"><td class=\"header\">Errors:</td><td>{0} (relative error is {1:0.00})</td></tr>", this.Errors, this.DeltaError).AppendLine();
            else if (this.Errors == 0)
                sb.AppendFormat("	<tr><td class=\"header\">Errors:</td><td>{0}</td></tr>", this.Errors).AppendLine();
            else
            {
                sb.AppendLine("	<tr class=\"warning\"><td class=\"header\">Errors:</td><td>Result not found in base file.</td></tr>");
                sb.AppendLine("</table>");
                sb.AppendLine("<p style=\"width: 100%; text-align: right;\"><a href=\"#top\">[Back to top]</a></p>");
                return false;
            }

            sb.AppendLine("</table>");
            return true;
        }

        private void SetAxes(NPlot.Bitmap.PlotSurface2D npSurface, Font AxisFont, Font TickFont)
        {
            //X axis
            npSurface.XAxis1.Label = "Time";
            npSurface.XAxis1.NumberFormat = "{0:####0.0}";
            npSurface.XAxis1.TicksLabelAngle = 90;
            npSurface.XAxis1.TickTextNextToAxis = true;
            npSurface.XAxis1.FlipTicksLabel = true;
            npSurface.XAxis1.LabelOffset = 110;
            npSurface.XAxis1.LabelOffsetAbsolute = true;
            npSurface.XAxis1.LabelFont = AxisFont;
            npSurface.XAxis1.TickTextFont = TickFont;

            //Y axis
            npSurface.YAxis1.Label = this.Title;
            npSurface.YAxis1.NumberFormat = "{0:####0.0}";
            npSurface.YAxis1.LabelFont = AxisFont;
            npSurface.YAxis1.TickTextFont = TickFont;

            //Legend definition:
            NPlot.Legend npLegend = new NPlot.Legend();
            npLegend.AttachTo(NPlot.PlotSurface2D.XAxisPosition.Top, NPlot.PlotSurface2D.YAxisPosition.Right);
            npLegend.VerticalEdgePlacement = Legend.Placement.Inside;
            npLegend.HorizontalEdgePlacement = Legend.Placement.Outside;
            npLegend.BorderStyle = NPlot.LegendBase.BorderType.Line;
            npLegend.XOffset = -5;
            npLegend.YOffset = -20;
            npLegend.BackgroundColor = Color.White;

            npSurface.Legend = npLegend;
            
            //Update PlotSurface:
            npSurface.Refresh();
        }

        private void InitPlot(out NPlot.Bitmap.PlotSurface2D npSurface, out Font AxisFont, out Font TickFont, out NPlot.Grid p, int width, int height)
        {
            npSurface = new NPlot.Bitmap.PlotSurface2D(width, height);
            npSurface.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //Font definitions:
            Font TitleFont = new Font("Arial", 12);
            AxisFont = new Font("Arial", 10);
            TickFont = new Font("Arial", 8);

            //Prepare PlotSurface:
            npSurface.Clear();
            npSurface.Title = this.Title;
            npSurface.BackColor = System.Drawing.Color.White;

            //Left Y axis grid:
            p = new Grid();
            npSurface.Add(p, NPlot.PlotSurface2D.XAxisPosition.Bottom, NPlot.PlotSurface2D.YAxisPosition.Left);
        }

        /// Convert a .NET Color to a hex string.
        /// @returns ex: "FFFFFF", "AB12E9"
        private static string ColorToHexString(Color color)
        {
            char[] cDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
            byte[] bytes = new byte[3];
            bytes[0] = color.R;
            bytes[1] = color.G;
            bytes[2] = color.B;
            char[] chars = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i * 2] = cDigits[b >> 4];
                chars[i * 2 + 1] = cDigits[b & 0xF];
            }
            return new string(chars);
        }
    }

    /// This class keeps information about the plots that are to be printed in the html report
    public class Series
    {
        private Color _col;
        private string _sArrayString;
        private string _title;
        private double[] _xAxis;
        private double[] _yAxis;

        /// Holds the color of the plot as C# object. It is converted at runtime via ColorHelper.ColorToHexString(Color color).
        public Color Color { get { return _col; } set { _col = value; } }
        /// This is the title used in the graph legend
        public string Title { get { return _title; } set { _title = value; } }
        /// Returns the string to be used in jQuery [[0,0],[1,0],...,[100,78]]
        public string ArrayString { get { return _sArrayString; } set { _sArrayString = value; } }
        /// Holds arrays for plotting bimtap
        public double[] XAxis { get { return this._xAxis; } set { this._xAxis = value; } }
        public double[] YAxis { get { return this._yAxis; } set { this._yAxis = value; } }

        /// Encodes arrays for the use as a jquery array.
        public static string GetArrayString(double[] xValues, double[] yValues)
        {
            StringBuilder s = new StringBuilder("[");
            double dOffset = 0;
            for (int i = 0; i < xValues.Length; i++)
            {
                if (i == yValues.Length)
                    break;
                if (Double.IsNaN(yValues[i]) || Double.IsNaN(xValues[i]))
                    continue; //return string.Empty;
                else
                    s.AppendFormat(CultureInfo.InvariantCulture, "[{0},{1}],", xValues[i], yValues[i] + dOffset);
            }
            return s.Remove(s.Length - 1, 1).Append("]").ToString();
        }        
    }

    /// This class represents the "meta report"
    public class MetaReport
    {
        private FileInfo _path;
        private List<Report> _reports = new List<Report>();
        private bool _bReportDirSet = false;
        
        public bool ReportDirSet { get { return _bReportDirSet; } set { _bReportDirSet = value; } }
        public FileInfo FileName { get { return _path; } set { _path = value; } }
        public List<Report> Reports { get { return _reports; } }

        public MetaReport() {  }
        public MetaReport(string sFilePath) { _path = new FileInfo(sFilePath); }

        /// Write the report to a html file
        /// @para log logfile object from the main program
        /// @para options options from commandline
        /// @return FALSE on error
        public bool WriteReport(Log log, Options options)
        {
            bool bRet = false;

            if (null == _reports || _reports.Count == 0 || _reports[0] == null)//Exit if empty
            {
                log.Error("No report to write!");
                return false;
            }

            if (!options.NoMetaReport)
            {
                if (null == _path)// Setting output path if not already done
                {
                    _path = new FileInfo(Path.Combine(Path.GetDirectoryName(_reports[0].FileName), string.Format(CultureInfo.CurrentCulture, "{0:yyyy-MM-dd}-index.html", DateTime.Now)));
                    log.WriteLine("No report path has been given, writing to {0}", _path);
                }
                else
                    if (!_path.Directory.Exists)
                        _path.Directory.Create();

                if (!_path.Extension.StartsWith(".htm", StringComparison.OrdinalIgnoreCase))//add file name to path if not already present
                    _path = new FileInfo(Path.Combine(_path.FullName, string.Format(CultureInfo.CurrentCulture, "{0:yyyy-MM-dd}-index.html", DateTime.Now)));

                log.WriteLine("Writing meta report to {0}", _path);
                if (!options.OverrideOutput && _path.Exists)
                {
                    _path = new FileInfo(Path.Combine(_path.DirectoryName, string.Format(CultureInfo.CurrentCulture, "{0:yyyy-MM-ddTHH-mm-ss}-index.html", DateTime.Now)));
                    log.WriteLine(LogLevel.Warning, "Meta report already exists and --override has been set to false. Changed target filename to \"{0}\"", _path);
                }

                //if (!_path.Exists)
                //    _path = new FileInfo(_path.DirectoryName + string.Format(CultureInfo.CurrentCulture, "{0:yyyy-MM-ddTHH-mm-ss}-index.html", DateTime.Now));

                // write report to html file
                using (TextWriter writer = new StreamWriter(_path.FullName, false))
                {
                    writer.WriteLine("<!DOCTYPE html>");
                    writer.WriteLine("  <head>");
                    writer.WriteLine("<style type=\"text/css\">");
                    writer.WriteLine("body{ background: #EEEEEE; color: #000; text-align: center;}");
                    writer.WriteLine("body, table{ font-family: Arial, Helvetica, sans-serif; font-size: 12px; }");
                    writer.WriteLine("#page{ width: 700px; margin: auto; text-align: left; background-color: #FFF;}");
                    writer.WriteLine("table.info{ border: 0; width: 690px; margin: 5px;}");
                    writer.WriteLine("table.info td{ background-color: #efefef; padding: 1em;}");
                    writer.WriteLine("table.info td.header{ font-weight: bold; width: 150px; background-color: #EEE;}");
                    writer.WriteLine("table.info td.error, span.error { background-color: #F5A9BC; color: red; }");
                    writer.WriteLine("table.info td.ok, span.ok{ background-color: #CDFECD; color: green; }");
                    writer.WriteLine("table.info td.untested, span.untested{ background-color: #DDD; color: #999; }");
                    writer.WriteLine("table.info td.right{ text-align: right; }");
                    writer.WriteLine("h1{ font-size: 16px; padding: 1em; }");
                    writer.WriteLine("</style>");
                    writer.WriteLine("</head>");
                    writer.WriteLine("<body>");
                    writer.WriteLine("<div id=\"page\">");
                    writer.WriteLine("<table class=\"info\">");
                    writer.WriteLine("	<tr><td colspan=\"3\" class=\"header\"><h1>Metareport - CSV file comaprison</h1></td></tr>");
                    writer.WriteLine("	<tr><td colspan=\"2\" class=\"header\">Timestamp:</td><td>{0} [UTC]</td></tr>", DateTime.UtcNow);
                    writer.WriteLine("	<tr><td colspan=\"2\" class=\"header\">Mode:</td><td>{0}</td></tr>", options.Mode.ToString());
                    switch (options.Mode)
                    {
                        case OperationMode.CsvTreeCompare:
                            writer.WriteLine("	<tr><td colspan=\"2\" class=\"header\">Base Directory:</td><td>{0}</td></tr>", options.Items[1]);
                            writer.WriteLine("	<tr><td colspan=\"2\" class=\"header\">Compare Directory:</td><td>{0}</td></tr>", options.Items[0]);
                            break;
                        case OperationMode.CsvFileCompare:
                            writer.WriteLine("	<tr><td colspan=\"2\" class=\"header\">Base File:</td><td>{0}</td></tr>", options.Items[1]);
                            writer.WriteLine("	<tr><td colspan=\"2\" class=\"header\">Compare File:</td><td>{0}</td></tr>", options.Items[0]);
                            break;
                        case OperationMode.FmuChecker:
                            writer.WriteLine("	<tr><td colspan=\"2\" class=\"header\">FMU Checker:</td><td>{0}</td></tr>", options.CheckerPath);
                            writer.WriteLine("	<tr><td colspan=\"2\" class=\"header\">FMU Arguments:</td><td>{0}</td></tr>", options.CheckerArgs);
                            break;
                        default:
                            break;
                    }
                    writer.WriteLine("	<tr><td colspan=\"2\" class=\"header\">Verbosity:</td><td>{0}</td></tr>", options.Verbosity.ToString(CultureInfo.CurrentCulture));

                    if (null != options.Tolerance)
                        writer.WriteLine("	<tr><td colspan=\"2\" class=\"header\">Tolerance:</td><td>{0}</td></tr>", options.Tolerance);

                    if (!String.IsNullOrEmpty(options.Logfile))
                    {
                        writer.WriteLine("	<tr><td colspan=\"2\" class=\"header\">Logfile:</td><td><a href=\"file:///{0}\">{0}</a></td></tr>", options.Logfile);
                        writer.WriteLine("	<tr><td colspan=\"2\" class=\"header\">Loglevel:</td><td>{0}</td></tr>", ((LogLevel)options.Verbosity));
                    }

                    int iTested = 0;
                    int iErrors = 0;
                    double dSuccess;

                    try
                    {
                        iTested = _reports.Count - (from c in _reports where c.TotalErrors == -1 select c).Count();
                        iErrors = (from c in _reports where c.TotalErrors > 0 && c.TotalErrors != -1 select c).Count();
                    }
                    catch (NullReferenceException)
                    {
                        //empty reports
                    }

                    if (iTested <= 0)
                        dSuccess = 0;
                    else
                        dSuccess = ((1 - ((double)iErrors / (double)iTested)));

                    writer.WriteLine("	<tr><td colspan=\"2\" class=\"header\">Compared Files:</td><td>The compare file contained {0} results. {1} results have been tested. {2} failed, success rate is {3:0.0%}.</td></tr>",
                        _reports.Count,     //All results
                        iTested,            //All tested results
                        iErrors,            //Errors
                        dSuccess);
                    writer.WriteLine("<tr><td class=\"header\" colspan=\"3\">Results</td></tr>");
                    writer.WriteLine("<tr><td colspan=\"2\">&nbsp;</td><td>FAILED - at least one result failed its check with the base file<br/>UNTESTED - no base file has been found for all results in the file<br/>SUCCEEDED - All results have been checked and are valid</td></tr>");

                    //write results and paths of the sub reports
                    foreach (Report r in _reports)
                    {
                        if (null != r)// Catch empty report objects
                        {
                            if (_bReportDirSet)
                            {
                                r.FileName = Path.Combine(_path.Directory.FullName, Path.GetFileName(r.FileName));
                                r.RelativePaths = true;
                            }


                            if (r.TotalErrors > 0)
                            {
                                if (r.RelativePaths)
                                    writer.WriteLine("<tr><td class=\"error right\">FAILED</td><td class=\"error\">&Oslash;{0:0.00}</td><td class=\"error\"><a href=\"{1}\">{1}</a></td></tr>", r.AverageError, Path.GetFileName(r.FileName));
                                else
                                    writer.WriteLine("<tr><td class=\"error right\">FAILED</td><td class=\"error\">&Oslash;{0:0.00}</td><td class=\"error\"><a href=\"file:///{0}\">{1}</a></td></tr>", r.FileName.Replace("\\", "/"), r.FileName);
                            }
                            else if (r.TotalErrors==-1) // if all results have not been checked, mark as "untested"
                            {
                                if (r.RelativePaths)
                                    writer.WriteLine("<tr><td colspan=\"2\" class=\"untested right\">UNTESTED</td><td class=\"untested\"><a href=\"{0}\">{0}</a></td></tr>", Path.GetFileName(r.FileName));
                                else
                                    writer.WriteLine("<tr><td colspan=\"2\" class=\"untested right\">UNTESTED</td><td class=\"untested\"><a href=\"file:///{0}\">{1}</a></td></tr>", r.FileName.Replace("\\", "/"), r.FileName);
                            }
                            else
                            {
                                if (r.RelativePaths)
                                    writer.WriteLine("<tr><td colspan=\"2\" class=\"ok right\">SUCCEEDED</td><td class=\"ok\"><a href=\"{0}\">{0}</a></td></tr>", Path.GetFileName(r.FileName));
                                else
                                    writer.WriteLine("<tr><td colspan=\"2\" class=\"ok right\">SUCCEEDED</td><td class=\"ok\"><td class=\"ok\"><a href=\"file:///{0}\">{1}</a></td></tr>", r.FileName.Replace("\\", "/"), r.FileName);
                            }

                        }
                    }

                    writer.WriteLine("</table>");
                    writer.WriteLine(@"</div>
  </body>
</html>
");
                }
                log.WriteLine("Metareport has been written to: {0}", this.FileName);
            }
            else
            {
                log.WriteLine("Skipping generation of metareport as \"--nometareport\" has been set.");
                foreach (Report r in _reports)
                    if (!r.WriteReport(log, _path.FullName, options))
                        log.Error("Error writing report to {0}", r.FileName);
            }
            return bRet;
        }
    }

    /// This represents the html report that is generated per checked result
    [Serializable]
    public sealed class Report : IDisposable
    {
        private string _path;
        private string _message;
        private string _metaPath;
        private List<Chart> _chart = new List<Chart>();
        private List<string> _data = new List<string>();
        private double _tolerance;
        private double _dAvErr = 0;
        private int _iTotalErrors = -1;
        private bool _bRelative = false;
        private bool _bInlineScripts = false;

        public double Tolerance
        {
            get { return _tolerance; }
            set { _tolerance = value; }
        }
        public string Message { get { return _message; } set { _message = value; } }
        public string FileName { get { return _path; } set { _path = value; } }
        public string MetaPath { get { return _metaPath; } set { _metaPath = value; } }
        public List<string> Data { get { return _data; } }
        public List<Chart> Chart { get { return _chart; } }
        public string BaseFile { get; set; }
        public string CompareFile { get; set; }
        public bool RelativePaths { get { return _bRelative; } set { _bRelative = value; } }
        public bool UseInlineScripts { get { return _bInlineScripts; } set { _bInlineScripts = value; } }
        public int TotalErrors
        {
            get
            {
                if (_iTotalErrors < 0)
                {
                    _iTotalErrors = 0;
                    foreach (Chart c in _chart)
                        _iTotalErrors += c.Errors;
                }
                return _iTotalErrors;
            }
        }
        public double AverageError
        {
            get
            {
                if (_dAvErr == 0)
                    _dAvErr = (from c in _chart where c.DeltaError > 0 select c.DeltaError).Sum() / (from c in _chart where c.DeltaError > 0 select c.DeltaError).Count();
                return _dAvErr;
            }
        }

        public Report(string sFilePath) { _path = sFilePath; }

        public bool WriteReport(Log log, string metaPath, Options options)
        {
            if (null != metaPath)
                _metaPath = metaPath;
            else
                _metaPath = string.Empty;

            if (!string.IsNullOrEmpty(options.ReportDir))
                _path = Path.Combine(options.ReportDir, Path.GetFileName(_path));
            else
            {
                try
                {
                    _path = Path.GetFullPath(_path);
                }
                catch (PathTooLongException)
                {
                    log.Error("The report path \"{0}\" is too long for the filesystem. Cannot write this report", _path);
                    return false;
                }
            }
            bool bRet = false;

            if (_bRelative)
                _path = Path.Combine(Path.GetDirectoryName(_metaPath), _path);

            if (!options.OverrideOutput && File.Exists(_path))
            {
                _path = Path.Combine(Path.GetDirectoryName(_path), string.Format(CultureInfo.CurrentCulture, "{0:yyyy-MM-ddTHH-mm-ss}-{1}", DateTime.Now, Path.GetFileName(_path)));
                log.WriteLine(LogLevel.Warning, "Report already exists and --override has been set to false. Changed target filename to \"{0}\"", _path);
            }
            using (TextWriter writer = new StreamWriter(_path, false))
            {
                WriteHeader(writer);
                WriteChart(writer);
                WriteFooter(writer);
                bRet = true;
            }
            log.WriteLine("Report has been written to: {0}", _path);

            if (!this._bInlineScripts && !options.UseBitmapPlots)//Save script files from ressource to file system
            {
                Assembly ass = Assembly.GetExecutingAssembly();

                foreach (string s in ass.GetManifestResourceNames())
                    using (Stream stream = ass.GetManifestResourceStream(s))
                    {
                        string sPath = Path.GetDirectoryName(_path);
                        if (s.ToLowerInvariant().EndsWith(".js"))
                            sPath = Path.Combine(sPath, "js");
                        else if (s.ToLowerInvariant().EndsWith(".css"))
                            sPath = Path.Combine(sPath, "css");
                        else
                            continue;

                        if (!Directory.Exists(sPath))
                            Directory.CreateDirectory(sPath);

                        sPath = Path.Combine(sPath, s);

                        if (!File.Exists(sPath))
                            using (FileStream fileStream = File.Create(sPath, (int)stream.Length))
                            {
                                // Initialize the bytes array with the stream length and then fill it with data
                                byte[] bytesInStream = new byte[stream.Length];
                                stream.Read(bytesInStream, 0, bytesInStream.Length);
                                // Use write method to write to the file specified above
                                fileStream.Write(bytesInStream, 0, bytesInStream.Length);
                            }
                    }
            }

            //Clear big data after writing
            this._chart.Clear();
            this._data.Clear();

            return bRet;
        }

        private static void WriteFooter(TextWriter writer)
        {
            writer.WriteLine(@"</div>
  </body>
</html>
");
        }

        private void WriteChart(TextWriter writer)
        {
            foreach (Chart ch in _chart)
                if (!ch.UseBitmap)
                    writer.WriteLine(ch.RenderChart());
                else
                    writer.WriteLine(ch.RenderBitmap(this._path));
        }

        private void WriteHeader(TextWriter writer)
        {
            writer.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">");
            writer.WriteLine("  <head>");
            writer.WriteLine("  <title>Report " + DateTime.Now.ToString() + @"</title>");

            Assembly ass = Assembly.GetExecutingAssembly();
            List<string> lScripts = new List<string>();
            foreach (string s in ass.GetManifestResourceNames())
            {
                if (!s.ToLowerInvariant().EndsWith(".js"))
                    continue;
                if (this._bInlineScripts)
                {
                    var javascriptHeaders = new StreamReader(ass.GetManifestResourceStream(s));
                    lScripts.Add(string.Format("<script language=\"javascript\" type=\"text/javascript\">{0}</script>", javascriptHeaders.ReadToEnd()));
                }
                else
                    lScripts.Add(string.Format("<script src=\"js/{0}\"></script>", s));
            }
            lScripts = lScripts.OrderByDescending(x => x).ToList<string>();//Sort alphabetically to ensure jquery is loaded first
            writer.WriteLine(string.Join(Environment.NewLine, lScripts.ToArray()));
            if (this._bInlineScripts)
            {
                writer.WriteLine("<style type=\"text/css\">");
                writer.WriteLine(".jqplot-target{position:relative;color:#666;font-family:\"Trebuchet MS\",Arial,Helvetica,sans-serif;font-size:1em;}.jqplot-axis{font-size:.75em;}.jqplot-xaxis{margin-top:10px;}.jqplot-x2axis{margin-bottom:10px;}.jqplot-yaxis{margin-right:10px;}.jqplot-y2axis,.jqplot-y3axis,.jqplot-y4axis,.jqplot-y5axis,.jqplot-y6axis,.jqplot-y7axis,.jqplot-y8axis,.jqplot-y9axis{margin-left:10px;margin-right:10px;}.jqplot-axis-tick,.jqplot-xaxis-tick,.jqplot-yaxis-tick,.jqplot-x2axis-tick,.jqplot-y2axis-tick,.jqplot-y3axis-tick,.jqplot-y4axis-tick,.jqplot-y5axis-tick,.jqplot-y6axis-tick,.jqplot-y7axis-tick,.jqplot-y8axis-tick,.jqplot-y9axis-tick{position:absolute;}.jqplot-xaxis-tick{top:0;left:15px;vertical-align:top;}.jqplot-x2axis-tick{bottom:0;left:15px;vertical-align:bottom;}.jqplot-yaxis-tick{right:0;top:15px;text-align:right;}.jqplot-yaxis-tick.jqplot-breakTick{right:-20px;margin-right:0;padding:1px 5px 1px 5px;z-index:2;font-size:1.5em;}.jqplot-y2axis-tick,.jqplot-y3axis-tick,.jqplot-y4axis-tick,.jqplot-y5axis-tick,.jqplot-y6axis-tick,.jqplot-y7axis-tick,.jqplot-y8axis-tick,.jqplot-y9axis-tick{left:0;top:15px;text-align:left;}.jqplot-meterGauge-tick{font-size:.75em;color:#999;}.jqplot-meterGauge-label{font-size:1em;color:#999;}.jqplot-xaxis-label{margin-top:10px;font-size:11pt;position:absolute;}.jqplot-x2axis-label{margin-bottom:10px;font-size:11pt;position:absolute;}.jqplot-yaxis-label{margin-right:10px;font-size:11pt;position:absolute;}.jqplot-y2axis-label,.jqplot-y3axis-label,.jqplot-y4axis-label,.jqplot-y5axis-label,.jqplot-y6axis-label,.jqplot-y7axis-label,.jqplot-y8axis-label,.jqplot-y9axis-label{font-size:11pt;position:absolute;}table.jqplot-table-legend{margin-top:12px;margin-bottom:12px;margin-left:12px;margin-right:12px;}table.jqplot-table-legend,table.jqplot-cursor-legend{background-color:rgba(255,255,255,0.6);border:1px solid #ccc;position:absolute;font-size:.75em;}td.jqplot-table-legend{vertical-align:middle;}td.jqplot-seriesToggle:hover,td.jqplot-seriesToggle:active{cursor:pointer;}td.jqplot-table-legend>div{border:1px solid #ccc;padding:1px;}div.jqplot-table-legend-swatch{width:0;height:0;border-top-width:5px;border-bottom-width:5px;border-left-width:6px;border-right-width:6px;border-top-style:solid;border-bottom-style:solid;border-left-style:solid;border-right-style:solid;}.jqplot-title{top:0;left:0;padding-bottom:.5em;font-size:1.2em;}table.jqplot-cursor-tooltip{border:1px solid #ccc;font-size:.75em;}.jqplot-cursor-tooltip{border:1px solid #ccc;font-size:.75em;white-space:nowrap;background:rgba(208,208,208,0.5);padding:1px;}.jqplot-highlighter-tooltip{border:1px solid #ccc;font-size:.75em;white-space:nowrap;background:rgba(208,208,208,0.5);padding:1px;}.jqplot-point-label{font-size:.75em;z-index:2;}td.jqplot-cursor-legend-swatch{vertical-align:middle;text-align:center;}div.jqplot-cursor-legend-swatch{width:1.2em;height:.7em;}.jqplot-error{text-align:center;}.jqplot-error-message{position:relative;top:46%;display:inline-block;}div.jqplot-bubble-label{font-size:.8em;padding-left:2px;padding-right:2px;color:rgb(20%,20%,20%);}div.jqplot-bubble-label.jqplot-bubble-label-highlight{background:rgba(90%,90%,90%,0.7);}div.jqplot-noData-container{text-align:center;background-color:rgba(96%,96%,96%,0.3);}");
                writer.WriteLine("body{ background: #EEEEEE; color: #000; text-align: center;}");
                writer.WriteLine("body, table{ font-family: Arial, Helvetica, sans-serif; font-size: 12px; }");
                writer.WriteLine("#page{ width: 700px; margin: auto; text-align: left; background-color: #FFF;}");
                writer.WriteLine("table.info{ border: 0; width: 690px; margin: 5px;}");
                writer.WriteLine("table.info td{ background-color: #efefef; padding: 1em;}");
                writer.WriteLine("table.info td.header{ font-weight: bold; width: 150px; background-color: #EEE;}");
                writer.WriteLine("tr.error td, tr.error td.header { background-color: #F5A9BC; color: red; }");
                writer.WriteLine("tr.warning td, tr.warning td.header { background-color: #FFCC66; color: #FF6600; }");
                writer.WriteLine("h1{ font-size: 16px; padding: 1em; }");
                writer.WriteLine("</style>");
            }
            else
            {
                writer.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"css/CsvCompare.Resources.jquery.jqplot.min.css\">");
                writer.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"css/CsvCompare.Resources.style.css\">");
            }
            writer.WriteLine("</head>"); 
            writer.WriteLine("<body>");
            writer.WriteLine("<div id=\"page\">");
            writer.WriteLine("<a id=\"top\"/>");
            writer.WriteLine("<h1>{0}</h1>", this._path);
            writer.WriteLine("<table class=\"info\">");
            if (!String.IsNullOrEmpty(_metaPath))
                if (!_bRelative)
                    writer.WriteLine("	<tr><td class=\"header\">Meta Report:</td><td><a href=\"file:///{0}\">{1}</a></td></tr>", _metaPath.Replace("\\", "/"), _metaPath);
                else
                    writer.WriteLine("	<tr><td class=\"header\">Meta Report:</td><td><a href=\"{0}\">{1}</a></td></tr>", Path.GetFileName(_metaPath), Path.GetFileName(_metaPath));
            
            if(null != this.BaseFile)
            writer.WriteLine("	<tr><td class=\"header\">Base File:</td><td><a href=\"file:///{0}\">{1}</a></td></tr>", this.BaseFile.Replace("\\", "/"), this.BaseFile);
            if (null != this.CompareFile)
                writer.WriteLine("	<tr><td class=\"header\">Compare File:</td><td><a href=\"file:///{0}\">{1}</a></td></tr>", this.CompareFile.Replace("\\", "/"), this.CompareFile);
            
            writer.WriteLine("	<tr><td class=\"header\">Tolerance:</td><td>{0}</td></tr>", _tolerance);
            writer.WriteLine("	<tr><td class=\"header\">Tested:</td><td>{0} [UTC]</td></tr>", DateTime.UtcNow);

            int iTested = _chart.Count - (from c in _chart where c.Errors == -1 select c).Count();
            int iErrors = (from c in _chart where c.Errors > 0 && c.Errors != -1 select c).Count();
            double dSuccess;

            if(iTested<=0)
               dSuccess = 0;
            else
               dSuccess = ((1 - ((double)iErrors / (double)iTested)));
            
            writer.WriteLine("	<tr><td class=\"header\">&nbsp;</td><td>The compare file contained {0} results. {1} results have been tested. {2} failed, success rate is {3:0.0%}.</td></tr>",
                _chart.Count,   //All results
                iTested,    //All tested results
                iErrors,   //Errors
                dSuccess);
            writer.Write("  <tr><td class=\"header\">Average relative error:</td><td>{0:0.00}</td></tr>", this.AverageError);
            writer.Write("  <tr><td class=\"header\">Failed Tests:</td><td>");
            foreach (Chart c in (from ch in _chart where ch.Errors > 0 select ch))
                writer.WriteLine("<a href=\"#a{0}\">{1}</a><br/>", c.Id, c.Title);
            writer.WriteLine("</td></tr>");
            writer.WriteLine("</table><hr/>");
        }

        #region IDisposable Member

        public void Dispose()
        {
            this._data.Clear();
            _message = null;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
