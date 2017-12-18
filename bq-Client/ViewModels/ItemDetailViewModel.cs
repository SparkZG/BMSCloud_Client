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

namespace bq_Client.ViewModels
{
    //A View Model for an ItemDetailPage
    public class ItemDetailViewModel : ViewModel, INavigationAware
    {
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
                if (value)
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

        private string dataType = "总电压(V)";
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

        private double soc = 0;
        /// <summary>
        /// 变量值       
        /// </summary>
        public double SOC
        {
            get { return soc; }
            set
            {
                SetProperty<double>(ref soc, value, "SOC");
            }
        }

        private string dataTime;
        /// <summary>
        /// 数据时间       
        /// </summary>
        public string DataTime
        {
            get { return dataTime; }
            set
            {
                SetProperty<string>(ref dataTime, value, "DataTime");
            }
        }

        private string modeText;
        /// <summary>
        /// 工作模式       
        /// </summary>
        public string ModeText
        {
            get { return modeText; }
            set
            {
                SetProperty<string>(ref modeText, value, "ModeText");
            }
        }


        #region 信号灯
        private byte statusV = 0;
        /// <summary>
        /// 电压信号灯标识
        /// </summary>
        public byte StatusV
        {
            get
            {
                return statusV;
            }
            set
            {
                SetProperty<byte>(ref statusV, value, "StatusV");
            }
        }

        private string tooltipV = "正常";
        /// <summary>
        /// 电压信号灯标识
        /// </summary>
        public string TooltipV
        {
            get
            {
                return tooltipV;
            }
            set
            {
                SetProperty<string>(ref tooltipV, value, "TooltipV");
            }
        }


        private byte statusA = 0;
        /// <summary>
        /// 电流信号灯标识
        /// </summary>
        public byte StatusA
        {
            get
            {
                return statusA;
            }
            set
            {
                SetProperty<byte>(ref statusA, value, "StatusA");
            }
        }

        private string tooltipA = "正常";
        /// <summary>
        /// 电压信号灯标识
        /// </summary>
        public string TooltipA
        {
            get
            {
                return tooltipA;
            }
            set
            {
                SetProperty<string>(ref tooltipA, value, "TooltipA");
            }
        }
        private byte statusSOC = 0;
        /// <summary>
        /// SOC信号灯标识
        /// </summary>
        public byte StatusSOC
        {
            get
            {
                return statusSOC;
            }
            set
            {
                SetProperty<byte>(ref statusSOC, value, "StatusSOC");
            }
        }

        private string tooltipSOC = "正常";
        /// <summary>
        /// 电压信号灯标识
        /// </summary>
        public string TooltipSOC
        {
            get
            {
                return tooltipSOC;
            }
            set
            {
                SetProperty<string>(ref tooltipSOC, value, "TooltipSOC");
            }
        }
        private byte statusT = 0;
        /// <summary>
        /// 温度信号灯标识
        /// </summary>
        public byte StatusT
        {
            get
            {
                return statusT;
            }
            set
            {
                SetProperty<byte>(ref statusT, value, "StatusT");
            }
        }

        private string tooltipT = "正常";
        /// <summary>
        /// 电压信号灯标识
        /// </summary>
        public string TooltipT
        {
            get
            {
                return tooltipT;
            }
            set
            {
                SetProperty<string>(ref tooltipT, value, "TooltipT");
            }
        }
        #endregion


        public int packGroupIndex = 0;
        public int packIndex = 0;
        /// <summary>
        /// 电池总压
        /// </summary>
        public DigitVarViewModel BatVolt;
        /// <summary>
        /// 充放电流
        /// </summary>
        public DigitVarViewModel BatTemp;
        /// <summary>
        /// 单体最高电压
        /// </summary>
        public DigitVarViewModel cellMaxVolt;
        /// <summary>
        /// 单体平均电压
        /// </summary>
        public DigitVarViewModel cellAvrVolt;
        /// <summary>
        /// 单体最低电压
        /// </summary>
        public DigitVarViewModel cellMinVolt;
        /// <summary>
        /// 工作模式
        /// </summary>
        public DigitVarViewModel modeDigit = new DigitVarViewModel();
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
        public List<DigitVarViewModel> MainList = new List<DigitVarViewModel>();

        /// <summary>
        /// 实时信息用到的刷新链表
        /// </summary>
        public List<DigitVarViewModel> CurrentDataList = new List<DigitVarViewModel>();
        /// <summary>
        /// 实时数据缓存
        /// </summary>
        public DataTable dataCurrent = new DataTable();
        /// <summary>
        /// Pack数据类型list
        /// </summary>
        public List<string> PackDataTypeComobox = new List<string> { };

        /// <summary>
        /// 更新实时信息图表委托
        /// </summary>
        public DeleFunc1 UpdateChartFun = null;



        public SampleDataItem selectedItem;
        public ItemDetailViewModel() { }
        public SampleDataItem SelectedItem
        {
            get { return selectedItem; }
            set { SetProperty<SampleDataItem>(ref selectedItem, value, () => SelectedItem); }
        }
        private void LoadState(object navigationParameter)
        {
            SampleDataItem item = SampleDataSource.GetPackItem(Convert.ToInt32(navigationParameter));
            SelectedItem = item;
        }

        /// <summary>
        /// itemViewModel初始化
        /// </summary>
        private void LoadDataList()
        {
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
                    //电池温度先不添加到teleMeterOtherList中！
                    PackOtherList.Add(dv);
                }
            }

            packGroupIndex = Convert.ToInt32(SelectedItem.GroupHeader.Substring(SelectedItem.GroupHeader.Length - 2, 2)) - 1;
            packIndex = SelectedItem.ItemId - 1;

        }

        /// <summary>
        /// itemPage初始化
        /// </summary>
        public void SetMainList()
        {

            //主要信息复制过来
            DigitVarViewModel.CopyTo(BatVolt, BatVoltDigitVar);
            MainList.Add(BatVolt);

            DigitVarViewModel.CopyTo(BatTemp, BatAmpDigitVar);
            MainList.Add(BatTemp);

            DigitVarViewModel.CopyTo(cellMaxVolt, cellMaxVoltDigit);
            MainList.Add(cellMaxVolt);

            DigitVarViewModel.CopyTo(cellAvrVolt, cellAvrVoltDigit);
            MainList.Add(cellAvrVolt);

            DigitVarViewModel.CopyTo(cellMinVolt, cellMinVoltDigit);
            MainList.Add(cellMinVolt);

            DigitVarViewModel.CopyTo(modeDigit, modeTextDigit);
            //工作模式字节索引
            modeDigit.VarByteIndex += (SelectedItem.CellNum - PackCellList.Count + SelectedItem.TemperatureNum - TempertureList.Count) * 3;

            //判断当前pack电芯数量和温度数量是否和加载的协议一样，不一样的话则重置字节索引
            InitialPackViewModelData(SelectedItem.CellNum, SelectedItem.TemperatureNum, ref PackCellList, ref PackOtherList, ref TempertureList, ref MainList);


            foreach (var item in MainList)
            {
                CurrentDataList.Add(item);
            }

            foreach (var item in PackCellList)
            {
                CurrentDataList.Add(item);
            }

            for (int i = TempertureList.Count - 1; i >= 0; i--)
            {
                //倒序添加到PackOtherList
                PackOtherList.Insert(2, TempertureList[i]);
            }
            foreach (var item in PackOtherList)
            {
                CurrentDataList.Add(item);
            }

            dataCurrent.Columns.Add("时间");
            foreach (DigitVarViewModel dv in CurrentDataList)
            {
                dataCurrent.Columns.Add(dv.VarNameUnit);
                PackDataTypeComobox.Add(dv.VarNameUnit);
            }

            if (socketClient != null)
            {
                OffConnect = socketClient.OffLine;
                //开启自动刷新
                AutoRenovate = true;
                ItemDetail_Tick(this, null);
            }
            else
            {
                OffConnect = true;
            }
        }

        private void ItemDetail_Tick(object sender, EventArgs e)
        {
            if (socketClient != null)
            {
                BLLCommon.SendToServer(CS_AskReply.ClientAskPack, Properties.Settings.Default.DTUIndex, Convert.ToByte(packGroupIndex), Convert.ToByte(packIndex), BitConverter.GetBytes(1));
            }
        }

        private void RenovateData(DataRow dr)
        {
            byte[] arrByteInfo = BLLCommon.GetArrData(dr);

            //刷新显示的同时缓存实时数据
            DataRow drCurrent = dataCurrent.NewRow();
            drCurrent["时间"] = DateTime.Now;
            foreach (var item in CurrentDataList)
            {
                item.UpdateData(arrByteInfo);
                drCurrent[item.VarNameUnit] = item.VarValue;
            }
            //利用委托更新图表Source
            UpdateChartFun(Convert.ToDouble(drCurrent[DataType]));
            dataCurrent.Rows.Add(drCurrent);

            if (dataCurrent.Rows.Count > 10000)
            {
                for (int i = 0; i < dataCurrent.Rows.Count - 10000; i++)
                {
                    dataCurrent.Rows.RemoveAt(0);
                }
            }

            //电压信号灯
            double D_value = cellMaxVolt.VarValue - cellMinVolt.VarValue;
            if (D_value >= 0 && D_value <= 0.1)
            {
                StatusV = 0x00;
                TooltipV = "正常";
            }
            else if (D_value > 0.1 && D_value <= 0.2)
            {
                StatusV = 0x01;
                TooltipV = "电芯压差三级告警";
            }
            else if (D_value > 0.2 && D_value <= 0.3)
            {
                StatusV = 0x02;
                TooltipV = "电芯压差二级告警";
            }
            else
            {
                StatusV = 0x03;
                TooltipV = "电芯压差一级告警";
            }
            if (BatVolt.VarPromptStatus != "NormalOn")
            {
                if (TooltipV == "正常")
                {
                    TooltipV = "";
                }
                StatusV = 0x03;
                TooltipV += BatVolt.VarName + "异常";
            }
            foreach (var item in PackCellList)
            {
                if (item.VarPromptStatus != "NormalOn")
                {
                    if (TooltipV == "正常")
                    {
                        TooltipV = "";
                    }
                    StatusV = 0x03;
                    TooltipV += item.VarName + "异常";
                    break;
                }
            }

            //电流信号灯
            if (BatTemp.VarPromptStatus != "NormalOn")
            {
                StatusA = 0x03;
                TooltipA = BatTemp.VarName + "异常";
            }
            else
            {
                TooltipA = "正常";
            }


            //更新SOC
            SOC = (PackOtherList.Find(delegate(DigitVarViewModel dv)
                {
                    return dv.VarName == "剩余容量";
                }).VarValue / PackOtherList.Find(delegate(DigitVarViewModel dv)
                {
                    return dv.VarName == "总容量";
                }).VarValue) * 100;
            if (SOC > 30 && SOC <= 100)
            {
                StatusSOC = 0x00;
                TooltipSOC = "正常";
            }
            else if (SOC <= 30 && SOC > 10)
            {
                StatusSOC = 0x01;
                TooltipSOC = "电量低于30%";
            }
            else if (SOC <= 10 && SOC > 5)
            {
                StatusSOC = 0x02;
                TooltipSOC = "电量低于10%";
            }
            else
            {
                StatusSOC = 0x03;
                TooltipSOC = "电量过低";
            }

            StatusT = 0x00;
            TooltipT = "正常";
            //温度信号灯
            foreach (var item in PackOtherList)
            {
                if (item.HasPromptStatus)
                {
                    if (item.VarPromptStatus != "NormalOn")
                    {
                        StatusT = 0x03;
                        TooltipT = item.VarName + "异常";
                        break;
                    }
                }
            }

            //刷新工作模式
            modeDigit.UpdateData(arrByteInfo);
            foreach (Byte item in modeDict.Keys)
            {
                if (item == Convert.ToByte(modeDigit.VarValue))
                {
                    ModeText = modeDict[item];
                    break;
                }
            }

            //当前数据时间
            DataTime = dr[1].ToString().Trim();
        }

        public void FuncAction(object dtObject, byte eventType)
        {
            if (eventType != CS_AskReply.ClientReplyPack)
            {
                return;
            }
            DataTable dt = dtObject as DataTable;
            if (dt.Rows.Count == 0)
            {
                equalNullNum++;
                if (equalNullNum >= 2)
                {
                    AutoRenovate = false;
                    equalNullNum = 0;
                    DXMessageBox.Show("当前Pack没有数据，请确保DTU连接正常或者下位机工作正常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }
            equalNullNum = 0;
            //使用ui元素
            try
            {
                RenovateData(dt.Rows[0]);
            }
            catch (Exception)
            {
                AutoRenovate = false;
                equalNullNum = 0;
                DXMessageBox.Show("数据异常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region INavigationAware Members
        public void NavigatedFrom(NavigationEventArgs e)
        {
            Properties.Settings.Default.HistoryMasterIndex = packGroupIndex;
            Properties.Settings.Default.HistoryPackIndex = packIndex;
            Properties.Settings.Default.Save();
        }
        public void NavigatedTo(NavigationEventArgs e)
        {
            LoadState(e.Parameter);
            modelIndex = 3;
            FuncDoAction = new DeleFunc2(FuncAction);
            ExcuteTimer.Tick += new EventHandler(ItemDetail_Tick);
            FuncRefershOffLine = sign => OffConnect = (bool)sign;
            LoadDataList();

        }
        public void NavigatingFrom(NavigatingEventArgs e)
        {
            if (socketClient != null)
            {
                ExcuteTimer.Stop();
            }
            FuncDoAction = null;
            FuncRefershOffLine = null;
            ExcuteTimer.Tick -= new EventHandler(ItemDetail_Tick);
        }
        #endregion
    }
}
