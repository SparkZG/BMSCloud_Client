using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;

namespace bq_Client.ViewModels
{
    public class SummaryVarViewModel : DataViewModel
    {
        public SummaryVarViewModel() { }
        public SummaryVarViewModel(DataRow dr)
        {
            VarName = dr["caption"].ToString();
            VarUnit = dr["unit"].ToString();
            VarScale = Convert.ToDouble(dr["scale"]);
            VarByteNum = Convert.ToDouble(dr["byte"]);
            VarByteIndex = Convert.ToInt32(dr["byteIndex"]);
        }
        public static void CopyTo(SummaryVarViewModel desSv, SummaryVarViewModel sourceSv)
        {
            desSv.VarName = sourceSv.VarName;
            desSv.VarUnit = sourceSv.VarUnit;
            desSv.VarScale = sourceSv.VarScale;
            desSv.VarByteNum = sourceSv.VarByteNum;
            desSv.VarByteIndex = sourceSv.VarByteIndex;
            desSv.VarBitIndex = sourceSv.VarBitIndex;
            desSv.MonomerByteIndex = sourceSv.MonomerByteIndex;
            desSv.MonomerByteNum = sourceSv.MonomerByteNum;
            desSv.PackIDByteIndex = sourceSv.PackIDByteIndex;
            desSv.PackIDByteNum = sourceSv.PackIDByteNum;
            desSv.VarPackID = sourceSv.VarPackID;
            desSv.VarMonomerIndex = sourceSv.VarMonomerIndex;
            desSv.HasLink = sourceSv.HasLink;
        }
        public void AddLinkStatus(DataRow dr)
        {
            if (dr["caption"].ToString().Contains("pack"))
            {
                HasLink = true;
                packIDByteIndex = Convert.ToInt32(dr["byteIndex"]);
                packIDByteNum = Convert.ToDouble(dr["byte"]);
            }
            else
            {
                HasLink = true;
                MonomerByteIndex = Convert.ToInt32(dr["byteIndex"]);
                MonomerByteNum = Convert.ToDouble(dr["byte"]);
            }
        }

        public void UpdateData(byte[] arrData)
        {
            try
            {
                VarValue = Convert.ToDouble(GetObjectByIndex(arrData, VarByteNum, VarByteIndex)) * VarScale;
                VarValueUnit = VarValue + VarUnit;
                if (HasLink)
                {
                    VarPackID = ((byte)GetObjectByIndex(arrData, PackIDByteNum, PackIDByteIndex) + 1).ToString();
                    VarMonomerIndex = ((byte)GetObjectByIndex(arrData, MonomerByteNum, MonomerByteIndex) + 1).ToString();
                }
                else
                {
                    VarPackID = "——";
                    VarMonomerIndex = "——";
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        /// <summary>
        /// 变量名称+单位      
        /// </summary>
        public string VarNameUnit
        {
            get { return VarName + "(" + VarUnit + ")"; }
        }

        private string varValueUnit;
        /// <summary>
        /// 变量值+Unit      
        /// </summary>
        public string VarValueUnit
        {
            get { return varValueUnit; }
            set
            {
                varValueUnit = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VarValueUnit"));
            }
        }

        private double varValue;
        /// <summary>
        /// 变量值       
        /// </summary>
        public double VarValue
        {
            get { return varValue; }
            set
            {
                varValue = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VarValue"));
            }
        }

        private string varUnit;
        /// <summary>
        /// 变量单位       
        /// </summary>
        public string VarUnit
        {
            get { return varUnit; }
            set
            {
                varUnit = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VarUnit"));
            }
        }

        private double varScale;
        /// <summary>
        /// 变量精度       
        /// </summary>
        public double VarScale
        {
            get { return varScale; }
            set
            {
                varScale = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VarScale"));
            }
        }

        private bool hasLink = false;
        /// <summary>
        /// 变量实时状态       
        /// </summary>
        public bool HasLink
        {
            get { return hasLink; }
            set
            {
                hasLink = value;
                OnPropertyChanged(new PropertyChangedEventArgs("HasLink"));
            }
        }

        #region PACKID
        private string varPackID;
        /// <summary>
        /// PackId       
        /// </summary>
        public string VarPackID
        {
            get { return varPackID; }
            set
            {
                varPackID = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VarPackID"));
            }
        }

        private int packIDByteIndex;
        /// <summary>
        /// PackID字节索引       
        /// </summary>
        public int PackIDByteIndex
        {
            get { return packIDByteIndex; }
            set
            {
                packIDByteIndex = value;
                OnPropertyChanged(new PropertyChangedEventArgs("PackIDByteIndex"));
            }
        }
        private double packIDByteNum;
        /// <summary>
        /// PackID字节数      
        /// </summary>
        public double PackIDByteNum
        {
            get { return packIDByteNum; }
            set
            {
                packIDByteNum = value;
                OnPropertyChanged(new PropertyChangedEventArgs("PackIDByteNum"));
            }
        }
        #endregion

        #region 单体索引
        private string varMonomerIndex;
        /// <summary>
        /// 单体索引       
        /// </summary>
        public string VarMonomerIndex
        {
            get { return varMonomerIndex; }
            set
            {
                varMonomerIndex = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VarMonomerIndex"));
            }
        }

        private int monomerByteIndex;
        /// <summary>
        /// Monomer字节索引       
        /// </summary>
        public int MonomerByteIndex
        {
            get { return monomerByteIndex; }
            set
            {
                monomerByteIndex = value;
                OnPropertyChanged(new PropertyChangedEventArgs("MonomerByteIndex"));
            }
        }
        private double monomerByteNum;
        /// <summary>
        /// Monomer字节数      
        /// </summary>
        public double MonomerByteNum
        {
            get { return monomerByteNum; }
            set
            {
                monomerByteNum = value;
                OnPropertyChanged(new PropertyChangedEventArgs("MonomerByteNum"));
            }
        }
        #endregion
    }
}
