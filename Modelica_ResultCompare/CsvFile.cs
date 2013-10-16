using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CsvCompare
{
    /// This class parses CSV files and holds results in a dictionary
    public class CsvFile
    {
        private double _dRangeDelta = 0.002;
        private string _fileName = string.Empty;
        private List<double> _xAxis = new List<double>();
        private Dictionary<string, List<double>> _values = new Dictionary<string, List<double>>();
        private bool _bShowRelativeErrors = true;

        /// Holds values for x axis (time)
        public List<double> XAxis { get { return _xAxis; } }
        /// Holds values for the results in a dictionary.
        /// 
        /// The key is the result identifier.
        public Dictionary<string, List<double>> Results { get { return _values; } }
        /// This value can be used to produce a offset between base and comparison values
        public double RangeDelta { get { return _dRangeDelta; } set { _dRangeDelta = value; } }
        /// This value enables/disables relative error differences in the error graph
        public bool ShowRelativeErrors
        {
            get { return _bShowRelativeErrors; }
            set { _bShowRelativeErrors = value; }
        }

        /// The constructor reads the csv file to this object
        /// @para filename The full path of the csv file
        /// @para dTolerance Double value containing the delta for the tube generation
        public CsvFile(string fileName, Options options, Log log)
        {
            //Parse tolerance from commandline
            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";

            //understand 0.002
            if (!Double.TryParse(options.Tolerance, NumberStyles.AllowDecimalPoint, provider, out _dRangeDelta))
            {
                //understand 0,002
                provider.NumberDecimalSeparator = ",";
                if (!Double.TryParse(options.Tolerance, NumberStyles.AllowDecimalPoint, provider, out _dRangeDelta))
                    //understand 2e-2 etc.
                    if (!Double.TryParse(options.Tolerance, out _dRangeDelta))
                        log.WriteLine(LogLevel.Warning, "could not parse given tolerance argument: \"{0}\", using default \"{1}\".", options.Tolerance, _dRangeDelta);
            }

            if (File.Exists(fileName))
            {
                _fileName = Path.GetFullPath(fileName);
                using (TextReader reader = new StreamReader(fileName))
                {
                    string sLine = reader.ReadLine();
                    if (null == sLine)
                        throw new ArgumentNullException(string.Format("\"{0}\" is empty, nothing to parse here.", fileName));

                    List<string> map = new List<string>();

                    //skip comments
                    while (!string.IsNullOrEmpty(sLine) && sLine.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                        sLine = reader.ReadLine();

                    Regex reg = new Regex(string.Format(CultureInfo.CurrentCulture, "{0}(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))", options.Delimiter));

                    //read the columns from the first line
                    string[] values = reg.Split(sLine);

                    //one element means the line has not been parsed correctly
                    if (null == values || values.Length == 1)
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The file {0} could not be parsed. Maybe the wrong delimiter is set? It has been set to \"{1}\".", fileName, options.Delimiter));

                    foreach (string sCol in values)
                        if (!string.IsNullOrEmpty(sCol))
                        {
                            string sTemp=sCol.Trim(' ', '"', '\t');
                            if (sTemp != "t" && sTemp != "time" && sTemp != "Time")//Skip time values
                            {
                                try
                                {
                                    _values.Add(sTemp, new List<double>());
                                    map.Add(sTemp);
                                }
                                catch (ArgumentException)
                                {
                                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Error parsing the csv file \"{0}\". The result {1} is already in the list (maybe you set no or a wrong delimiter for the parser? I used \"{2}\").",
                                        fileName, sTemp, options.Delimiter));
                                }
                            }
                        }

                    //read the rest of the csv file
                    while ((sLine = reader.ReadLine()) != null)
                    {
                        //Skip comments
                        if (sLine.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                            continue;

                        values = reg.Split(sLine);
                        int iCol = 0;

                        //read values to the dictionary
                        foreach (string sCol in values)
                        {
                            double dValue;
                            if (!Double.TryParse(sCol.Trim('"'), NumberStyles.Any, provider, out dValue))
                            {
                                if(!string.IsNullOrEmpty(sCol))
                                    log.WriteLine(LogLevel.Warning, "Could not parse string \"{0}\" as double value, skipping.", sCol);
                                iCol++;
                                continue;
                            }

                            if (iCol == 0)
                                _xAxis.Add(dValue);
                            else
                                try
                                {
                                    _values[map[iCol - 1]].Add(dValue);
                                }
                                catch (KeyNotFoundException)
                                {
                                    break;
                                }

                            iCol++;
                        }
                    }

                    if (_xAxis.Count <= 1)
                        throw new ArgumentNullException(string.Format(CultureInfo.CurrentCulture, "{0} could not be parsed and might be an invalid csv file.", fileName));
                }
            }
            else
                throw new FileNotFoundException();
        }

        public override string ToString()
        {
            return _fileName;
        }

        public Report PlotCsvFile(string sReportPath, Log log)
        {
            Report r = new Report(sReportPath);
            log.WriteLine("Generating plot for report");

            foreach (KeyValuePair<string, List<double>> res in _values)
                PrepareCharts(r, res.Value.ToArray<double>(), res);

            return r;
        }
        public Report CompareFiles(Log log, CsvFile csvBase)
        {
            if (null != _fileName)
                if (Path.GetDirectoryName(_fileName).Length > 0)
                    return CompareFiles(log, csvBase, string.Format(CultureInfo.CurrentCulture, "{0}\\{1}_report.html", Path.GetDirectoryName(_fileName), Path.GetFileNameWithoutExtension(csvBase.ToString())));
                else
                    return CompareFiles(log, csvBase, Path.GetFileNameWithoutExtension(_fileName) + ".html");
            else
                return CompareFiles(log, csvBase, null);
        }
        public Report CompareFiles(Log log, CsvFile csvBase, string sReportPath)
        {
            int iInvalids = 0;

            Report rep = new Report(sReportPath);
            log.WriteLine("Comparing \"{0}\" to \"{1}\"", _fileName, csvBase.ToString());

            List<double> lXHighTube = new List<double>();
            List<double> lYHighTube = new List<double>();
            List<double> lXLowTube = new List<double>();
            List<double> lYLowTube = new List<double>();
            List<double> lvYHighTube = new List<double>();
            List<double> lvYLowTube = new List<double>();
            List<double> lErrorsX = new List<double>();
            List<double> lErrorsY = new List<double>();

            double[] darResults = null;

            rep.BaseFile = csvBase.ToString();
            rep.CompareFile = _fileName;

            foreach (KeyValuePair<string, List<double>> res in csvBase.Results)
            {
                Range r = new Range();
                int iError = -1;
                bool bSkipValidation = false;
                lXHighTube.Clear();
                lXLowTube.Clear();
                lYHighTube.Clear();
                lYLowTube.Clear();
                lvYHighTube.Clear();
                lvYLowTube.Clear();
                lErrorsX.Clear();
                lErrorsY.Clear();
                darResults = null;

                if (!this.Results.ContainsKey(res.Key))
                {
                    log.WriteLine(LogLevel.Warning, "{0} not found in \"{1}\", skipping checks.", res.Key, this._fileName);
                    bSkipValidation = true;
                }
                else
                {
                    try
                    {
                        if (res.Value.Count == 0)
                        {
                            log.Error("{0} has no y-Values! Maybe error during parsing? Skipping", res.Key);
                            continue;
                        }

                        log.WriteLine("Generating tubes for {0}", res.Key);
                        r.CalculateTubes(csvBase.XAxis.ToArray(), res.Value.ToArray(), lXHighTube, lYHighTube, lXLowTube, lYLowTube, _dRangeDelta, 1);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        log.Error("Error in the calculation of the tubes. Skipping {0}", res.Key);
                        rep.Chart.Add(new Chart() { Title = res.Key, Errors = 1 });
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        log.Error("Error in the calculation of the tubes. Skipping {0}", res.Key);
                        rep.Chart.Add(new Chart() { Title = res.Key, Errors = 1 });
                        continue;
                    }
                    catch (Exception ex)//Should never be fired
                    {
                        log.Error("Exception in the calculation of the tubes. Skipping {0} [Message: {1}]", res.Key, ex.Message);
                        rep.Chart.Add(new Chart() { Title = res.Key, Errors = 1 });
                    }

                    if (csvBase.XAxis.Count < this.XAxis.Count)
                        log.WriteLine(LogLevel.Warning, "The resolution of the base x-axis is smaller than the compare x-axis. The better the base resolution is, the better the validation result will be!");
                    else
                        log.WriteLine(LogLevel.Debug, "The resolution of the base x-axis is good.");

                    log.WriteLine(LogLevel.Debug, "Calibrating timelines");

                    darResults = Range.CalibrateValues(this.XAxis.ToArray(), csvBase.XAxis.ToArray(), this.Results[res.Key].ToArray<double>());
                    lvYHighTube = Range.CalibrateValues(lXHighTube.ToArray(), csvBase.XAxis.ToArray(), lYHighTube.ToArray()).ToList<double>();
                    lvYLowTube = Range.CalibrateValues(lXLowTube.ToArray(), csvBase.XAxis.ToArray(), lYLowTube.ToArray()).ToList<double>();

                    Range.RelativeErrors = _bShowRelativeErrors;
                    iError = Range.Validate(lvYLowTube, lvYHighTube, darResults.ToList<double>(), csvBase.XAxis, ref lErrorsX, ref lErrorsY);

                    if (iError == 0)
                        log.WriteLine(res.Key + " is valid");
                    else
                    {
                        log.WriteLine(LogLevel.Warning, "{0} is invalid! {1} errors have been found during validation.", res.Key, iError);
                        iInvalids++;
                        Environment.ExitCode = 1;
                    }
                }

                PrepareCharts(csvBase.XAxis, rep, lXHighTube, lYHighTube, lXLowTube, lYLowTube, lvYHighTube, lvYLowTube, lErrorsY, darResults, res, iError, bSkipValidation);
            }
            rep.Tolerance = _dRangeDelta;

            return rep;
        }

        private void PrepareCharts(Report rep, double[] darResults, KeyValuePair<string, List<double>> res)//Draw result only
        {
            PrepareCharts(_xAxis, rep, null, null, null, null, null, null, null, darResults, res, 0, true);
        }

        private static void PrepareCharts(List<double> xAxis, Report rep, List<double> lXHighTube, List<double> lYHighTube, List<double> lXLowTube, List<double> lYLowTube, List<double> lvYHighTube, List<double> lvYLowTube, List<double> lErrorsY, double[] darResults, KeyValuePair<string, List<double>> res, int iError, bool bSkipValidation)
        {
            Chart ch = new Chart()
            {
                LabelX = "Time",
                LabelY = res.Key,
                Errors = iError,
                Title = res.Key
            };

            if (!bSkipValidation)
            {
                ch.Series.Add(new Series()
                {
                    Color = Color.Orange,
                    ArrayString = Series.GetArrayString(xAxis, res.Value),
                    Title = "Base (to compare with)"
                });
                ch.Series.Add(new Series()
                {
                    Color = Color.Green,
                    ArrayString = Series.GetArrayString(xAxis, darResults.ToList<double>()),
                    Title = "Result"
                });
#if DEBUG               //Draw uncalibrated tubes in Debug mode
                ch.Series.Add(new Series()
                {
                    Color = Color.Pink,
                    ArrayString = Series.GetArrayString(lXHighTube, lYHighTube),
                    Title = "High Tube"
                });
                ch.Series.Add(new Series()
                {
                    Color = Color.Pink,
                    ArrayString = Series.GetArrayString(lXLowTube, lYLowTube),
                    Title = "Low Tube"
                });
#endif
                ch.Series.Add(new Series()
                {
                    Color = Color.LightBlue,
                    ArrayString = Series.GetArrayString(xAxis, lvYLowTube),
                    Title = "Calibrated Low Tube"
                });
                ch.Series.Add(new Series()
                {
                    Color = Color.LightGreen,
                    ArrayString = Series.GetArrayString(xAxis, lvYHighTube),
                    Title = "Calibrated High Tube"
                });
            }
            else
            {
                ch.Series.Add(new Series()
                {
                    Color = Color.Green,
                    ArrayString = Series.GetArrayString(xAxis, res.Value),
                    Title = "Compare"
                });
            }
            if (iError > 0)
            {
                ch.Series.Add(new Series()
                {
                    Color = Color.DarkGoldenrod,
                    ArrayString = Series.GetArrayString(xAxis, lErrorsY),
                    Title = "ERRORS"
                });
            }
            rep.Chart.Add(ch);
        }

        public void Save(Options options)
        {
            Save(Path.GetDirectoryName(_fileName), options);
        }
        public void Save(string sFolderName, Options options)
        {
            string sFilename = string.Format(CultureInfo.CurrentCulture, "{0}{1}.generated.csv", sFolderName, Path.GetFileNameWithoutExtension(_fileName));
            NumberFormatInfo provider = new NumberFormatInfo();

            provider.NumberDecimalSeparator = ".";

            using (TextWriter writer = new StreamWriter(sFilename))
            {
                writer.Write(string.Format(provider, "\"time\"{0}", options.Delimiter));

                foreach (KeyValuePair<string, List<double>> res in _values)
                    writer.Write(string.Format(provider, "\"{0}\"{1}", res.Key, options.Delimiter));

                writer.WriteLine();

                int iCount = 0;
                foreach (double dTime in _xAxis)
                {
                    writer.Write(string.Format(provider, "\"{0}\"{1}", dTime, options.Delimiter));
                    foreach (KeyValuePair<string, List<double>> res in _values)
                    {
                        try
                        {
                            if (res.Value.Count == 0)
                                continue;

                            writer.Write(string.Format(provider, "\"{0}\"{1}", res.Value[iCount], options.Delimiter));
                        }
                        catch (ArgumentException) { }
                    }
                    
                    writer.WriteLine();
                    iCount++;
                }
            }
        }
    }
}
