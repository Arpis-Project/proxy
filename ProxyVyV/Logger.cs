namespace ProxyVyV
{
    using System;
    using System.IO;
    using System.Text;

    public sealed class Logger : IDisposable
    {
        private string fFileName = "DefaultLogFile";
        private string fFileExt = ".LOG";
        private TLogLevel fLogLevel = TLogLevel.logNone;
        private bool finalizationSupressed = false;
        private StreamWriter SW = null;
        private bool DirectoryChecked = false;
        private static readonly Logger instance = new Logger();
        private bool disposed = false;

        private Logger()
        {
        }

        private void Check4NullStreamWriter(string PathAndName)
        {
            bool flag = false;
            if (ReferenceEquals(this.SW, null))
            {
                flag = true;
            }
            else if (ReferenceEquals(this.SW.BaseStream, null))
            {
                flag = true;
            }
            if (flag)
            {
                this.CreateStreamWriter(PathAndName);
                if (this.finalizationSupressed)
                {
                    GC.ReRegisterForFinalize(this);
                }
            }
        }

        private void CreateStreamWriter(string PathAndName)
        {
            this.SW = new StreamWriter(PathAndName, true, Encoding.Default);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
            this.finalizationSupressed = true;
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing && ((this.SW != null) && (this.SW.BaseStream != null)))
                {
                    this.SW.Dispose();
                }
                this.disposed = true;
            }
        }

        ~Logger()
        {
            this.Dispose(false);
        }

        public string FormatLogTypeString(TLogType ALogType)
        {
            string str = string.Empty;
            try
            {
                switch (ALogType)
                {
                    case TLogType.ltStart:
                        str = "**  Start Logging | ";
                        break;

                    case TLogType.ltInterface:
                        str = "  ->  Interface   | ";
                        break;

                    case TLogType.ltMethod:
                        str = "  <>  Method      | ";
                        break;

                    case TLogType.ltCheckpoint:
                        str = "  []  Checkpoint  | ";
                        break;

                    case TLogType.ltData:
                        str = "      Data        |   ";
                        break;

                    case TLogType.ltList:
                        str = "      List Data   | ";
                        break;

                    case TLogType.ltInformational:
                        str = "      Info.       | ";
                        break;

                    case TLogType.ltError:
                        str = "  ER  ERROR       | ";
                        break;

                    case TLogType.ltSystem:
                        str = "      System      | ";
                        break;

                    case TLogType.ltInternal:
                        str = "      Internal    | ";
                        break;

                    case TLogType.ltStop:
                        str = "**  Stop Logging  | ";
                        break;

                    case TLogType.ltClose:
                        str = "**  File Closed   | ";
                        break;

                    default:
                        break;
                }
                return str;
            }
            catch
            {
                return "***LOG TYPE ERROR***";
            }
        }

        private string GetDatePostfix() =>
            ("_" + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0'));

        public void Log(string logStr, TLogType ALogType, TLogLevel ALogLevel)
        {
            string pathAndName = "";
            if ((ALogLevel != TLogLevel.logNone) || (ALogType == TLogType.ltError))
            {
                try
                {
                    pathAndName = this.FilePath + this.FileName;
                }
                catch (Exception)
                {
                }
                this.LogSW(logStr, ALogType, ALogLevel, pathAndName);
            }
        }

        private void LogSW(string logStr, TLogType ALogType, TLogLevel ALogLevel, string PathAndName)
        {
            string str = string.Empty;
            try
            {
                this.Check4NullStreamWriter(PathAndName);
                str = this.TimeStamp + this.FormatLogTypeString(ALogType) + logStr;
                if (((ALogType == TLogType.ltStart) || (ALogType == TLogType.ltError)) || (ALogType > TLogType.ltInternal))
                {
                    this.SW.WriteLine(str);
                }
                else if (ALogLevel <= this.LogLevel)
                {
                    this.SW.WriteLine(str);
                }
                this.SW.Flush();
            }
            catch (Exception)
            {
            }
        }

        public TLogLevel StrToLogLevelDef(string Value, TLogLevel ALevel)
        {
            try
            {
                return (TLogLevel)Enum.Parse(typeof(TLogLevel), Value);
            }
            catch
            {
                return ALevel;
            }
        }

        public static Logger Instance =>
            instance;

        public string FileName
        {
            get =>
                (this.fFileName + this.GetDatePostfix() + this.fFileExt);
            set =>
                this.fFileName = value;
        }

        public string FilePath
        {
            get
            {
                string str2;
                try
                {
                    string path = @"\Logs\";
                    if (!this.DirectoryChecked)
                    {
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        this.DirectoryChecked = true;
                    }
                    str2 = path;
                }
                catch
                {
                    throw;
                }
                return str2;
            }
        }

        public TLogLevel LogLevel
        {
            get =>
                this.fLogLevel;
            set =>
                this.fLogLevel = value;
        }

        public string TimeStamp
        {
            get
            {
                DateTime now = DateTime.Now;
                string[] textArray1 = new string[13];
                textArray1[0] = now.Year.ToString();
                textArray1[1] = ".";
                textArray1[2] = now.Month.ToString().PadLeft(2, '0');
                textArray1[3] = ".";
                textArray1[4] = now.Day.ToString().PadLeft(2, '0');
                textArray1[5] = ".";
                textArray1[6] = now.Hour.ToString().PadLeft(2, '0');
                textArray1[7] = ".";
                textArray1[8] = now.Minute.ToString().PadLeft(2, '0');
                textArray1[9] = ".";
                textArray1[10] = now.Second.ToString().PadLeft(2, '0');
                textArray1[11] = ".";
                textArray1[12] = now.Millisecond.ToString().PadLeft(3, '0');
                return string.Concat(textArray1);
            }
        }
    }
}

