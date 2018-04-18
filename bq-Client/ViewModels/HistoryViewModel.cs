using DevExpress.Mvvm;
using System;
using DevExpress.Xpf.WindowsUI.Navigation;
using bq_Client.DataModel;
using SocketLibrary;
using System.Data;
using System.Collections.Generic;
using System.Windows.Controls;
using DevExpress.Xpf.WindowsUI;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using DevExpress.Xpf.Core;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Globalization;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Grid;
using System.Windows;
using bq_Client.DataFactory;

namespace bq_Client.ViewModels
{
    class HistoryViewModel : ViewModel, INavigationAware
    {

        private object packIndex = 5;
        public object PackIndex
        {
            get { return packIndex; }
            set
            {
                SetProperty<object>(ref packIndex, value, "PackIndex");
            }
        }
        private object masterIndex = 1;
        public object MasterIndex
        {
            get { return masterIndex; }
            set
            {
                SetProperty<object>(ref masterIndex, value, "MasterIndex");
            }
        }

        private bool offConnect = true;
        /// <summary>
        /// 离线标志       
        /// </summary>
        public bool OffConnect
        {
            get { return offConnect; }
            set
            {
                SetProperty<bool>(ref offConnect, value, "OffConnect");
            }
        }

        #region 读取历史信息并分析
        private DateTime startTime = DateTime.Now;
        /// <summary>
        /// 开始时间       
        /// </summary>
        public DateTime StartTime
        {
            get { return startTime; }
            set
            {
                SetProperty<DateTime>(ref startTime, value, "StartTime");
                CreateChartFun(null);
            }
        }

        private DateTime endTime = DateTime.Now;
        /// <summary>
        /// 结束时间      
        /// </summary>
        public DateTime EndTime
        {
            get { return endTime; }
            set
            {
                SetProperty<DateTime>(ref endTime, value, "EndTime");
                CreateChartFun(null);
            }
        }

        private int dataRange = 0;
        /// <summary>
        /// 数据范围       
        /// </summary>
        public int DataRange
        {
            get { return dataRange; }
            set
            {
                SetProperty<int>(ref dataRange, value, "DataRange");
                CreateChartFun(null);
            }
        }

        private string dataTypeChart = "总电压(V)";
        /// <summary>
        /// 图表数据类型       
        /// </summary>
        public string DataTypeChart
        {
            get { return dataTypeChart; }
            set
            {
                SetProperty<string>(ref dataTypeChart, value, "DataTypeChart");
                CreateChartFun(null);
            }
        }
        #endregion

        /// <summary>
        /// 历史信息用到的刷新链表,需要用到ObservableCollection，而不能直接用list，前者可以实现在表删除、添加等操作时提供通知
        /// </summary>
        public ObservableCollection<DigitVarViewModel> PackHistoryDataList = new ObservableCollection<DigitVarViewModel>();
        /// <summary>
        /// Pack缓存和历史数据的列
        /// </summary>
        public ObservableCollection<GridColumn> PackListColumn = new ObservableCollection<GridColumn> { };
        /// <summary>
        /// Pack实时数据缓存
        /// </summary>
        public DataTable dataHistoryPack = new DataTable();
        /// <summary>
        /// Pack数据类型list
        /// </summary>
        public ObservableCollection<string> PackDataTypeComobox = new ObservableCollection<string> { };

        /// <summary>
        /// 遥测量电芯链表，左侧ListBox的绑定Source
        /// </summary>
        public List<DigitVarViewModel> PackCellList = new List<DigitVarViewModel>();
        /// <summary>
        /// 遥测量其他信息链表，右侧ListBox的绑定Source
        /// </summary>
        public List<DigitVarViewModel> PackOtherList = new List<DigitVarViewModel>();
        /// <summary>
        /// 温度链表
        /// </summary>
        public List<DigitVarViewModel> TempertureList = new List<DigitVarViewModel>();
        /// <summary>
        /// 主要信息：包括SOC,总电压，总电流等
        /// </summary>
        public List<DigitVarViewModel> PackMainList = new List<DigitVarViewModel>();



        /// <summary>
        /// Master缓存和历史数据的列
        /// </summary>
        public List<GridColumn> MasterDataColumn = new List<GridColumn> { };



        private DataTable dataHistory = new DataTable();
        /// <summary>
        /// 历史数据       
        /// </summary>
        public DataTable DataHistory
        {
            get { return dataHistory; }
            set
            {
                SetProperty<DataTable>(ref dataHistory, value, "DataHistory");
            }
        }
        /// <summary>
        /// 更新数据分析图表委托
        /// </summary>
        public DeleFunc1 CreateChartFun = null;

        /// <summary>
        /// 更新Pack选择之后更新绑定委托
        /// </summary>
        public DeleFunc1 AfterSelectPackFun = null;

        /// <summary>
        /// 发送读取历史信息命令
        /// </summary>
        /// <param name="_selectIndex">0-询问Master信息，1-询问Pack信息</param>
        public void SendReadHistoryCommand(int _selectIndex)
        {
            if (socketClient != null)
            {
                string TableName = "";
                string cmdText = "";
                byte eventType;
                DataTable dtTableName = MySqlHelper.GetDataSetByTableName(MySqlHelper.GetConn(), System.Data.CommandType.Text, "SHOW tables;", TableName, null).Tables[0];
                List<string> listTN = new List<string> { };
                foreach (DataRow dr in dtTableName.Rows)
                {
                    listTN.Add(dr[0].ToString());
                }

                if (_selectIndex == 1)
                {
                    string tableName = BLLCommon.GetPackTableName(ref TableName, StartTime, EndTime, Properties.Settings.Default.DTUIndex, (int)MasterIndex - 1, (int)packIndex - 1, listTN.ToArray());
                    if (tableName == null)
                    {
                        DXMessageBox.Show("所选日期没有数据存在！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (DataRange == 0)
                    {

                        cmdText = "SELECT pack_info,insert_time FROM " + tableName;
                    }
                    else
                    {
                        cmdText = string.Format("SELECT pack_info,insert_time FROM " + tableName + " limit {0}", DataRange);
                    }
                    eventType = CS_AskReply.ClientReplyPack;
                }
                else
                {
                    string tableName = BLLCommon.GetMasterTableName(ref TableName, StartTime, EndTime, Properties.Settings.Default.DTUIndex, (int)MasterIndex - 1, listTN.ToArray());
                    if (tableName == null)
                    {
                        DXMessageBox.Show("所选日期没有数据存在！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (DataRange == 0)
                    {

                        cmdText = "SELECT group_info,insert_time FROM " + tableName;
                    }
                    else
                    {
                        cmdText = string.Format("SELECT group_info,insert_time FROM " + tableName + " limit {0}", DataRange);
                    }
                    eventType = CS_AskReply.ClientReplyGroup;
                }


                BLLCommon.ShowWaitWindow();
                DataTable dtData = new DataTable();
                try
                {
                    dtData = MySqlHelper.GetDataSetByTableName(MySqlHelper.GetConn(), System.Data.CommandType.Text, cmdText, TableName, null).Tables[0];
                }
                catch (Exception)
                {
                    DXMessageBox.Show("服务器连接异常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate()
                           {
                               BLLCommon.CloseWaitWindow(true);
                           }));
                    return;
                }
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    BLLCommon.CloseWaitWindow(false);
                }));
                if (dtData.Rows.Count >= 1)
                {
                    GetServerData(dtData, eventType);
                }
                else
                {
                    DXMessageBox.Show("当前查询模块没有数据，请确保DTU连接正常或者下位机工作正常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                //List<byte> SendTimeList = new List<byte> { };
                //SendTimeList.AddRange(BitConverter.GetBytes(DataRange));
                //SendTimeList.AddRange(BLLCommon.DateTimeToBytes(StartTime));
                //SendTimeList.AddRange(BLLCommon.DateTimeToBytes(EndTime));
                //if (_selectIndex == 0)

                //    BLLCommon.SendToServer(CS_AskReply.ClientAskGroup, Properties.Settings.Default.DTUIndex, Convert.ToByte((int)MasterIndex - 1), 0xff, SendTimeList.ToArray());

                //else
                //    BLLCommon.SendToServer(CS_AskReply.ClientAskPack, Properties.Settings.Default.DTUIndex, Convert.ToByte((int)MasterIndex - 1), Convert.ToByte((int)packIndex - 1), SendTimeList.ToArray());
            }
        }

        public void SendReadPackStatus()
        {
            if (socketClient != null)
            {
                string TableName = "packs_status";
                string cmdText = string.Format("SELECT cellNum,temperatureNum FROM {0} where cust_id={1} and group_id={2} and pack_id={3};",
                   TableName, Properties.Settings.Default.DTUIndex, (int)MasterIndex - 1, (int)PackIndex - 1);
                DataTable dtData = new DataTable();
                try
                {
                    dtData = MySqlHelper.GetDataSetByTableName(MySqlHelper.GetConn(), System.Data.CommandType.Text, cmdText, TableName, null).Tables[0];
                }
                catch (Exception)
                {
                    DXMessageBox.Show("服务器连接异常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                GetServerData(dtData, CS_AskReply.ClientReplyPackStatus);
                //BLLCommon.SendToServer(CS_AskReply.ClientAskPackStatus, Properties.Settings.Default.DTUIndex, Convert.ToByte((int)MasterIndex - 1), Convert.ToByte((int)PackIndex - 1), BitConverter.GetBytes(1));
            }
        }

        /// <summary>
        /// 导出Csv
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public bool ExportCSV(string FilePath)
        {
            if (!CSVFileHelper.SaveCSV(DataHistory, FilePath))
            {
                return false;
            }
            return true;
        }

        private void GetServerData(DataTable dt, byte eventType)
        {
            if (eventType == CS_AskReply.ClientReplyPackStatus)
            {
                ChangePackChoose(Convert.ToInt32(dt.Rows[0]["cellNum"]), Convert.ToInt32(dt.Rows[0]["temperatureNum"]));
            }
            else
            {
                DataHistory.Rows.Clear();
                string errorStr = string.Empty;
                foreach (DataRow dr in dt.Rows)
                {
                    DataRow drHistory = DataHistory.NewRow();
                    drHistory["时间"] = dr[1];
                    byte[] arrByteInfo = BLLCommon.GetArrData(dr);
                    try
                    {
                        if (eventType == CS_AskReply.ClientReplyPack)
                        {
                            foreach (var item in PackHistoryDataList)
                            {
                                item.UpdateData(arrByteInfo);
                                drHistory[item.VarNameUnit] = item.VarValue;
                            }
                        }
                        else if (eventType == CS_AskReply.ClientReplyGroup)
                        {
                            foreach (var item in packGroupList)
                            {
                                item.UpdateData(arrByteInfo);
                                drHistory[item.VarNameUnit] = item.VarValue;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        errorStr = "部分数据存在异常！";
                        continue;
                    }

                    DataHistory.Rows.Add(drHistory);
                }
                if (errorStr != string.Empty)
                {
                    DXMessageBox.Show(errorStr, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                CreateChartFun(null);
            }
        }

        /*public void FuncAction(object dtObject, byte eventType)
        {
            DataTable dt = dtObject as DataTable;
            //使用ui元素

            if (dt.Rows.Count >= 1)
            {
                BLLCommon.CloseWaitWindow(false);
                GetServerData(dt, eventType);
            }
            else
            {
                BLLCommon.CloseWaitWindow(false);
                DXMessageBox.Show("当前查询模块没有数据，请确保DTU连接正常或者下位机工作正常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }*/

        /// <summary>
        /// itemViewModel初始化
        /// </summary>
        private void LoadDataList()
        {

            //Master历史信息表列
            foreach (var item in MasterListColumn)
            {
                GridColumn gc = new GridColumn();
                gc.Header = item.Header;
                gc.FieldName = item.FieldName;
                MasterDataColumn.Add(gc);
            }

            if (socketClient != null)
            {
                OffConnect = socketClient.OffLine;
            }
            else
            {
                OffConnect = true;
            }
        }

        public void ChangePackChoose(int _cellNum, int _TemperatureNum)
        {
            PackCellList.Clear();
            TempertureList.Clear();
            PackOtherList.Clear();
            PackMainList.Clear();
            PackListColumn.Clear();
            PackDataTypeComobox.Clear();
            dataHistoryPack.Columns.Clear();
            PackHistoryDataList.Clear();

            //将电芯列表复制过来
            foreach (var item in teleMeterCellList)
            {
                DigitVarViewModel dv = new DigitVarViewModel();
                DigitVarViewModel.CopyTo(dv, item);
                PackCellList.Add(dv);
            }

            //将其他列表复制过来
            foreach (var item in teleMeterOtherList)
            {
                DigitVarViewModel dv = new DigitVarViewModel();
                DigitVarViewModel.CopyTo(dv, item);
                if (dv.VarName.Contains("电池温度"))
                    TempertureList.Add(dv);
                else
                {
                    PackOtherList.Add(dv);
                }
            }
            foreach (var item in MainListDigit)
            {
                DigitVarViewModel dv = new DigitVarViewModel();
                DigitVarViewModel.CopyTo(dv, item);
                PackMainList.Add(dv);
            }

            InitialPackViewModelData(_cellNum, _TemperatureNum, ref PackCellList, ref PackOtherList, ref TempertureList, ref PackMainList);


            foreach (var item in PackMainList)
            {
                PackHistoryDataList.Add(item);
            }
            foreach (var item in PackCellList)
            {
                PackHistoryDataList.Add(item);
            }
            foreach (var item in PackOtherList)
            {
                PackHistoryDataList.Add(item);
            }
            foreach (var item in TempertureList)
            {
                PackHistoryDataList.Add(item);
            }

            //Pack历史信息汇总
            dataHistoryPack.Columns.Add("时间");
            GridColumn dcTime = new GridColumn();
            dcTime.Header = "时间";
            dcTime.FieldName = "时间";
            PackListColumn.Add(dcTime);
            foreach (DigitVarViewModel dv in PackHistoryDataList)
            {
                GridColumn dc = new GridColumn();
                dc.Header = dv.VarNameUnit;
                dc.FieldName = dv.VarNameUnit;
                PackListColumn.Add(dc);
                dataHistoryPack.Columns.Add(dv.VarNameUnit);
                PackDataTypeComobox.Add(dv.VarNameUnit);
            }
            AfterSelectPackFun(null);
        }


        /// <summary>
        /// 从返回的列表中获取字节数组
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>       

        #region INavigationAware Members
        public void NavigatedFrom(NavigationEventArgs e)
        {
            ExcuteTimerCust.Start();
        }
        public void NavigatedTo(NavigationEventArgs e)
        {
            ExcuteTimerCust.Stop();
            //modelIndex = 4;
            //FuncDoAction = new DeleFunc2(FuncAction);
            FuncRefershOffLine = sign => OffConnect = (bool)sign;
            LoadDataList();

        }
        public void NavigatingFrom(NavigatingEventArgs e)
        {
            if (socketClient != null)
            {
                ExcuteTimer.Stop();
            }
            //FuncDoAction = null;
            FuncRefershOffLine = null;
        }
        #endregion
    }
}
