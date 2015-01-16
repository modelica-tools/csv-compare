// CurveCompare.cs
// author: Susanne Walther
// date: 05.01.2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
#if GUI
using System.Windows.Forms;
#endif
using CurveCompare.DataImport;

namespace CurveCompare
{
    /// <summary>
    /// The class provides methods for comparison of curves.  
    /// </summary>
    public class CurveCompare
    {
        /// <summary>
        /// Compares a test curve with a reference curve. Calculates a tube, if test curve data == null.
        /// </summary>
        /// <param name="modelName">Model name.</param>
        /// <param name="resultName">Result name.</param>
        /// <param name="referenceX">x values of reference curve.</param>
        /// <param name="referenceY">y values of reference curve.</param>
        /// <param name="testX">x values of test curve.</param>
        /// <param name="testY">y values of test curve.</param>
        /// <param name="options">Options for calculation of tube size, chart and saving.</param>
        /// <returns>Tube report.</returns>
        public TubeReport Validate(string modelName, string resultName, double[] referenceX, double[] referenceY, double[] testX, double[] testY, IOptions options)
        {
            TubeReport report = new TubeReport();
            Curve refCurve, testCurve;
            bool testExists = (testX != null && testY != null && testX.Length > 0 && testY.Length > 0);
            bool saveImage = (!String.IsNullOrWhiteSpace(options.ReportFolder) && Directory.Exists(options.ReportFolder));
            string name = modelName + " - " + resultName;

            // write log file
            if (options.Log != null)
            {
                options.Log.WriteLine(LogLevel.Done, "----------------------------------------------");
                options.Log.WriteLine(LogLevel.Done, "Model: " + modelName);
                options.Log.WriteLine(LogLevel.Done, "Result: " + resultName);
            }

            if (referenceX != null && referenceY != null && referenceX.Length != 0 && referenceY.Length != 0)
            {
                // Data import: Prepare curve data
                refCurve = new Curve("Reference " + name, referenceX, referenceY);

                if (testExists)
                    testCurve = new Curve("Test " + name, testX, testY);
                else
                    testCurve = new Curve();

                if (refCurve.ImportSuccessful && (testCurve.ImportSuccessful || !testExists))
                {
                    // Calculate tube size
                    TubeSize size = new TubeSize(refCurve);

                    if (!Double.IsNaN(options.BaseX)) // overwrite BaseX just in case it has got a value
                        size.BaseX = options.BaseX;
                    if (!Double.IsNaN(options.BaseY))
                        size.BaseY = options.BaseY;
                    if (!Double.IsNaN(options.Ratio))
                        size.Ratio = options.Ratio;

                    if (options is Options1)
                        size.Calculate(((Options1)options).Value, ((Options1)options).Axes, options.Relativity);
                    else if (options is Options2)
                        size.Calculate(((Options2)options).X, ((Options2)options).Y, options.Relativity);

                    if (size.Successful)
                    {
                        // Calculate tube
                        Tube tube = new Tube(size);
                        tube.AlgorithmOption = Algorithms.AlgorithmOptions.Rectangle;
                        report = tube.Calculate(refCurve);

                        if (tube.TubeSuccessful)
                        {
                            if (testExists)
                            {
                                // Validation
                                report = tube.Validate(testCurve);

                                if (options.Log != null)
                                {
                                    options.Log.WriteLine(LogLevel.Done, "Test curve is " + report.Valid.ToString());
                                    options.Log.WriteLine(LogLevel.Done, "Errors (points of test curve outside tube): " + report.Errors.Count.ToString());
                                }

                                if (tube.ValidationSuccessful)
                                    report.ErrorStep = Step.None;

                                // Error: Validation not successful
                                else
                                {
                                    report.ErrorStep = Step.Validation;
                                    report.Valid = Validity.Undefined;
                                    if (options.Log != null)
                                        options.Log.WriteLine(LogLevel.Error, "Validation not successful.");
                                }
                            }
                            else
                            {
                                report.ErrorStep = Step.Validation;
                                report.Valid = Validity.Undefined;
                                if (options.Log != null)
                                    options.Log.WriteLine(LogLevel.Error, "no test curve data.");
                            }

#if GUI
                            // Visualization
                            if (saveImage || options.ShowWindow)
                            {
                                ChartControl chartControl = new ChartControl(options.DrawFastAbove, options.DrawPointsBelow, options.DrawLabelNumber);
                                chartControl.addTitle("Tube size: (" + size.X + "; " + size.Y + ")");
                                chartControl.AddLine(refCurve.Name, refCurve, Color.FromKnownColor(KnownColor.OrangeRed));
                                chartControl.AddLine("Upper", report.Upper, Color.FromKnownColor(KnownColor.MediumSpringGreen));
                                chartControl.AddLine("Lower", report.Lower, Color.FromKnownColor(KnownColor.DeepSkyBlue));

                                if (testExists)
                                {
                                    string valid = "";
                                    if (report.Valid == Validity.Valid)
                                        valid = "valid";
                                    else if (report.Valid == Validity.Invalid)
                                        valid = "invalid";
                                    chartControl.AddLine("Test: " + valid, testCurve, Color.FromKnownColor(KnownColor.BlueViolet));
                                    chartControl.AddErrors("Errors " + name, report.Errors);
                                }

                                // Visualization: save image
                                if (saveImage)
                                    chartControl.saveAsImage(options.ReportFolder + resultName + ".png", System.Drawing.Imaging.ImageFormat.Png);

                                // Visualization: show window with MS Chart Control
                                if (options.ShowWindow)
                                    if (!testExists || (options.ShowValidity == Validity.All || (options.ShowValidity == Validity.Invalid && report.Valid == Validity.Invalid) || (options.ShowValidity == Validity.Valid && report.Valid == Validity.Valid)))
                                        Application.Run(chartControl);

                                
                            }
#endif
                        }
                        // Error: tube calculation not successful
                        else
                        {
                            report.ErrorStep = Step.Tube;
                            report.Valid = Validity.Undefined;

                            if (options.Log != null)
                                options.Log.WriteLine(LogLevel.Error, "Tube calculation not successful.");
                        }

                        if (options.Log != null)
                        {
                            options.Log.WriteLine(LogLevel.Done, "Tube calculation algorithm: " + tube.AlgorithmOption.ToString());
                        }
                    }
                    // Error: tube size calculation not successful
                    else
                    {
                        report.ErrorStep = Step.TubeSize;
                        report.Valid = Validity.Undefined;

                        if (options.Log != null)
                        {
                            options.Log.WriteLine(LogLevel.Error, "TubeSize calculation not successful.");
                            options.Log.WriteLine(LogLevel.Error, "TubeSize.Ratio: " + size.Ratio);
                            options.Log.WriteLine(LogLevel.Error, "TubeSize.BaseX: " + size.BaseX);
                            options.Log.WriteLine(LogLevel.Error, "TubeSize.BaseY: " + size.BaseY);
                        }
                    }
                    report.Size = size;
                }
                // Error: data import not successful
                else
                {
                    report.ErrorStep = Step.DataImport;
                    report.Valid = Validity.Undefined;

                    if (options.Log != null)
                    {
                        if (!refCurve.ImportSuccessful)
                            options.Log.WriteLine(LogLevel.Error, "Reference curve: Import not successful.");
                        if (testExists && !testCurve.ImportSuccessful)
                            options.Log.WriteLine(LogLevel.Error, "Test curve: Import not successful.");
                    }
                }
                report.Reference = refCurve;
                report.Test = testCurve;
            }
            // Error: no data 
            else
            {
                report.ErrorStep = Step.DataImport;
                report.Valid = Validity.Undefined;

                if (options.Log != null) 
                {
                    if (referenceX != null || referenceX.Length != 0)
                        options.Log.WriteLine(LogLevel.Error, "Reference curve: Missing data.");
                    if (referenceY!= null || referenceY.Length != 0)
                        options.Log.WriteLine(LogLevel.Error, "Test curve: Missing data.");
                }
            }

            report.ModelName = modelName;
            report.ResultName = resultName;
            return report;
        }
        /// <summary>
        /// Compares a (set of) test curve(s) with a (set of) reference curve(s). Calculates a tube, if test curve data == null.
        /// </summary>
        /// <param name="modelName">Name, e.g. model name.</param>
        /// <param name="referenceFileName">Full path name of file with data of reference curve.</param>
        /// <param name="testFileName">Full path name of file with data of test curve.</param>
        /// <param name="result">Result identifier. Identities the result in reference and test, that shall be compared. Set result = "" for comparing all results in reference file.</param>
        /// <param name="options">Options for calculation of tube size, chart and saving.</param>
        /// <returns>List of tube reports.</returns>
        public List<TubeReport> ValidateCSV(string modelName, string referenceFileName, string testFileName, string result, IOptions options, ReadOptions readOptions)
        {
            List<TubeReport> reportList = new List<TubeReport>();
            TubeReport report;
            CsvFile refCsvFile;
            CsvFile testCsvFile = null;
            int CalculationFailedCount = 0;
            int validCount = 0;
            int invalidCount = 0;
            bool useGivenResult = !String.IsNullOrWhiteSpace(result);
            bool testExists = (!String.IsNullOrWhiteSpace(testFileName) && (File.Exists(testFileName)));
            double[] refX, refY, testX, testY;
           
            if (String.IsNullOrWhiteSpace(referenceFileName) || (!File.Exists(referenceFileName)))
                return reportList;
           
            // read curve data, set arrays with x values
            refCsvFile = new CsvFile(referenceFileName, readOptions, options.Log);
            refX = refCsvFile.XAxis.ToArray();
            if (testExists)
            {
                testCsvFile = new CsvFile(testFileName, readOptions, options.Log);
                testX = testCsvFile.XAxis.ToArray();
            }
            else
                testX = null;

            // for each curve in the set of curves from reference file
            foreach (KeyValuePair<string, List<double>> entry in refCsvFile.Results)
            {
                if (!useGivenResult)
                    result = entry.Key;

                // Validate, if there is a curve with the same key in the set of curves in the test file or just calculate tube if !testExists
                if (testExists && !testCsvFile.Results.ContainsKey(result))
                    continue;

                // set arrays with y values
                refY = refCsvFile.Results[result].ToArray();
                if (testExists) 
                    testY = testCsvFile.Results[result].ToArray();
                else
                    testY = null;

                report = Validate(modelName, result, refX, refY, testX, testY, options);

                if (report.Valid == Validity.Valid)
                    validCount++;
                else if (report.Valid == Validity.Invalid)
                    invalidCount++;
                if (report.ErrorStep != Step.None)
                    CalculationFailedCount++;
                reportList.Add(report);

                if (useGivenResult)
                    break;
            }
            if (options.Log != null)
            {
                // write in log file
                options.Log.WriteLine(LogLevel.Done, "-----------------------------------------------------------------------");
                options.Log.WriteLine(LogLevel.Done, "Calculation failed: " + CalculationFailedCount);
                options.Log.WriteLine(LogLevel.Done, "Valid Results: " + validCount);
                options.Log.WriteLine(LogLevel.Done, "Invalid Results: " + invalidCount);
            }
            return reportList;
        }
        /// <summary>
        /// Find the defective step for each TubeReport in a List of TubeReport
        /// </summary>
        /// <param name="reportList">List of TubeReport</param>
        /// <returns>List of defective steps.</returns>
        public List<TubeReport> findErrorReports(List<TubeReport> reports, Log log) 
        {
           List<TubeReport> errorReports = new List<TubeReport>();
           foreach (TubeReport report in reports)
                        if (report.ErrorStep != Step.None)
                            errorReports.Add(report);
           if (log != null)
           {
               log.WriteLine(LogLevel.Done, "Calculation errors in : " + errorReports.Count.ToString() + " results");
           }
           return errorReports;
        }
        
    }
}
