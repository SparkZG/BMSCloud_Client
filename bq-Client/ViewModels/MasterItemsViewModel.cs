using System.Collections.Generic;
using DevExpress.Mvvm;
using DevExpress.Xpf.WindowsUI.Navigation;
using bq_Client.DataModel;
using SocketLibrary;
using System;
using System.Data;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using DevExpress.Xpf.Core;
using System.Windows;
using bq_Client.DataFactory;


namespace bq_Client.ViewModels
{
    //A View Model for a MasterItemsPage
    public class MasterItemsViewModel : ViewModel, INavigationAware
    {

        IEnumerable<SampleDataItem> masterItems;
        public MasterItemsViewModel() { }
        public IEnumerable<SampleDataItem> MasterItems
        {
            get { return masterItems; }
            private set { SetProperty<IEnumerable<SampleDataItem>>(ref masterItems, value, "MasterItems"); }
        }
        public void LoadState(object navigationParameter)
        {
            MasterItems = SampleDataSource.Instance.MasterItems;
            foreach (var _masteritem in MasterItems)
            {
                _masteritem.InitailData();
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

        private bool autoRenovate;
        /// <summary>
        /// 自动刷新标识       
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

        private bool isEnable;
        /// <summary>
        /// 是否可动态刷新       
        /// </summary>
        public bool IsEnable
        {
            get { return isEnable; }
            set
            {
                SetProperty<bool>(ref isEnable, value, "IsEnable");
                if (value)
                    AutoRenovate = true;
                else
                    AutoRenovate = false;

            }
        }

        private double renovateInterval = Properties.Settings.Default.Internal;
        /// <summary>
        /// 刷新时间       
        /// </summary>
        public double RenovateInterval
        {
            get { return renovateInterval; }
            set
            {
                SetProperty<double>(ref renovateInterval, value, "RenovateInterval");
                ExcuteTimer.Interval = TimeSpan.FromSeconds(value);
                Properties.Settings.Default.Internal = value;
                Properties.Settings.Default.Save();
            }
        }

        private int custSelectIndex = Properties.Settings.Default.DTUIndex;
        /// <summary>
        /// 客户索引       
        /// </summary>
        public int CustSelectIndex
        {
            get { return custSelectIndex; }
            set
            {
                SetProperty<int>(ref custSelectIndex, value, "CustSelectIndex");
            }
        }

        private CustInfoModel custSelectItem = null;
        /// <summary>
        /// 客户model       
        /// </summary>
        public CustInfoModel CustSelectItem
        {
            get { return custSelectItem; }
            set
            {
                SetProperty<CustInfoModel>(ref custSelectItem, value, "CustSelectItem");
                if (value == null)
                {
                    return;
                }
                Properties.Settings.Default.DTUIndex = (ushort)value.CustId;
                Properties.Settings.Default.MasterNum = value.GroupNum;
                Properties.Settings.Default.PackNum = value.PackNum / value.GroupNum;
                Properties.Settings.Default.Save();

                //刷新状态栏数据                
                FuncUpdateStatus(value);

                //根据选择的客户生成相应的MasterItem
                if (value.GroupNum < SampleDataSource.Instance.MasterItems.Count)
                {
                    for (int i = SampleDataSource.Instance.MasterItems.Count - 1; i >= value.GroupNum; i--)
                    {
                        SampleDataSource.Instance.MasterItems.RemoveAt(i);
                    }
                }
                else if (value.GroupNum > SampleDataSource.Instance.MasterItems.Count)
                {
                    for (int i = SampleDataSource.Instance.MasterItems.Count + 1; i <= value.GroupNum; i++)
                    {
                        SampleDataSource.Instance.MasterItems.Add(new SampleDataItem("--", i, false, "Master" + i.ToString().PadLeft(2, '0')));
                    }
                }
                //根据选择的客户生成相应的PackItem
                int _packNumInMaster = value.PackNum / value.GroupNum;
                if (_packNumInMaster < SampleDataSource.Instance.PackItems.Count)
                {
                    for (int i = SampleDataSource.Instance.PackItems.Count - 1; i >= _packNumInMaster; i--)
                    {
                        SampleDataSource.Instance.PackItems.RemoveAt(i);
                    }
                }
                else if (_packNumInMaster > SampleDataSource.Instance.PackItems.Count)
                {
                    for (int i = SampleDataSource.Instance.PackItems.Count + 1; i <= _packNumInMaster; i++)
                    {
                        SampleDataSource.Instance.PackItems.Add(new SampleDataItem("Pack" + i.ToString().PadLeft(2, '0'), i, false, "--"));
                    }
                }
            }
        }

        /*public void FuncAction(object o, byte eventType)
        {
            if (eventType != CS_AskReply.ClientReplyGroupStatus)
            {
                return;
            }
            DataTable dtMaster = o as DataTable;
            if (dtMaster.Rows.Count == 0)
            {
                equalNullNum++;
                if (equalNullNum >= 2)
                {
                    AutoRenovate = false;
                    equalNullNum = 0;
                    DXMessageBox.Show("当前Cust没有数据，请确保DTU连接正常或者下位机工作正常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }
            equalNullNum = 0;

            foreach (var item in MasterItems)
            {
                foreach (DataRow dr in dtMaster.Rows)
                {
                    int _groupIndex = Convert.ToInt32(dr["group_id"]) + 1;

                    if (item.ItemId == _groupIndex)
                    {
                        item.AvaV = dr["AvaV"].ToString();
                        item.Cycle = dr["Cycle"].ToString();
                        item.MaxT = dr["MaxT"].ToString();
                        item.MaxV = dr["MaxV"].ToString();
                        item.MinT = dr["MinT"].ToString();
                        item.MinV = dr["MinV"].ToString();
                        item.RemainC = dr["RemainC"].ToString();
                        item.SOC = dr["SOC"].ToString();
                        item.Status = Convert.ToByte(dr["Status"]);
                        item.TotalA = dr["TotalA"].ToString();
                        item.TotalC = dr["TotalC"].ToString();
                        item.TotalV = dr["TotalV"].ToString();
                        break;
                    }
                    else
                    {
                        item.InitailData();
                    }
                }
            }

        }*/


        private void MasterItems_Tick(object sender, EventArgs e)
        {
            if (socketClient != null)
            {
                string TableName = "groups_status";
                string cmdText = string.Format("SELECT * FROM {0} where cust_id={1}",
                    TableName, Properties.Settings.Default.DTUIndex);
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
                if (dtData.Rows.Count == 0)
                {
                    equalNullNum++;
                    if (equalNullNum >= 2)
                    {
                        AutoRenovate = false;
                        equalNullNum = 0;
                        DXMessageBox.Show("当前Cust没有数据，请确保DTU连接正常或者下位机工作正常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    return;
                }
                equalNullNum = 0;

                foreach (var item in MasterItems)
                {
                    foreach (DataRow dr in dtData.Rows)
                    {
                        int _groupIndex = Convert.ToInt32(dr["group_id"]) + 1;

                        if (item.ItemId == _groupIndex)
                        {
                            item.AvaV = dr["AvaV"].ToString();
                            item.Cycle = dr["Cycle"].ToString();
                            item.MaxT = dr["MaxT"].ToString();
                            item.MaxV = dr["MaxV"].ToString();
                            item.MinT = dr["MinT"].ToString();
                            item.MinV = dr["MinV"].ToString();
                            item.RemainC = dr["RemainC"].ToString();
                            item.SOC = dr["SOC"].ToString();
                            item.Status = Convert.ToByte(dr["Status"]);
                            item.TotalA = dr["TotalA"].ToString();
                            item.TotalC = dr["TotalC"].ToString();
                            item.TotalV = dr["TotalV"].ToString();
                            break;
                        }
                        else
                        {
                            item.InitailData();
                        }
                    }
                }

                //BLLCommon.SendToServer(CS_AskReply.ClientAskGroupStatus, Properties.Settings.Default.DTUIndex, Convert.ToByte(0xff), Convert.ToByte(0xff), BitConverter.GetBytes(1));
            }
        }

        public void RefreshMasterItems(object _item)
        {
            var _selectItem = _item as CustInfoModel;

            if (_selectItem.CustStatus == "0")
            {
                IsEnable = false;
                foreach (var item in MasterItems)
                {
                    item.InitailData();
                }
            }
            else
            {
                if (!IsEnable)
                {
                    IsEnable = true;
                    MasterItems_Tick(this, null);
                }
            }
        }

        #region INavigationAware Members
        public void NavigatedFrom(NavigationEventArgs e)
        {
            IsFirst = false;
        }
        public void NavigatedTo(NavigationEventArgs e)
        {
            IsFirst = true;
            LoadState(e.Parameter);
            //modelIndex = 1;
            ExcuteTimer.Tick += new EventHandler(MasterItems_Tick);
            //FuncDoAction = new DeleFunc2(FuncAction);
            FuncUpdateStatus += RefreshMasterItems;
            FuncRefershOffLine = sign => OffConnect = (bool)sign;
            if (socketClient != null)
            {
                OffConnect = socketClient.OffLine;

                //开启自动刷新
                AutoRenovate = true;
                //连接之后就读取一次
                ExcuteTimerCust_Tick(this, null);
            }
            else
            {
                OffConnect = true;
            }
        }
        public void NavigatingFrom(NavigatingEventArgs e)
        {
            if (socketClient != null)
            {
                ExcuteTimer.Stop();
            }
            //FuncDoAction = null; 
            ExcuteTimer.Tick -= new EventHandler(MasterItems_Tick);
            FuncUpdateStatus -= RefreshMasterItems;
            FuncRefershOffLine = null;
        }
        #endregion
    }
}
