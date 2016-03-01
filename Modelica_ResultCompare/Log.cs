using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvCompare
{
    public sealed class Log : IDisposable
    {
        private LogLevel _level;
        private LogLevel _verbosity;
        private string _lastLog;
        private DataTable _log;
        private bool _logToConsole;
        private bool _logToFile;
        private int _iErrorCount;
        private FileInfo _logFileInfo;

        public LogLevel DefaultLevel
        {
            get { return _level; }
            set { _level = value; }
        }
        public LogLevel Verbosity
        {
            get { return _verbosity; }
            set { _verbosity = value; }
        }
        public string LogFile
        {
            get { return _logFileInfo.FullName; }
        }
        public string LastLog
        {
            get { return _lastLog; }
        }
        public DataTable LogData
        {
            get { return _log; }
        }
        public bool LogToConsole
        {
            get { return _logToConsole; }
            set { _logToConsole = value; }
        }
        public int Errors
        {
            get
            {
                return _iErrorCount;
            }
        }

        public Log()
        {
            _level = LogLevel.Information;
            _verbosity = LogLevel.Warning;
            _lastLog = string.Empty;
            _logToConsole = true;

            InstantiateLogTable();
        }
        
        public Log(string fileName) : this(fileName, false) { }
        public Log(string fileName, bool logToConsole): this()
        {
            _logToConsole = logToConsole;

            _logFileInfo = new FileInfo(fileName);
            WriteLine(LogLevel.Logger, string.Empty);
        }

        private void InstantiateLogTable()
        {
            _log = new DataTable();
            _log.Locale = CultureInfo.InvariantCulture;
            _log.Columns.Add("TIMESTAMP", typeof(DateTime));
            _log.Columns.Add("SCOPE", typeof(LogLevel));
            _log.Columns.Add("MESSAGE", typeof(string));
        }

        public void WriteLine(LogLevel level, string format, params object[] message)
        {
            WriteLine(level, string.Format(CultureInfo.CurrentCulture, format, message));
        }
        public void WriteLine(string message)
        {
            try { WriteLine(LogLevel.Information, message); }
            catch { }
        }
        public void WriteLine(string format, params object[] message)
        {
            WriteLine(LogLevel.Information, format, message);
        }
        public void WriteLine(LogLevel level, string message)
        {
            if (level < _verbosity)
                return;

            DateTime timeStamp = DateTime.Now;
            ConsoleColor col = Console.ForegroundColor;

            if (level == LogLevel.Error || level == LogLevel.Exception)
            {
                if (LogToConsole)
                    Console.ForegroundColor = ConsoleColor.Red;

                _iErrorCount++;
            }

            if (level == LogLevel.Warning)
                if (LogToConsole)
                    Console.ForegroundColor = ConsoleColor.Yellow;

            if (level == LogLevel.Debug)
                if (LogToConsole)
                    Console.ForegroundColor = ConsoleColor.DarkGray;

            if (level == LogLevel.Done && LogToConsole)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("---------------");
            }

            if (null == _logFileInfo)
                _logFileInfo = new FileInfo(Path.GetTempFileName());

            _lastLog = string.Format(CultureInfo.CurrentCulture, "{0} [ {1,11} ] {2}",
                            timeStamp.ToString("yyyy-MM-ddZHH:mm:ss", CultureInfo.CurrentCulture),
                            level.ToString(), message);

            if (_logToFile)
            {
                using (TextWriter logFile = new StreamWriter(_logFileInfo.FullName, true, Encoding.UTF8))
                {

                    if (level == LogLevel.Logger)
                    {
                        logFile.WriteLine("* * *");
                        _lastLog = string.Format(CultureInfo.CurrentCulture, "New logging session started at {0} by {1} ...",
                            timeStamp.ToString("yyyy-MM-ddZHH:mm:ss", CultureInfo.CurrentCulture),
                            Environment.UserName);
                        logFile.WriteLine(_lastLog);
                    }
                    else
                    {
                        logFile.WriteLine(_lastLog);

                        try
                        {
                            logFile.Flush();
                        }
                        catch { }
                    }
                }
            }

            if (_logToConsole)
            {
                if (level == LogLevel.Error || level == LogLevel.Exception)
                    Console.Error.WriteLine(_lastLog);
                else
                    Console.WriteLine(_lastLog);

                Console.ForegroundColor = col;
            }

            if (message.Length > 0)
                _log.Rows.Add(timeStamp, level, message);
        }
        public void Error(string message)
        {
            WriteLine(LogLevel.Error, message);
        }
        public void Error(string format, params object[] message)
        {
            WriteLine(LogLevel.Error, format, message);
        }
        public void Done(string format)
        {
            WriteLine(LogLevel.Done, format);
        }
        public void Done(string format, params object[] message)
        {
            WriteLine(LogLevel.Done, format, message);
        }

        public void MoveLog(string targetDirectory)
        {
            DirectoryInfo oldPath = _logFileInfo.Directory;
            DirectoryInfo newPath;

            if (!Directory.Exists(targetDirectory))
                newPath = Directory.CreateDirectory(targetDirectory);
            else
                newPath = new DirectoryInfo(targetDirectory);

            File.Move(_logFileInfo.FullName, Path.Combine(newPath.FullName, _logFileInfo.Name));

            _logFileInfo = new FileInfo(Path.Combine(newPath.FullName, _logFileInfo.Name));
            WriteLine(LogLevel.Information, "Moved logfile from {0} to {1}.", oldPath.FullName, newPath.FullName);
        }
        public void SetLogFile(FileInfo logfile)
        {
            _logFileInfo = logfile;
            _logToFile = true;

            using (TextWriter logFile = new StreamWriter(_logFileInfo.FullName, true, Encoding.UTF8))
            {
                foreach (DataRow row in _log.Rows)
                    logFile.WriteLine(string.Format(CultureInfo.CurrentCulture, "{0} [ {1,11} ] {2}",
                            DateTime.Parse(row[0].ToString(), CultureInfo.CurrentCulture).ToString("yyyy-MM-ddZHH:mm:ss", CultureInfo.CurrentCulture),
                            Enum.Parse(typeof(LogLevel), row[1].ToString()).ToString(), row[2].ToString()));

                try
                {
                    logFile.Flush();
                }
                catch { }
            }
        }

        #region IDisposable Member

        public void Dispose()
        {
            this._log.Dispose();
            GC.Collect();
        }

        #endregion
    }
    public enum LogLevel
    {
        None = 0,
        Logger,
        Debug,
        Information,
        Warning,
        Exception,
        Error,
        Done
    }
}
