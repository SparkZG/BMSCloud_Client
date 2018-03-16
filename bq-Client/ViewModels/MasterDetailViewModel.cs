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
using System.Threading;
using System.Windows;
using bq_Client.DataFactory;

namespace bq_Client.ViewModels
{
    public class MasterDetailViewModel : ViewModel, INavigationAware
    {
        public int packGroupIndex = 0;
        /// <summary>
        /// 协议中信息列表
        /// </summary>
        public List<SummaryVarViewModel> MasterDataList = new List<SummaryVarViewModel>();
        /// <summary>
        /// 正在发生的遥信状态值（warn，protect），从warnStateList中获取
        /// </summary>
        public ObservableCollection<StateVarViewModel> stateList = new ObservableCollection<StateVarViewModel>();

        public DataTable dataCurrent = new DataTable();

        private string RenovateTime = "";

        private string dataType = "SOC(%)";
        /// <summary>
        /// 数据类型       
        /// </summary>
        public string DataType
        {
            get { return dataType; }
            set
            {
                SetProperty<string>(ref dataType, value, "DataType");
            }
        }
        private bool autoRenovate;
        /// <summary>
        /// 变量值       
        /// </summary>
        public bool AutoRenovate
        {
            get { return autoRenovate; }
            set
            {
                SetProperty<bool>(ref autoRenovate, value, "AutoRenovate");
                if (autoRenovate)
                {
                    ExcuteTimer.Start();
                }
                else
                {
                    ExcuteTimer.Stop();
                }
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
                if (value)
                {
                    AutoRenovate = false;
                }
                else
                {
                    AutoRenovate = true;
                }
            }
        }

        /// <summary>
        /// 更新PackGroup实时信息图表委托
        /// </summary>
        public DeleFunc1 UpdateChartFun = null;

        /// <summary>
        /// 动态折线图datasource
        /// </summary>
        //public ObservableCollection<DataModel.ValueByDate> ChartSource = new ObservableCollection<DataModel.ValueByDate>();

        IEnumerable<SampleDataItem> packitems;
        public IEnumerable<SampleDataItem> PackItems
        {
            get { return packitems; }
            private set { SetProperty<IEnumerable<SampleDataItem>>(ref packitems, value, "PackItems"); }
        }

        public SampleDataItem selectedItem;
        public MasterDetailViewModel() { }
        public SampleDataItem SelectedItem
        {
            get { return selectedItem; }
            set { SetProperty<SampleDataItem>(ref selectedItem, value, "SelectedItem"); }
        }
        private void LoadState(object navigationParameter)
        {
            PackItems = SampleDataSource.Instance.PackItems;
            if (navigationParameter == null)
            {
                navigationParameter = SampleDataSource.Instance.CurrentMasterId;
            }
            SampleDataItem item = SampleDataSource.GetMasterItem(Convert.ToInt32(navigationParameter));
            SelectedItem = item;
            foreach (var _packitem in PackItems)
            {
                _packitem.InitailData();
                _packitem.GroupHeader = SelectedItem.GroupHeader;
            }
        }

        public void LoadDataList()
        {
            foreach (var item in packGroupList)
            {
                SummaryVarViewModel sv = new SummaryVarViewModel();
                SummaryVarViewModel.CopyTo(sv, item);
                MasterDataList.Add(sv);
            }

            dataCurrent = dataCurrentMaster.Clone();
            packGroupIndex = Convert.ToInt32(SelectedItem.GroupHeader.Substring(SelectedItem.GroupHeader.Length - 2, 2)) - 1;


            if (socketClient != null)
            {
                OffConnect = socketClient.OffLine;

                //开启自动刷新
                AutoRenovate = true;
                MasterDetail_Tick(this, null);
            }
            else
            {
                OffConnect = true;
            }
        }
        private void MasterDetail_Tick(object sender, EventArgs e)
        {
            if (socketClient != null)
            {
                string TableName = "groupsinfo" + DateTime.Now.ToString("yyyyMMdd");
                string cmdText = string.Format("SELECT group_info,insert_time FROM {0} where cust_id={1} and group_id={2} order by id desc limit {3};",
                    TableName, Properties.Settings.Default.DTUIndex, packGroupIndex, 1);
                DataTable dtData = new DataTable();
                try
                {
                    dtData = MySqlHelper.GetDataSetByTableName(MySqlHelper.GetConn(), System.Data.CommandType.Text, cmdText, TableName, null).Tables[0];
                }
                catch (Exception)
                {
                    AutoRenovate = false;
                    equalNullNum = 0;
                    DXMessageBox.Show("服务器连接异常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (dtData.Rows.Count == 0)
                {
                    equalNullNum++;
                    if (equalNullNum >= 2)
                    {
                        AutoRenovate = false;
                        equalNullNum = 0;
                        DXMessageBox.Show("当前Master没有数据，请确保DTU连接正常或者下位机工作正常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }
                equalNullNum = 0;
                //使用ui元素
                try
                {
                    RenovateData(dtData.Rows[0]);
                }
                catch (Exception)
                {
                    AutoRenovate = false;
                    equalNullNum = 0;
                    DXMessageBox.Show("主机信息异常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                

                //询问PackStatus
                TableName = "packs_status";
                cmdText = string.Format("SELECT * FROM {0} where cust_id={1} and group_id={2};",
                TableName, Properties.Settings.Default.DTUIndex, packGroupIndex);
                try
                {
                    dtData = MySqlHelper.GetDataSetByTableName(MySqlHelper.GetConn(), System.Data.CommandType.Text, cmdText, TableName, null).Tables[0];
                }
                catch (Exception)
                {
                    AutoRenovate = false;
                    equalNullNum = 0;
                    DXMessageBox.Show("服务器连接异常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (dtData.Rows.Count == 0)
                {
                    equalNullNum++;
                    if (equalNullNum >= 2)
                    {
                        AutoRenovate = false;
                        equalNullNum = 0;
                        DXMessageBox.Show("当前Master获取Pack状态数据失败，请确保DTU连接正常或者下位机工作正常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }
                equalNullNum = 0;
                foreach (var item in PackItems)
                {
                    foreach (DataRow dr in dtData.Rows)
                    {
                        int _packIndex = Convert.ToInt32(dr["pack_id"]);
                        int _packNum = item.ItemId - 1;
                        if (_packNum == _packIndex)
                        {
                            item.AvaV = dr["AvaV"].ToString();
                            item.Cycle = dr["Cycle"].ToString();
                            item.MaxV = dr["MaxV"].ToString();
                            item.MinV = dr["MinV"].ToString();
                            item.RemainC = dr["RemainC"].ToString();
                            item.SOC = dr["SOC"].ToString();
                            item.TotalA = dr["TotalA"].ToString();
                            item.TotalC = dr["TotalC"].ToString();
                            item.TotalV = dr["TotalV"].ToString();
                            item.Status = Convert.ToByte(dr["Status"]);
                            item.CellNum = Convert.ToInt32(dr["CellNum"]);
                            item.TemperatureNum = Convert.ToInt32(dr["TemperatureNum"]);
                            break;
                        }
                        else
                        {
                            item.InitailData();
                        }
                    }
                }
                //BLLCommon.SendToServer(CS_AskReply.ClientAskGroup, Properties.Settings.Default.DTUIndex, Convert.ToByte(packGroupIndex), Convert.ToByte(0xff), BitConverter.GetBytes(1));
                //Thread.Sleep(100);
                //BLLCommon.SendToServer(CS_AskReply.ClientAskPackStatus, Properties.Settings.Default.DTUIndex, Convert.ToByte(packGroupIndex), Convert.ToByte(0xff), BitConverter.GetBytes(1));
            }
        }

        /*public void FuncAction(object dtObject, byte eventType)
        {
            DataTable dt = dtObject as DataTable;
            if (dt.Rows.Count == 0)
            {
                equalNullNum++;
                if (equalNullNum >= 4)
                {
                    AutoRenovate = false;
                    equalNullNum = 0;
                    DXMessageBox.Show("当前Master没有数据，请确保DTU连接正常或者下位机工作正常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return;
            }
            equalNullNum = 0;
            //使用ui元素         
            if (eventType == CS_AskReply.ClientReplyGroup)
            {
                try
                {
                    RenovateData(dt.Rows[0]);
                }
                catch (Exception)
                {
                    AutoRenovate = false;
                    equalNullNum = 0;
                    DXMessageBox.Show("主机信息异常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (eventType == CS_AskReply.ClientReplyPackStatus)
            {
                foreach (var item in PackItems)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        int _packIndex = Convert.ToInt32(dr["pack_id"]);
                        int _packNum = item.ItemId - 1;
                        if (_packNum == _packIndex)
                        {
                            item.AvaV = dr["AvaV"].ToString();
                            item.Cycle = dr["Cycle"].ToString();
                            item.MaxV = dr["MaxV"].ToString();
                            item.MinV = dr["MinV"].ToString();
                            item.RemainC = dr["RemainC"].ToString();
                            item.SOC = dr["SOC"].ToString();
                            item.TotalA = dr["TotalA"].ToString();
                            item.TotalC = dr["TotalC"].ToString();
                            item.TotalV = dr["TotalV"].ToString();
                            item.Status = Convert.ToByte(dr["Status"]);
                            item.CellNum = Convert.ToInt32(dr["CellNum"]);
                            item.TemperatureNum = Convert.ToInt32(dr["TemperatureNum"]);
                            break;
                        }
                        else
                        {
                            item.InitailData();
                        }
                    }
                }
            }
        }*/
        private void RenovateData(DataRow dr)
        {
            byte[] arrData = BLLCommon.GetArrData(dr);
            DataRow drCurrent = dataCurrent.NewRow();
            drCurrent["时间"] = DateTime.Now;
            foreach (var item in MasterDataList)
            {
                item.UpdateData(arrData);
                drCurrent[item.VarNameUnit] = item.VarValue;
            }

            UpdateChartFun(Convert.ToDouble(drCurrent[DataType]));
            dataCurrent.Rows.Add(drCurrent);

            if (dataCurrent.Rows.Count > 10000)
            {
                for (int i = 0; i < dataCurrent.Rows.Count - 10000; i++)
                {
                    dataCurrent.Rows.RemoveAt(0);
                }
            }

            if (RenovateTime != dr[1].ToString())
            {
                //更新图表Source

                RenovateTime = dr[1].ToString();
            }

            stateList.Clear();
            foreach (var item in allstateList)
            {
                item.UpdateData(arrData);
                if (item.StateValue)
                {
                    stateList.Add(item);
                }
            }
        }

        #region INavigationAware Members
        public void NavigatedFrom(NavigationEventArgs e)
        {
            IsFirst = false;
            Properties.Settings.Default.HistoryMasterIndex = packGroupIndex;
            Properties.Settings.Default.HistoryPackIndex = -1;
            Properties.Settings.Default.Save();
        }
        public void NavigatedTo(NavigationEventArgs e)
        {
            IsFirst = true;
            LoadState(e.Parameter);
            //FuncDoAction = new DeleFunc2(FuncAction);
            //modelIndex = 2;
            ExcuteTimer.Tick += new EventHandler(MasterDetail_Tick);
            FuncRefershOffLine = sign => OffConnect = (bool)sign;
        }
        public void NavigatingFrom(NavigatingEventArgs e)
        {
            if (socketClient != null)
            {
                ExcuteTimer.Stop();
            }
            //FuncDoAction = null;
            FuncRefershOffLine = null;
            ExcuteTimer.Tick -= new EventHandler(MasterDetail_Tick);
            SampleDataSource.Instance.CurrentMasterId = SelectedItem.ItemId;
        }
        #endregion
    }
}
