using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevExpress.Mvvm;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace bq_Client.DataModel
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    public abstract class SampleDataCommon : ViewModelBase
    {
        public SampleDataCommon(String title, int _index)
        {
            this.itemId = _index;
            this._title = title;
        }

        private int itemId;
        public int ItemId
        {
            get
            {
                return this.itemId;
            }
        }
        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value, "Title"); }
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String title, int _index, bool isFlowBreak, string groupHeader)
            : base(title, _index)
        {
            this.IsFlowBreak = isFlowBreak;
            this.GroupHeader = groupHeader;
        }

        public void InitailData()
        {
            TotalV = "0.00";
            TotalA = "0.00";
            SOC = "0.00";
            MaxV = "0.000";
            MinV = "0.000";
            AvaV = "0.000";
            MaxT = "0.0";
            MinT = "0.0";
            RemainC = "0.00";
            Cycle = "0";
            Status = 0xff;
            TotalC = "0.00";
        }

        private string _GroupHeader;
        private bool _IsFlowBreak;

        public bool IsFlowBreak
        {
            get { return _IsFlowBreak; }
            set { this.SetProperty(ref this._IsFlowBreak, value, "IsFlowBreak"); }
        }

        public string GroupHeader
        {
            get { return _GroupHeader; }
            set { this.SetProperty(ref this._GroupHeader, value, "GroupHeader"); }
        }

        private string totalV = "0.00";
        public string TotalV
        {
            get { return this.totalV + " V"; }
            set { this.SetProperty(ref this.totalV, value, "TotalV"); }
        }
        private string totalA = "0.00";
        public string TotalA
        {
            get { return this.totalA + " A"; }
            set { this.SetProperty(ref this.totalA, value, "TotalA"); }
        }
        private string soc = "0.00";
        public string SOC
        {
            get { return this.soc + " %"; }
            set { this.SetProperty(ref this.soc, value, "SOC"); }
        }
        private string maxV = "0.000";
        public string MaxV
        {
            get { return this.maxV + " V"; }
            set { this.SetProperty(ref this.maxV, value, "MaxV"); }
        }
        private string minV = "0.000";
        public string MinV
        {
            get { return this.minV + " V"; }
            set { this.SetProperty(ref this.minV, value, "MinV"); }
        }
        private string avaV = "0.000";
        public string AvaV
        {
            get { return this.avaV + " V"; }
            set { this.SetProperty(ref this.avaV, value, "AvaV"); }
        }
        private string maxT = "0.0";
        public string MaxT
        {
            get { return this.maxT + " ℃"; }
            set { this.SetProperty(ref this.maxT, value, "MaxT"); }
        }
        private string minT = "0.0";
        public string MinT
        {
            get { return this.minT + " ℃"; }
            set { this.SetProperty(ref this.minT, value, "MinT"); }
        }
        private string remainC = "0.00";
        public string RemainC
        {
            get { return this.remainC + " Ah"; }
            set { this.SetProperty(ref this.remainC, value, "RemainC"); }
        }
        private string totalC = "0.00";
        public string TotalC
        {
            get { return this.totalC + " Ah"; }
            set { this.SetProperty(ref this.totalC, value, "TotalC"); }
        }

        private string cycle = "0";
        public string Cycle
        {
            get { return this.cycle + " 次"; }
            set { this.SetProperty(ref this.cycle, value, "Cycle"); }
        }

        private byte status = 0xff;
        public byte Status
        {
            get { return this.status; }
            set { this.SetProperty(ref this.status, value, "Status"); }
        }

        private int cellNum = 12;
        public int CellNum
        {
            get { return this.cellNum; }
            set { this.SetProperty(ref this.cellNum, value, "CellNum"); }
        }

        private int temperatureNum = 8;
        public int TemperatureNum
        {
            get { return this.temperatureNum; }
            set { this.SetProperty(ref this.temperatureNum, value, "TemperatureNum"); }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    ///
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        public static SampleDataSource Instance
        {
            get { return _sampleDataSource; }
        }
        static readonly SampleDataSource _sampleDataSource = new SampleDataSource();

        ObservableCollection<SampleDataItem> _masteritems;
        public ObservableCollection<SampleDataItem> MasterItems
        {
            get { return Instance._masteritems; }
        }
        ObservableCollection<SampleDataItem> _packItems;
        public ObservableCollection<SampleDataItem> PackItems
        {
            get { return Instance._packItems; }
        }

        public int CurrentMasterId;

        public static SampleDataItem GetMasterItem(int itemId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = SampleDataSource.Instance.MasterItems.Where((item) => item.ItemId.Equals(itemId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }
        public static SampleDataItem GetPackItem(int itemId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = SampleDataSource.Instance.PackItems.Where((item) => item.ItemId.Equals(itemId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public SampleDataSource()
        {
            _masteritems = new ObservableCollection<SampleDataItem>();
            for (int j = 1; j <= Properties.Settings.Default.MasterNum; j++)
            {
                _masteritems.Add(new SampleDataItem(
                    "--", j, false, "Master" + j.ToString().PadLeft(2, '0')));

            }
            _packItems = new ObservableCollection<SampleDataItem>();
            for (int i = 1; i <= Properties.Settings.Default.PackNum; i++)
            {
                _packItems.Add(new SampleDataItem(
                    "Pack" + i.ToString().PadLeft(2, '0'), i, false, "Master" + i.ToString().PadLeft(2, '0')));
            }

        }
    }

    public class CustInfoModel : ViewModelBase
    {
        public CustInfoModel()
        {

        }

        public CustInfoModel(int _id, int _groupNum, int _packNum, string _custName, string _custStatus,byte _custFault)
        {
            this.CustId = _id;
            this.GroupNum = _groupNum;
            this.PackNum = _packNum;
            this.CustName = _custName;
            this.CustStatus = _custStatus;
            this.CustFault = _custFault;
        }

        private byte custFault = 0x00;
        public byte CustFault
        {
            get { return this.custFault; }
            set { this.SetProperty(ref this.custFault, value, "CustFault"); }
        }

        private int custId = 0;
        public int CustId
        {
            get { return this.custId; }
            set { this.SetProperty(ref this.custId, value, "CustId"); }
        }

        private int groupNum = 0;
        public int GroupNum
        {
            get { return this.groupNum; }
            set { this.SetProperty(ref this.groupNum, value, "GroupNum"); }
        }

        private int packNum = 0;
        public int PackNum
        {
            get { return this.packNum; }
            set { this.SetProperty(ref this.packNum, value, "PackNum"); }
        }

        private string custName = "--";
        public string CustName
        {
            get { return this.custName; }
            set { this.SetProperty(ref this.custName, value, "CustName"); }
        }

        private string custStatus = "--";
        public string CustStatus
        {
            get { return this.custStatus; }
            set { this.SetProperty(ref this.custStatus, value, "CustStatus"); }
        }

    }
}
