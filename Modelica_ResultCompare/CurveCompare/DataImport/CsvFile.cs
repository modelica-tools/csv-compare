// CsvFile.cs
// author: Sven Rütz, Susanne Walther
// date: 18.12.2014

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

namespace CurveCompare.DataImport
{
    /// <summary>
    /// Parses CSV files and holds results in a dictionary.
    /// </summary>
    public class CsvFile
    {
        private string _fileName = string.Empty;
        private List<double> _xAxis = new List<double>();
        private Dictionary<string, List<double>> _values = new Dictionary<string, List<double>>();

        /// <summary>
        /// Holds values for x axis.
        /// </summary>
        public List<double> XAxis { get { return _xAxis; } }        
        /// <summary>
        /// Holds values for the results in a dictionary. The key is the result identifier.
        /// </summary>
        public Dictionary<string, List<double>> Results { get { return _values; } }

        /// <summary>
        /// The constructor reads the csv file to this object.
        /// </summary>
        /// <param name="fileName">Full path name of the csv file.</param>
        /// <param name="options">Options for reading csv files.</param>
        public CsvFile(string fileName, ReadOptions options)
            : this(fileName, options, null)
        { }
       
        /// <summary>
        /// The constructor reads the csv file to this object
        /// </summary>
        /// <param name="fileName">Full path of the csv file</param>
        /// <param name="delimiter">Delimiter, that separates columns.</param>
        /// <param name="separator">Decimal separator.</param>
        /// <param name="log">Log for saving to log file.</param>
        public CsvFile(string fileName, ReadOptions options, Log log)
        {
            bool writeLogFile = (log != null);
            
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
                            string sTemp = sCol.Trim(' ', '"', '\t', '\'');
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

                    CheckHeaderForNumbers(log, writeLogFile, map);

                    //read the rest of the csv file
                    while ((sLine = reader.ReadLine()) != null)
                    {
                        //Skip comments
                        if (sLine.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                            continue;

                        //values = reg.Split(sLine); //splitting using regular expressions is slow
                        IEnumerable<string> dataValues;
                        if (options.Delimiter.Equals(options.Separator))
                            dataValues = Tokenize(sLine, options.Delimiter); //use custom tokenizer for improved performance
                        else
                            dataValues = sLine.Split(options.Delimiter); //use ordinary Split function for simple cases
                        
                        int iCol = 0;

                        NumberFormatInfo provider = new NumberFormatInfo();
                        provider.NumberDecimalSeparator = options.Separator.ToString();

                        //read values to the dictionary
                        foreach (string sCol in dataValues)
                        {
                            double dValue;
                            if (!Double.TryParse(sCol.Trim('"'), NumberStyles.Any, provider, out dValue))
                            {
                                if (!string.IsNullOrEmpty(sCol) && writeLogFile)
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

        private static void CheckHeaderForNumbers(Log log, bool writeLogFile, List<string> map)
        {
            //Check map for numbers to throw a warning if no header has been set
            foreach (string sCol in map)
            {
                double dTemp;
                if (double.TryParse(sCol, out dTemp) && writeLogFile)
                    log.WriteLine(LogLevel.Warning, "Column \"{0}\" seems to be a number and should be a column title. Maybe you forgot to add a header line?", sCol);
            }
        }
        
        private List<string> Tokenize(string str, char delimiter)
        {
            List<string> tokens = new List<string>();

            int pos = 0;
            int end = str.Length;
            bool withinQuotes = false;

            int lpos=pos;
            int length=0;

            while (pos < end) {
                char c = str[pos];
                
                if (c == '"')
                    withinQuotes = !withinQuotes;

                if (c == delimiter && !withinQuotes)
                {
                    string token = str.Substring(lpos, length);
                    tokens.Add(token);
                    lpos = pos+1;
                    length = 0;
                }
                else {
                    length++;
                }

                pos++;
            }

            //special treatment for lines which are not terminated by the delimiter
            if (length > 0)
            {
                string token = str.Substring(lpos, length);
                tokens.Add(token);
            }

            return tokens;
        }

        public override string ToString()
        {
            return _fileName;
        }
    }
}