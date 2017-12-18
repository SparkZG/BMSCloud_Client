using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Data;
using System.IO;
using System.IO.Ports;
using SocketLibrary;
using System.Xml;
using bq_Client.ViewModels;
using DevExpress.Xpf.Core;
using System.Windows;

namespace bq_Client
{
    public class BLLCommon
    {
        /// <summary>
        /// 等待窗体
        /// </summary>
        public static View.WaitWindow ww = null;

        public static object[] dataRange = new object[] { "全部", 1000, 2000, 5000, 8000, 10000, 15000, 20000, 30000, 50000 };

        public static string[] historyType = new string[] { "主机信息", "Pack信息" };

        public static byte[] GetArrData(DataRow dr)
        {
            string[] arrStrInfo = dr[0].ToString().Trim().Split(' ');
            byte[] arrData = new byte[arrStrInfo.Length];
            for (int i = 0; i < arrStrInfo.Length; i++)
            {
                arrData[i] = Convert.ToByte(arrStrInfo[i], 16);
            }
            return arrData;
        }
        public static DateTime BytesToDateTime(byte[] bytes, int offset)
        {
            if (bytes != null)
            {
                long ticks = BitConverter.ToInt64(bytes, offset);
                if (ticks < DateTime.MaxValue.Ticks && ticks > DateTime.MinValue.Ticks)
                {
                    DateTime dt = new DateTime(ticks);
                    return dt;
                }
            }
            return new DateTime();
        }

        public static byte[] DateTimeToBytes(DateTime dt)
        {
            return BitConverter.GetBytes(dt.Ticks);
        }

        public static void ShowWaitWindow()
        {
            if (ww == null)
            {
                ww = new View.WaitWindow();
                ww.ShowDialog();
            }
        }
        public static void CloseWaitWindow(bool IsReadFailed)
        {
            if (ww != null)
            {                
                ww.CloseSplashScreen();
                ww = null;
                if (IsReadFailed)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        DXMessageBox.Show("读取失败", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                }
            }
        }

        public static DataSet ExcelToDS(string Path)
        {
            if (Path.Length <= 0)
            {
                return null;
            }
            string strConn = "";
            string tableName = "";

            //需要安装下载新的驱动引擎http://download.microsoft.com/download/7/0/3/703ffbcb-dc0c-4e19-b0da-1463960fdcdb/AccessDatabaseEngine.exe
            strConn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + Path + ";" + "Extended Properties=Excel 12.0;";
            OleDbConnection conn = new OleDbConnection(strConn);
            try
            {
                conn.Open();
            }
            catch (Exception)
            {
                if (Path.Substring(Path.Length - 1, 1) == "s")
                    strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + Path + ";" + "Extended Properties=Excel 8.0;";
                else if (Path.Substring(Path.Length - 1, 1) == "x")
                {
                    throw new Exception("仅支持.xls版本导入");
                }
                conn.Open();
            }
            string strExcel = "";
            OleDbDataAdapter myCommand = null;

            DataSet ds = new DataSet(); ;
            DataTable schemaTable = conn.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
            for (int i = 0; i < schemaTable.Rows.Count; i++)
            {
                tableName = schemaTable.Rows[i][2].ToString().Trim();
                if (!tableName.Contains("FilterDatabase") && tableName.Substring(tableName.Length - 1, 1) != "_")
                {
                    ds.Tables.Add(tableName);
                    strExcel = string.Format("select * from [{0}]", tableName);
                    myCommand = new OleDbDataAdapter(strExcel, strConn);
                    myCommand.Fill(ds, tableName);
                }
            }
            conn.Close();
            return ds;
        }

        public static bool SaveTxt(string filePath, params string[] arrLog)
        {
            using (StreamWriter sw = new StreamWriter(filePath, false))
            {
                try
                {
                    for (int i = 0; i < arrLog.Length; i++)
                    {
                        sw.WriteLine(arrLog[i]);
                    }
                }
                catch (Exception)
                {
                    return false;
                }
                sw.Close();
                return true;
            }
        }

        public static void SendToServer(byte modeInt, ushort cust_id, byte group_id, byte pack_id, params byte[] sendContent)
        {
            List<byte> _listSend = new List<byte> { };
            _listSend.Add(FlagType.CS_AskReplay);
            _listSend.Add(modeInt);
            _listSend.Add(SendDirection.C_S);

            _listSend.AddRange(BitConverter.GetBytes(cust_id));
            _listSend.Add(group_id);
            _listSend.Add(pack_id);

            if (modeInt == 0x00 || modeInt == 0x02)
            {
                if (sendContent.Length > 4)
                {
                    //0x01代表历史数据请求
                    _listSend.Add(0x01);
                }
                else
                {
                    //0x00代表实时数据请求
                    _listSend.Add(0x00);
                }
            }
            else
            {
                _listSend.Add(0xff);
            }
            _listSend.Add(0xff);
            //字节数
            _listSend.AddRange(BitConverter.GetBytes(sendContent.Length));
            //最近多少条
            _listSend.AddRange(sendContent);
            Int32 sum = 0;
            for (int i = 1; i < _listSend.Count; i++)
            {
                sum += _listSend[i];
            }
            _listSend.AddRange(BitConverter.GetBytes(sum));
            _listSend.Add(0xAA);
            int lastLength = 1024 - _listSend.Count;
            for (int i = 0; i < lastLength; i++)
            {
                _listSend.Add(0);
            }
            ViewModel.socketClient.Send(_listSend.ToArray());
        }

    }

    public class CSVFileHelper
    {
        /// <summary>
        /// 将DataTable中数据写入到CSV文件中
        /// </summary>
        /// <param name="dt">提供保存数据的DataTable</param>
        /// <param name="fileName">CSV的文件路径</param>
        public static bool SaveCSV(DataTable dt, string fullPath)
        {
            FileInfo fi = new FileInfo(fullPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            using (FileStream fs = new FileStream(fullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                //StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
                using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    try
                    {
                        string data = "";
                        //写出列名称
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            data += dt.Columns[i].ColumnName.ToString();
                            if (i < dt.Columns.Count - 1)
                            {
                                data += ",";
                            }
                        }
                        sw.WriteLine(data);
                        //写出各行数据
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            data = "";
                            for (int j = 0; j < dt.Columns.Count; j++)
                            {
                                string str = dt.Rows[i][j].ToString();
                                str = str.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号
                                if (str.Contains(',') || str.Contains('"')
                                    || str.Contains('\r') || str.Contains('\n')) //含逗号 冒号 换行符的需要放到引号中
                                {
                                    str = string.Format("\"{0}\"", str);
                                }

                                data += str;
                                if (j < dt.Columns.Count - 1)
                                {
                                    data += ",";
                                }
                            }
                            sw.WriteLine(data);
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                    finally
                    {
                        sw.Close();
                        fs.Close();
                    }
                    return true;
                }
            }
        }

        /// <summary>
        /// 将CSV文件的数据读取到DataTable中
        /// </summary>
        /// <param name="fileName">CSV文件路径</param>
        /// <returns>返回读取了CSV数据的DataTable</returns>
        public static DataTable OpenCSV(string filePath)
        {
            //Encoding encoding = Common.GetType(filePath); //Encoding.ASCII;//
            DataTable dt = new DataTable();
            using (FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.Default))
                {
                    //StreamReader sr = new StreamReader(fs, encoding);
                    //string fileContent = sr.ReadToEnd();
                    //encoding = sr.CurrentEncoding;
                    //记录每次读取的一行记录
                    string strLine = "";
                    //记录每行记录中的各字段内容
                    string[] aryLine = null;
                    string[] tableHead = null;
                    //标示列数
                    int columnCount = 0;
                    //标示是否是读取的第一行
                    bool IsFirst = true;
                    //逐行读取CSV中的数据
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        //strLine = Common.ConvertStringUTF8(strLine, encoding);
                        //strLine = Common.ConvertStringUTF8(strLine);

                        if (IsFirst == true)
                        {
                            tableHead = strLine.Split(',');
                            IsFirst = false;
                            columnCount = tableHead.Length;
                            //创建列
                            for (int i = 0; i < columnCount; i++)
                            {
                                DataColumn dc = new DataColumn(tableHead[i]);
                                dt.Columns.Add(dc);
                            }
                        }
                        else
                        {
                            aryLine = strLine.Split(',');
                            DataRow dr = dt.NewRow();
                            for (int j = 0; j < columnCount; j++)
                            {
                                dr[j] = aryLine[j];
                            }
                            dt.Rows.Add(dr);
                        }
                    }
                    if (aryLine != null && aryLine.Length > 0)
                    {
                        dt.DefaultView.Sort = tableHead[0] + " " + "asc";
                    }

                    sr.Close();
                    fs.Close();
                }
            }
            return dt;
        }
    }

    public class DealXML
    {
        /// <summary>
        /// 将xml对象内容转换为dataset
        /// </summary>
        /// <param name="xmlData"></param>
        /// <returns></returns>
        private static DataSet ConvertXMLToDataSet(string xmlData)
        {
            using (StringReader stream = new StringReader(xmlData))
            {
                using (XmlTextReader reader = new XmlTextReader(stream))
                {
                    try
                    {
                        DataSet xmlDS = new DataSet();
                        xmlDS.ReadXml(reader);
                        return xmlDS;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (reader != null)
                            reader.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 将DataSet转换为xml对象字符串
        /// </summary>
        /// <param name="xmlDS"></param>
        /// <returns></returns>

        public static string ConvertDataSetToXML(DataSet xmlDS)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                //从stream装载到XmlTextReader 
                using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.Default))
                {

                    try
                    {
                        //用WriteXml方法写入文件.  
                        xmlDS.WriteXml(writer);
                        int count = (int)stream.Length;
                        byte[] arr = new byte[count];
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.Read(arr, 0, count);

                        return Encoding.Default.GetString(arr).Trim();
                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (writer != null) writer.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 将DataSet转换为xml文件
        /// </summary>
        /// <param name="xmlDS"></param>
        /// <param name="xmlFile"></param>

        public static void ConvertDataSetToXMLFile(DataSet xmlDS, string xmlFile)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                //从stream装载到XmlTextReader 
                using (XmlTextWriter writer = new XmlTextWriter(stream, Encoding.Default))
                {

                    try
                    {
                        //用WriteXml方法写入文件.  
                        xmlDS.WriteXml(writer);
                        int count = (int)stream.Length;
                        byte[] arr = new byte[count];
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.Read(arr, 0, count);

                        //返回Encoding.Default编码的文本  
                        using (StreamWriter sw = new StreamWriter(xmlFile))
                        {
                            sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                            string ss = System.Text.Encoding.Default.GetString(arr).Trim();
                            sw.WriteLine(ss);
                            sw.Flush();
                            sw.Close();
                        }
                        //排版生成的xml文档
                        XmlDocument doc = new XmlDocument();
                        doc.Load(xmlFile);
                        doc.Save(xmlFile);
                        doc = null;

                    }
                    catch (System.Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (writer != null) writer.Close();
                    }
                }
            }
        }


        // Xml结构的文件读到DataSet中
        public static DataSet XmlToDataTableByFile(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            string xmlString = doc.InnerXml;
            DataSet XmlDS = ConvertXMLToDataSet(xmlString);

            return XmlDS;
        }
    }
}
