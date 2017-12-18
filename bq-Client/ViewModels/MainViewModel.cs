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
using DevExpress.Xpf.Grid;
using System.IO;

namespace bq_Client.ViewModels
{
    public class MainViewModel : ViewModel
    {
        /// <summary>
        /// 读取pack数据结构Excel
        /// </summary>
        public DataTable DtPack = new DataTable();
        /// <summary>
        /// 读取packGroup数据结构Excel
        /// </summary>
        public DataTable DtPackgroup = new DataTable();
        /// <summary>
        /// 读取ModeText数据结构Excel
        /// </summary>
        public DataTable DtModeText = new DataTable();

        private CustInfoModel custSelectItem = new CustInfoModel();
        /// <summary>
        /// 变量值       
        /// </summary>
        public CustInfoModel CustSelectItem
        {
            get { return custSelectItem; }
            set
            {
                SetProperty<CustInfoModel>(ref custSelectItem, value, "CustSelectItem");
            }
        }
        protected override void OnViewLoaded()
        {
            ExcuteTimer.Interval = TimeSpan.FromSeconds(Properties.Settings.Default.Internal);
            ExcuteTimerCust.Interval = TimeSpan.FromSeconds(10);
            ExcuteTimerCust.Tick += ExcuteTimerCust_Tick;

            //初始化Provider
            Provider.InitailProvider();
            //原来是读取excel，由于受到补丁影响现在统一改成用.xml文件
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\" + "Protocol_Cloud.xml";
            if (File.Exists(filePath))
            {
                DataSet ds = DealXML.XmlToDataTableByFile(filePath);
                DtPack = ds.Tables["PackInfoNode"];
                DtPackgroup = ds.Tables["GroupInfoNode"];
                DtModeText = ds.Tables["ModeInfoNode"];
            }

            SetMasterVariable();
            SetPackVariable();

            base.OnViewLoaded();
            Navigate("MasterItemsPage");
            FuncUpdateStatus += o => CustSelectItem = o as CustInfoModel;
        }

        /// <summary>
        /// 根据协议文件初始化主机信息
        /// </summary>
        private void SetMasterVariable()
        {
            DataRow[] drPackGroupist = DtPackgroup.Select("type='PackGroup'");
            DataRow[] drLinkList = DtPackgroup.Select("type='LinkList'");
            DataRow[] drProtectList = DtPackgroup.Select("type='ProtectList'");
            DataRow[] drWarnList = DtPackgroup.Select("type='WarnList'");

            dataCurrentMaster.Columns.Add("时间");
            GridColumn dcTime = new GridColumn();
            dcTime.Header = "时间";
            dcTime.FieldName = "时间";
            MasterListColumn.Add(dcTime);
            foreach (var item in drPackGroupist)
            {
                SummaryVarViewModel dv = new SummaryVarViewModel(item);
                packGroupList.Add(dv);
                GridColumn dc = new GridColumn();
                dc.Header = dv.VarNameUnit;
                dc.FieldName = dv.VarNameUnit;
                MasterListColumn.Add(dc);
                dataCurrentMaster.Columns.Add(dv.VarNameUnit);
                MasterListComobox.Add(dv.VarNameUnit);
            }
            foreach (var item in drLinkList)
            {
                packGroupList.Find(delegate(SummaryVarViewModel dv)
                {
                    return dv.VarName == item["linkName"].ToString();
                }).AddLinkStatus(item);
            }
            foreach (var item in drProtectList)
            {
                StateVarViewModel sv = new StateVarViewModel(item);
                allstateList.Add(sv);
            }
            foreach (var item in drWarnList)
            {
                StateVarViewModel sv = new StateVarViewModel(item);
                allstateList.Add(sv);
            }
        }
        /// <summary>
        /// 根据协议文件初始化Pack信息
        /// </summary>
        private void SetPackVariable()
        {
            DataRow[] drCellList = DtPack.Select("type='CellList'");
            DataRow[] drOtherList = DtPack.Select("type='OtherList'");
            DataRow[] drCellLinkList = DtPack.Select("type='CellLinkList'");
            DataRow[] drOtherLinkList = DtPack.Select("type='OtherLinkList'");
            DataRow[] drMode = DtPack.Select("type='Mode'");
            DataRow[] drMainLinkList = DtPack.Select("type='MainLinkList'");
            DataRow[] drMainList = DtPack.Select("type='MainList'");

            foreach (var item in drCellList)
            {
                DigitVarViewModel dv = new DigitVarViewModel(item);
                teleMeterCellList.Add(dv);
            }
            foreach (var item in drOtherList)
            {
                DigitVarViewModel dv = new DigitVarViewModel(item);
                teleMeterOtherList.Add(dv);
            }
            foreach (var item in drCellLinkList)
            {
                //匿名方法
                teleMeterCellList.Find(delegate(DigitVarViewModel dv)
                {
                    return dv.VarName == item["linkName"].ToString();
                }).AddLinkStatus(item);
            }
            foreach (var item in drOtherLinkList)
            {
                //lambda表达式(亦可以用语句块)
                teleMeterOtherList.Find(dv => dv.VarName == item["linkName"].ToString()).AddLinkStatus(item);
            }
            foreach (var item in drMode)
            {
                modeTextDigit = new DigitVarViewModel(item);
            }
            foreach (DataRow item in DtModeText.Rows)
            {
                byte _modeByte = Convert.ToByte(item[0].ToString().Trim(), 16);
                modeDict.Add(_modeByte, item[1].ToString().Trim());
            }


            foreach (var item1 in drMainList)
            {
                DigitVarViewModel dv = new DigitVarViewModel(item1);
                foreach (var item in drMainLinkList)
                {
                    if (item["linkName"].ToString() == item1["caption"].ToString())
                    {
                        dv.AddLinkStatus(item);
                    }
                }
                GetMainDigit(item1["caption"].ToString(), dv);
            }
            
        }
        /// <summary>
        /// 根据参数名称初始化具体信息
        /// </summary>
        /// <param name="caption"></param>
        /// <param name="dv"></param>
        private void GetMainDigit(string caption, DigitVarViewModel dv)
        {
            switch (caption)
            {
                case "充放电流":
                    DigitVarViewModel.CopyTo(BatAmpDigitVar, dv);
                    MainListDigit.Add(BatAmpDigitVar);
                    break;
                case "总电压":
                    DigitVarViewModel.CopyTo(BatVoltDigitVar, dv);
                    MainListDigit.Add(BatVoltDigitVar);
                    break;
                case "单节最高电压":
                    DigitVarViewModel.CopyTo(cellMaxVoltDigit, dv);
                    MainListDigit.Add(cellMaxVoltDigit);
                    break;
                case "单节最低电压":
                    DigitVarViewModel.CopyTo(cellMinVoltDigit, dv);
                    MainListDigit.Add(cellMinVoltDigit);
                    break;
                case "单节平均电压":
                    DigitVarViewModel.CopyTo(cellAvrVoltDigit, dv);
                    MainListDigit.Add(cellAvrVoltDigit);
                    break;
                default:
                    break;
            }
        }
    }
}
