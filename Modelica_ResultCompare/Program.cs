/// Copyright (c) 2013, ITI GmbH Dresden
/// All rights reserved.
///
/// Redistribution and use in source and binary forms, with or without
/// modification, are permitted provided that the following conditions are met:
///
///  Redistributions of source code must retain the above copyright notice, 
///  this list of conditions and the following disclaimer.
///  Redistributions in binary form must reproduce the above copyright notice,
///  this list of conditions and the following disclaimer in the documentation 
///  and/or other materials provided with the distribution.
///
///  Neither the name of the ITI GmbH nor the names of its contributors may be
///  used to endorse or promote products derived from this software without
///  specific prior written permission.
///
/// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
/// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
/// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
/// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
/// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
/// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
/// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
/// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
/// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
/// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
/// POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace CsvCompare
{
    class Program
    {
        /// global Log object
        private static Log _log = new Log();

        /// The main entry of the application
        /// @para cmdArgs contains an array of commandline parameters that are parsed using CommandLine.Dll
        public static void Main(string[] cmdArgs)
        {
            //Global catch to prevent crash
            try { Run(cmdArgs); }
            catch (Exception ex)
            {
                if (null != _log)
                    _log.Error("Exception during run: {0}", ex.Message);
                else
                    Console.Error.WriteLine("Exception during run: {0}", ex.Message);

                var options = new Options();
                Console.WriteLine(options.GetUsage());
#if DEBUG
                Console.ReadLine();
#endif
                Environment.Exit(2);
            }
        }

        private static void Run(string[] cmdArgs)
        {
            var options = new Options();
            Parser parser = new Parser();
            if (parser.ParseArguments(cmdArgs, options))
            {
                if (options.Verbosity > 0)
                    _log.Verbosity = (LogLevel)options.Verbosity;
                else
#if DEBUG
                    _log.Verbosity = LogLevel.Debug;
#endif
                _log.WriteLine(LogLevel.Debug, "Using CSV Compare Version {0} ({1})", Info.AssemblyVersion, Assembly.GetExecutingAssembly().GetName().ProcessorArchitecture);
                _log.WriteLine(LogLevel.Debug, "Starting new result check @{0}", DateTime.Now);
                _log.WriteLine(LogLevel.Debug, "Parsing commandline options: {0} * {1}", Environment.NewLine, string.Join(" ", cmdArgs));
                _log.WriteLine(LogLevel.Debug, "Successfully parsed the following options:");
                _log.WriteLine(LogLevel.Debug, "Operation mode is {0}", options.Mode);
                _log.WriteLine(LogLevel.Debug, "Tolerance is {0}", options.Tolerance);

                if (options.Delimiter == 0)
                {
                    _log.WriteLine(LogLevel.Debug, "Delimiter is not explicitely set, so I am using \";\"");
                    options.Delimiter = ';';
                }
                else
                    _log.WriteLine(LogLevel.Debug, "Delimiter to be used to parse csv has been explicitely set and is \"{0}\"", options.Delimiter);

                if (null == options.Logfile)
                    _log.WriteLine(LogLevel.Debug, "Logfile is empty");
                else
                {
                    if (!options.OverrideOutput && File.Exists(options.Logfile))
                    {
                        options.Logfile = Path.Combine(Path.GetDirectoryName(options.Logfile), string.Format(CultureInfo.CurrentCulture, "{0:yyyy-MM-ddTHH-mm-ss}_log.txt", DateTime.Now));
                        _log.WriteLine(LogLevel.Warning, "Logfile already exists and --override has been set to false. Changed log file name to \"{0}\"", options.Logfile);
                    }
                    else
                        _log.WriteLine(LogLevel.Debug, "Logfile is {0}", options.Logfile);

                    _log.SetLogFile(new FileInfo(options.Logfile));
                }

                if (options.Items.Count == 0)
                {
                    _log.Error("No files or directories to be compared have been given.");
                    Console.WriteLine(options.GetUsage());
                    Environment.Exit(2);
                }

                MetaReport meta = new MetaReport();

                if (String.IsNullOrEmpty(options.ReportDir))//report to directory of compare file if no report directory has been set
                {
                    FileAttributes attr = File.GetAttributes(options.Items[0]);

                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        options.ReportDir = Path.GetFullPath(options.Items[0]);
                    else
                        options.ReportDir = Path.GetDirectoryName(options.Items[0]);
                    _log.WriteLine(LogLevel.Warning, "No report directory has been set, using \"{0}\"", options.ReportDir);
                }
                else
                {
                    meta.ReportDirSet = true;
                    options.ReportDir = Path.GetFullPath(options.ReportDir);//normalize report dir (i.e. make absolut if relative)
                }

                meta.FileName = new FileInfo(Path.Combine(options.ReportDir, string.Format(CultureInfo.CurrentCulture, "{0:yyyy-MM-dd}-index.html", DateTime.Now)));

                switch (options.Mode)
                {
                    case OperationMode.CsvFileCompare://compare the file given as option 1 with the file given as option 2
                        if (options.Items.Count != 2)
                        {
                            _log.Error("Not enough arguments have been given. Please specify base and compare file.");
                            Console.WriteLine(options.GetUsage());
                            Environment.Exit(2);
                        }
                        Report rep = CheckFiles(options);
                        if (null != rep)
                        {
                            meta.Reports.Add(rep);//Return "1" on invalid testresults
                            meta.WriteReport(_log, options);
                        }
                        break;
                    case OperationMode.CsvTreeCompare://compare files in the dirctory option 1 with the files in directory option 2
                        if (options.Items.Count != 2)
                        {
                            _log.Error("Not enough arguments have been given. Please specify base and compare directory.");
                            Console.WriteLine(options.GetUsage());
                            Environment.Exit(2);
                        }
                        CheckTrees(meta, options);
                        meta.WriteReport(_log, options);
                        break;
                    case OperationMode.FmuChecker://run FMU checker on all fmus in directory given via option 1 and compare the result to CSVs in the source directory
                        if (options.Items.Count != 1)
                        {
                            if (options.Items.Count == 0)
                                _log.Error("Not enough arguments have been given. Please specify a compare directory with FMUs.");
                            else
                                _log.Error("Too many arguments have been given. Please specify a compare directory with FMUs only.");

                            Console.WriteLine(options.GetUsage());
                            Environment.Exit(2);
                        }
                        if (string.IsNullOrEmpty(options.CheckerPath))
                        {
                            _log.Error("No path to FMU Checker binary given.");
                            Console.WriteLine(options.GetUsage());
                        }
                        else
                        {
                            CheckFMUTree(ref meta, options);
                            meta.WriteReport(_log, options);
                        }
                        break;
                    case OperationMode.PlotOnly:
                        foreach (string item in options.Items)
                        {
                            CsvFile file = new CsvFile(item, options, _log);
                            meta.Reports.Add(file.PlotCsvFile(null, _log));                            
                        }

                        meta.WriteReport(_log, options);
                        break;
                    default://Invalid mode
                        Console.WriteLine(options.GetUsage());
                        break;
                }
            }
            else
                Console.WriteLine(options.GetUsage());
#if DEBUG
            //Keep window open in debug mode
            _log.WriteLine(LogLevel.Debug, "Done.");
            Console.ReadLine();
#endif
        }

        private static void CheckFMUTree(ref MetaReport meta, Options options)
        {
            DirectoryInfo dirCompare, dirBase;

            string sTempDir = string.Format(CultureInfo.CurrentCulture, "{0}CSV-Compare-{1:yyyyMMdd}", Path.GetTempPath(), DateTime.Now);
            dirBase = new DirectoryInfo(options.Items[0]);
            if (!dirBase.Exists)
            {
                _log.Error("The directory \"{0}\" containing fmu files does not exist.", dirBase.FullName);
                Environment.Exit(2);
            }            

            string outFile = string.Empty;

            dirCompare = new DirectoryInfo(sTempDir);
            if (!dirCompare.Exists)
                Directory.CreateDirectory(dirCompare.ToString());

            if (!String.IsNullOrEmpty(options.ReportDir))
                meta = new MetaReport(options.ReportDir);
            else
                meta = new MetaReport();

            foreach (FileInfo file in dirBase.GetFiles("*.fmu", SearchOption.AllDirectories))
            {
                if (!RunFMUChecker(options, dirCompare, ref outFile, file))//Skip to next FMU on error
                    continue;

                string sBase = Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + ".csv");
                _log.WriteLine(LogLevel.Debug, "Searching for csv {0} ", sBase);
                if (!File.Exists(sBase))//if $FILENAME$.csv could not be found search for protocol.csv
                {
                    sBase = Path.Combine(file.Directory.FullName, "protocol.csv");
                    _log.WriteLine(LogLevel.Information, "Not found trying \"{0}\"", sBase);
                    if (!File.Exists(sBase))
                    {
                        _log.Error("Found nothing to compare with, skipping {0}", file.Name);
                        continue;
                    }
                }

                _log.WriteLine(LogLevel.Information, "Trying to compare with \"{0}\"", sBase);
                try
                {
                    CsvFile csvCompare = new CsvFile(outFile, options, _log);

                    try
                    {
                        meta.Reports.Add(csvCompare.CompareFiles(_log, new CsvFile(sBase, options, _log), Path.Combine(options.ReportDir, file.Name + ".html")));
                    }
                    catch (ArgumentNullException ex) { _log.Error(ex.Message); }
                }
                catch (ArgumentException ex)
                {
                    _log.Error(ex.Message);
                    continue;
                }
            }
        }

        private static void CheckTrees(MetaReport meta, Options options)
        {
            DirectoryInfo dirCompare = new DirectoryInfo(options.Items[0]);
            DirectoryInfo dirBase = new DirectoryInfo(options.Items[1]);

            foreach (FileInfo file in dirCompare.GetFiles("*.csv", SearchOption.AllDirectories))
            {
                _log.WriteLine(LogLevel.Debug, "Searching for file {0} in {1}", file.Name, dirBase.FullName);
                
                //Try different locations for the base file
                string sBaseFile = string.Empty;
                if (dirBase.GetFiles(file.Name).Length == 1)//1. Base file is in the, via commandline, given base directory
                {
                    sBaseFile = Path.Combine(dirBase.FullName, file.Name);
                    _log.WriteLine(LogLevel.Debug, "Found base file in given base directory \"{0}\", comparing ...", dirBase.Name);
                }
                else
                {
                    try
                    {
                        sBaseFile = file.FullName.Replace(options.Items[0], options.Items[1]);//2. Base file is in the same directory structure as the compare file
                        if (!File.Exists(sBaseFile))
                        {
                            _log.WriteLine(LogLevel.Warning, "{0} not found in {1} or {2}, skipping validation.", file.Name, dirBase.Name, sBaseFile);
                            continue;
                        }
                        else
                            _log.WriteLine(LogLevel.Debug, "Found base file in given base directory \"{0}\", comparing ...", dirBase);
                    }
                    catch (Exception ex) 
                    {
                        _log.Error(ex.Message);
                        continue;
                    }
                }               

                Report r = CheckFiles(options, file.FullName, sBaseFile);
                if (null != r)
                    meta.Reports.Add(r);
            }
        }

        private static Report CheckFiles(Options options, string Compare = null, string Base = null)
        {
            //Check Arguments
            if (string.IsNullOrEmpty(Compare))
                if (options.Items.Count == 2)
                    Compare = options.Items[0];
                else
                    throw new ArgumentException("You have to set compare and base csv files!");
            if (string.IsNullOrEmpty(Base))
                if (options.Items.Count == 2)
                    Base = options.Items[1];
                else
                    throw new ArgumentException("You have to set compare and base csv files!");

            CsvFile csvCompare, csvBase;

            try
            {
                csvCompare = new CsvFile(Compare, options, _log);
                csvCompare.ShowRelativeErrors = !options.AbsoluteError;
            }
            catch (ArgumentException ex)          
            {
                _log.Error("Nothing has been parsed; maybe wrong csv format?");
                _log.Error("Exception said: {0}", ex.Message);
                Environment.ExitCode=2;
                return null;
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, "Compare file \"{0}\" does not exist, exiting.", Compare));
            }

            try
            {
                csvBase = new CsvFile(Base, options, _log);
            }
            catch (ArgumentException argEx)
            {
                _log.Error(argEx.Message);
                return null;
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, "Base file \"{0}\" does not exist, exiting.", Base));
            }

#if DEBUG   //Save csv files during DEBUG session
            if (!string.IsNullOrEmpty(options.ReportDir))
            {
                csvBase.Save(options.ReportDir, options);
                csvCompare.Save(options.ReportDir, options);
            }
            else
            {
                csvBase.Save(options);
                csvCompare.Save(options);
            }
#endif
            _log.WriteLine(LogLevel.Debug, "Exiting with exit code \"{0}\".", Environment.ExitCode);
            return csvCompare.CompareFiles(_log, csvBase);
        }

        private static bool RunFMUChecker(Options options, DirectoryInfo dirCompare, ref string outFile, FileInfo file)
        {
            bool bRet = true;
            string sArgs;

            using (Process p = new Process())
            {
                outFile = string.Format(CultureInfo.CurrentCulture, "{0}/{1}.csv", dirCompare.FullName, Path.GetFileNameWithoutExtension(file.FullName));
                p.StartInfo = new ProcessStartInfo(options.CheckerPath);

                if (!string.IsNullOrEmpty(options.CheckerArgs))
                    sArgs = options.CheckerArgs;
                else
                    sArgs = "-l 1 -h 1e-2 -s 1.5";

                p.StartInfo.Arguments = string.Format(CultureInfo.CurrentCulture, "{0} -o \"{1}\" \"{2}\"", sArgs, outFile, file.FullName);
                p.StartInfo.UseShellExecute = false;

                _log.WriteLine(LogLevel.Debug, "Run \"{0} {1}\"", p.StartInfo.FileName, p.StartInfo.Arguments);

                if (p.Start())
                    p.WaitForExit();

                if (p.ExitCode != 0)
                {
                    _log.Error("Error during FMU Checker run");
                    bRet = false;
                }
            }
            return bRet;
        }
    }
}