// ReadOptions.cs
// author: Susanne Walther
// date: 18.12.2014

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CurveCompare.DataImport
{
    public class ReadOptions
    {
        private char delimiter, separator;

        /// <summary>
        /// Delimiter, that separates columns. Option for file reading.
        /// </summary>
        public char Delimiter
        {
            get { return delimiter; }
            set { delimiter = value; }
        }
        /// <summary>
        /// Decimal separator. Option for file reading.
        /// </summary>
        public char Separator
        {
            get { return separator; }
            set { separator = value; }
        }
        public ReadOptions()
        {
            delimiter = ';';
            separator = '.';
        }
        public ReadOptions(char delimiter, char separator)
        {
            this.delimiter = delimiter;
            this.separator = separator;
        }
    }
}
