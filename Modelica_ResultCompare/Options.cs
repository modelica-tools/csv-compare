using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace CsvCompare
{
    public enum OperationMode
    {
        NotSet,
        CsvFileCompare,
        CsvTreeCompare,
        FmuChecker,
        PlotOnly
    }

    public enum ExecutionMethod
    {
        Relative,
        Absolute
    }

    public enum ToleranceDirection
    {
        X,
        Y
    }

    public class Options
    {
        [Option('a', "args", Required = false, HelpText = "Arguments to run FMU checker with. [Default is \"-l 5 -h 1e-2 -s 1.5\"]")]
        public string CheckerArgs { get; set; }

        [Option('c', "checker", Required = false, HelpText = "Complete path of the FMU checker binary without arguments.")]
        public string CheckerPath { get; set; }

        [Option('l', "logfile", Required = false, HelpText = "Log the output to the given file.")]
        public string Logfile { get; set; }

        [Option('m', "mode", DefaultValue = OperationMode.CsvFileCompare, Required = false, HelpText = "Set the tools operation mode. Valid modes are: CsvFileCompare, CsvTreeCompare, FmuChecker, PlotOnly(experimental)")]
        public OperationMode Mode { get; set; }

        [Option("method", DefaultValue = ExecutionMethod.Relative, Required = false, HelpText = "Set the tools execution method. Valid methods are: Relative and Absolute.If relative method is opted range of tolerance is [0,1] and for absolute there is no limit.")]
        public ExecutionMethod Method { get; set; }

        [Option("direction", DefaultValue = ToleranceDirection.X, Required = false, HelpText = "Set the tolerance to any one of the axis.valid directions are: X and Y.")]
        public ToleranceDirection Direction { get; set; }

        [Option('o', "override", DefaultValue = false, HelpText = "Override output files if they already exist (Default behaviour is to put the output next to the found file with a time stamp in the file name).")]
        public bool OverrideOutput { get; set; }

        [Option('e', "abserror", DefaultValue = false, HelpText = "Shows, if set, only 0 and 1 in the error graph (peeks) instead of the difference between the error and the penetrated tube.")]
        public bool AbsoluteError { get; set; }

        [Option("comparisonflag", DefaultValue = false, HelpText = "Generates a text file that indicates if the test has been passed and contains test details.")]
        public bool ComparisonFlag { get; set; }

        [Option('r', "reportdir", HelpText = "Specifies the directory where the html reports(s) are to be saved.")]
        public string ReportDir { get; set; }

        [Option('n', "nometareport", DefaultValue = false, HelpText = "Set this to disable the generation of a meta report.")]
        public bool NoMetaReport { get; set; }

        [Option('b', "bitmap", DefaultValue = false, HelpText = "Set this to generate raster instead of vector plots (i.e. reports will shrink).")]
        public bool UseBitmapPlots { get; set; }

        [Option('i', "inline", DefaultValue = false, HelpText = "If set, javascript and style sheet files are inserted as inline text in every html output file")]
        public bool InlineScripts { get; set; }

        [Option('t', "tolerance", Required = false, DefaultValue = "0.002", HelpText = "Set the width of the tube at discontinuity in x-direction [Default is 0.002].")]
        public string Tolerance { get; set; }

        [Option('v', "verbosity", DefaultValue = 4, Required = false, HelpText = "Sets the verbosity of the output (1 most to 4[Default] less verbose).")]
        public int Verbosity { get; set; }

        [Option('d', "delimiter", Required = false, HelpText = "Sets the delimiter that is used to parse and write csv files. Default value is \";\".")]
        public char Delimiter { get; set; }

        [Option('s', "separator", DefaultValue = '.', Required = false, HelpText = "Sets the decimal separator that is used to parse and write csv files. Default value is \".\".")]
        public char Separator { get; set; }

        [ValueList(typeof(List<string>))]
        public IList<string> Items { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            Environment.ExitCode = 1;
            return HelpText.AutoBuild(this).ToString();
        }
    }
}
