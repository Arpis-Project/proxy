namespace ProxyVyV
{
    using System;
    using System.IO;
    using System.Text;

    public class Log
    {
        public void write(string strTextToWrite)
        {
            object[] objArray1 = new object[] { "Proxy", DateTime.Now.Year, DateTime.Now.Month.ToString().PadRight(2, '0'), DateTime.Now.Day.ToString().PadRight(2, '0'), ".txt" };
            string strFileName = string.Concat(objArray1);
            write_p(strTextToWrite, strFileName);
        }

        private static void write_p(string strTextToWrite, string strFileName)
        {
            try
            {
                try
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    if (!Directory.Exists(currentDirectory + @"\LogProxy"))
                    {
                        Directory.CreateDirectory(currentDirectory + @"\LogProxy");
                    }
                    StreamWriter writer = new StreamWriter(currentDirectory + @"\LogProxy\" + strFileName, true, Encoding.ASCII);
                    writer.WriteLine(DateTime.Now.ToString() + " : " + strTextToWrite);
                    writer.Close();
                }
                catch (Exception exception)
                {
                    StreamWriter writer2 = new StreamWriter(@"C:\TEMP\" + strFileName, true, Encoding.ASCII);
                    writer2.WriteLine(DateTime.Now.ToString() + " : Error escribiendo en el log -- " + exception.Message);
                    writer2.Close();
                }
            }
            finally
            {
            }
        }

        public void writeFEBOS(string strTextToWrite)
        {
            object[] objArray1 = new object[5];
            objArray1[0] = "FEBOSUAProxy_";
            objArray1[1] = DateTime.Now.Year;
            objArray1[2] = DateTime.Now.Month.ToString().PadRight(2, '0');
            DateTime now = DateTime.Now;
            objArray1[3] = now.Day.ToString().PadRight(2, '0');
            objArray1[4] = ".txt";
            string str = string.Concat(objArray1);
            try
            {
                try
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    if (!Directory.Exists(currentDirectory + @"\LogFEBOS"))
                    {
                        Directory.CreateDirectory(currentDirectory + @"\LogFEBOS");
                    }
                    StreamWriter writer = new StreamWriter(currentDirectory + @"\LogFEBOS\" + str, true, Encoding.ASCII);
                    writer.WriteLine(DateTime.Now.ToString() + " : " + strTextToWrite);
                    writer.Close();
                }
                catch (Exception exception)
                {
                    StreamWriter writer2 = new StreamWriter(@"C:\TEMP\" + str, true, Encoding.ASCII);
                    writer2.WriteLine(DateTime.Now.ToString() + " : Error escribiendo en el log -- " + exception.Message);
                    writer2.Close();
                }
            }
            finally
            {
            }
        }

        public void writeTB(string strTextToWrite)
        {
            object[] objArray1 = new object[] { "Proxy_TB", DateTime.Now.Year, DateTime.Now.Month.ToString().PadRight(2, '0'), DateTime.Now.Day.ToString().PadRight(2, '0'), ".txt" };
            string strFileName = string.Concat(objArray1);
            write_p(strTextToWrite, strFileName);
        }

        public void writeDTEVyV(string strTextToWrite)
        {
            object[] objArray1 = new object[5];
            objArray1[0] = "DTEVyVProxy_";
            objArray1[1] = DateTime.Now.Year;
            objArray1[2] = DateTime.Now.Month.ToString().PadRight(2, '0');
            DateTime now = DateTime.Now;
            objArray1[3] = now.Day.ToString().PadRight(2, '0');
            objArray1[4] = ".txt";
            string str = string.Concat(objArray1);
            try
            {
                try
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    if (!Directory.Exists(currentDirectory + @"\LogDTEVyV"))
                    {
                        Directory.CreateDirectory(currentDirectory + @"\LogDTEVyV");
                    }
                    StreamWriter writer = new StreamWriter(currentDirectory + @"\LogDTEVyV\" + str, true, Encoding.ASCII);
                    writer.WriteLine(DateTime.Now.ToString() + " : " + strTextToWrite);
                    writer.Close();
                }
                catch (Exception exception)
                {
                    StreamWriter writer2 = new StreamWriter(@"C:\TEMP\" + str, true, Encoding.ASCII);
                    writer2.WriteLine(DateTime.Now.ToString() + " : Error escribiendo en el log -- " + exception.Message);
                    writer2.Close();
                }
            }
            finally
            {
            }
        }

        public void writeDTEDBNET(string strTextToWrite)
        {
            object[] objArray1 = new object[5];
            objArray1[0] = "DBNETProxy_";
            objArray1[1] = DateTime.Now.Year;
            objArray1[2] = DateTime.Now.Month.ToString().PadRight(2, '0');
            DateTime now = DateTime.Now;
            objArray1[3] = now.Day.ToString().PadRight(2, '0');
            objArray1[4] = ".txt";
            string str = string.Concat(objArray1);
            try
            {
                try
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    if (!Directory.Exists(currentDirectory + @"\LogDTEDBNET"))
                    {
                        Directory.CreateDirectory(currentDirectory + @"\LogDTEDBNET");
                    }
                    StreamWriter writer = new StreamWriter(currentDirectory + @"\LogDTEDBNET\" + str, true, Encoding.ASCII);
                    writer.WriteLine(DateTime.Now.ToString() + " : " + strTextToWrite);
                    writer.Close();
                }
                catch (Exception exception)
                {
                    StreamWriter writer2 = new StreamWriter(@"C:\TEMP\" + str, true, Encoding.ASCII);
                    writer2.WriteLine(DateTime.Now.ToString() + " : Error escribiendo en el log -- " + exception.Message);
                    writer2.Close();
                }
            }
            finally
            {
            }
        }

        public void writeDTESignature(string strTextToWrite)
        {
            object[] objArray1 = new object[5];
            objArray1[0] = "SignatureProxy_";
            objArray1[1] = DateTime.Now.Year;
            objArray1[2] = DateTime.Now.Month.ToString().PadRight(2, '0');
            DateTime now = DateTime.Now;
            objArray1[3] = now.Day.ToString().PadRight(2, '0');
            objArray1[4] = ".txt";
            string str = string.Concat(objArray1);
            try
            {
                try
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    if (!Directory.Exists(currentDirectory + @"\LogDTESignature"))
                    {
                        Directory.CreateDirectory(currentDirectory + @"\LogDTESignature");
                    }
                    StreamWriter writer = new StreamWriter(currentDirectory + @"\LogDTESignature\" + str, true, Encoding.ASCII);
                    writer.WriteLine(DateTime.Now.ToString() + " : " + strTextToWrite);
                    writer.Close();
                }
                catch (Exception exception)
                {
                    StreamWriter writer2 = new StreamWriter(@"C:\TEMP\" + str, true, Encoding.ASCII);
                    writer2.WriteLine(DateTime.Now.ToString() + " : Error escribiendo en el log -- " + exception.Message);
                    writer2.Close();
                }
            }
            finally
            {
            }
        }

        public void writeDTEGetOne(string strTextToWrite)
        {
            object[] objArray1 = new object[5];
            objArray1[0] = "DTEGetOneProxy_";
            objArray1[1] = DateTime.Now.Year;
            objArray1[2] = DateTime.Now.Month.ToString().PadRight(2, '0');
            DateTime now = DateTime.Now;
            objArray1[3] = now.Day.ToString().PadRight(2, '0');
            objArray1[4] = ".txt";
            string str = string.Concat(objArray1);
            try
            {
                try
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    if (!Directory.Exists(currentDirectory + @"\LogDTEGetOne"))
                    {
                        Directory.CreateDirectory(currentDirectory + @"\LogDTEGetOne");
                    }
                    StreamWriter writer = new StreamWriter(currentDirectory + @"\LogDTEGetOne\" + str, true, Encoding.ASCII);
                    writer.WriteLine(DateTime.Now.ToString() + " : " + strTextToWrite);
                    writer.Close();
                }
                catch (Exception exception)
                {
                    StreamWriter writer2 = new StreamWriter(@"C:\TEMP\" + str, true, Encoding.ASCII);
                    writer2.WriteLine(DateTime.Now.ToString() + " : Error escribiendo en el log -- " + exception.Message);
                    writer2.Close();
                }
            }
            finally
            {
            }
        }

        public void writeDTEFacele(string strTextToWrite)
        {
            object[] objArray1 = new object[5];
            objArray1[0] = "DTEVyVProxy_";
            objArray1[1] = DateTime.Now.Year;
            objArray1[2] = DateTime.Now.Month.ToString().PadRight(2, '0');
            DateTime now = DateTime.Now;
            objArray1[3] = now.Day.ToString().PadRight(2, '0');
            objArray1[4] = ".txt";
            string str = string.Concat(objArray1);
            try
            {
                try
                {
                    string currentDirectory = Directory.GetCurrentDirectory();
                    if (!Directory.Exists(currentDirectory + @"\LogDTEFacele"))
                    {
                        Directory.CreateDirectory(currentDirectory + @"\LogDTEFacele");
                    }
                    StreamWriter writer = new StreamWriter(currentDirectory + @"\LogDTEFacele\" + str, true, Encoding.ASCII);
                    writer.WriteLine(DateTime.Now.ToString() + " : " + strTextToWrite);
                    writer.Close();
                }
                catch (Exception exception)
                {
                    StreamWriter writer2 = new StreamWriter(@"C:\TEMP\" + str, true, Encoding.ASCII);
                    writer2.WriteLine(DateTime.Now.ToString() + " : Error escribiendo en el log -- " + exception.Message);
                    writer2.Close();
                }
            }
            finally
            {
            }
        }
    }
}

