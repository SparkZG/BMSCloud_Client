using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;

namespace bq_Client.ViewModels
{
    /// <summary>
    /// 定义包含变量名、单位、精度、字节数、值、均衡状态等的数据类型，多用于遥测信息    
    /// </summary>
    public class DigitVarViewModel : DataViewModel
    {
        public DigitVarViewModel() { }

        public DigitVarViewModel(string name, double byteNum, string unit, double scale, bool hasStatus, bool hasBalance)
        {
            VarName = name;
            VarUnit = unit;
            VarScale = scale;
            VarByteNum = byteNum;
            HasBalance = hasBalance;
            HasPromptStatus = hasBalance;
        }

        public DigitVarViewModel(DataRow dr)
        {
            VarName = dr["caption"].ToString();
            VarUnit = dr["unit"].ToString();
            VarScale = Convert.ToDouble(dr["scale"]);
            VarByteNum = Convert.ToDouble(dr["byte"]);
            VarByteIndex = Convert.ToInt32(dr["byteIndex"]);
            VarBitIndex = Convert.ToInt32(dr["bitIndex"]);
        }

        public void AddLinkStatus(DataRow dr)
        {
            if (dr["caption"].ToString().Contains("告警"))
            {
                HasPromptStatus = true;
                StatusBitIndex = Convert.ToInt32(dr["bitIndex"]);
                StatusByteIndex = Convert.ToInt32(dr["byteIndex"]);
                StatusByteNum = Convert.ToDouble(dr["byte"]);
            }
            else
            {
                HasBalance = true;
                BalanceBitIndex = Convert.ToInt32(dr["bitIndex"]);
                BalanceByteIndex = Convert.ToInt32(dr["byteIndex"]);
                BalanceByteNum = Convert.ToDouble(dr["byte"]);
            }
        }

        public static void CopyTo(DigitVarViewModel desDv, DigitVarViewModel sourceDv)
        {
            desDv.VarName = sourceDv.VarName;
            desDv.VarUnit = sourceDv.VarUnit;
            desDv.VarScale = sourceDv.VarScale;
            desDv.VarByteNum = sourceDv.VarByteNum;
            desDv.VarByteIndex = sourceDv.VarByteIndex;
            desDv.VarBitIndex = sourceDv.VarBitIndex;
            desDv.StatusBitIndex = sourceDv.StatusBitIndex;
            desDv.StatusByteIndex = sourceDv.StatusByteIndex;
            desDv.StatusByteNum = sourceDv.StatusByteNum;
            desDv.BalanceBitIndex = sourceDv.BalanceBitIndex;
            desDv.BalanceByteIndex = sourceDv.BalanceByteIndex;
            desDv.BalanceByteNum = sourceDv.BalanceByteNum;
            desDv.HasPromptStatus = sourceDv.HasPromptStatus;
            desDv.HasBalance = sourceDv.HasBalance;
        }

        public void UpdateData(byte[] arrData)
        {
            try
            {
                VarValue = Convert.ToDouble(GetObjectByIndex(arrData, VarByteNum, VarByteIndex, VarBitIndex)) * VarScale;
                if (HasPromptStatus)
                {
                    byte refresh = (byte)GetObjectByIndex(arrData, StatusByteNum, StatusByteIndex, StatusBitIndex);
                    if (refresh == 0x00)
                    {
                        VarPromptStatus = "NormalOn";
                    }
                    else if (refresh == 0x01)
                    {
                        VarPromptStatus = "UnderOn";
                    }
                    else if (refresh == 0x02)
                    {
                        VarPromptStatus = "OverOn";
                    }
                    else
                    {
                        VarPromptStatus = "OtherOn";
                    }
                }
                if (HasBalance)
                {
                    int boolBalance = (int)GetObjectByIndex(arrData, balanceByteNum, balanceByteIndex, balanceBitIndex);
                    if (boolBalance == 0)
                    {
                        IsBalance = false;
                    }
                    else
                    {
                        IsBalance = true;
                    }
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


        #region 变量状态

        private bool hasPromptStatus = false;
        /// <summary>
        /// 变量实时状态       
        /// </summary>
        public bool HasPromptStatus
        {
            get { return hasPromptStatus; }
            set
            {
                hasPromptStatus = value;
                OnPropertyChanged(new PropertyChangedEventArgs("HasPromptStatus"));
            }
        }


        private string varPromptStatus;
        /// <summary>
        /// 变量实时状态       
        /// </summary>
        public string VarPromptStatus
        {
            get { return varPromptStatus; }
            set
            {
                varPromptStatus = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VarPromptStatus"));
            }
        }

        private int statusByteIndex;
        /// <summary>
        /// 告警状态字节索引      
        /// </summary>
        public int StatusByteIndex
        {
            get { return statusByteIndex; }
            set
            {
                statusByteIndex = value;
                OnPropertyChanged(new PropertyChangedEventArgs("StatusByteIndex"));
            }
        }

        private int statusBitIndex;
        /// <summary>
        /// 告警状态位索引       
        /// </summary>
        public int StatusBitIndex
        {
            get { return statusBitIndex; }
            set
            {
                statusBitIndex = value;
                OnPropertyChanged(new PropertyChangedEventArgs("StatusBitIndex"));
            }
        }
        private double statusByteNum = 1;
        /// <summary>
        /// 告警状态字节数      
        /// </summary>
        public double StatusByteNum
        {
            get { return statusByteNum; }
            set
            {
                statusByteNum = value;
                OnPropertyChanged(new PropertyChangedEventArgs("StatusByteNum"));
            }
        }
        #endregion

        #region 变量均衡状态

        private bool hasBalance = false;
        /// <summary>
        /// 变量实时状态       
        /// </summary>
        public bool HasBalance
        {
            get { return hasBalance; }
            set
            {
                hasBalance = value;
                OnPropertyChanged(new PropertyChangedEventArgs("HasBalance"));
            }
        }

        private Boolean isBalance;
        /// <summary>
        /// 变量均衡状态       
        /// </summary>
        public Boolean IsBalance
        {
            get { return isBalance; }
            set
            {
                isBalance = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsBalance"));
            }
        }

        private int balanceByteIndex;
        /// <summary>
        /// 告警状态字节索引      
        /// </summary>
        public int BalanceByteIndex
        {
            get { return balanceByteIndex; }
            set
            {
                balanceByteIndex = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BalanceByteIndex"));
            }
        }

        private int balanceBitIndex;
        /// <summary>
        /// 告警状态位索引       
        /// </summary>
        public int BalanceBitIndex
        {
            get { return balanceBitIndex; }
            set
            {
                balanceBitIndex = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BalanceBitIndex"));
            }
        }
        private double balanceByteNum = 0.125;
        /// <summary>
        /// 告警状态字节数      
        /// </summary>
        public double BalanceByteNum
        {
            get { return balanceByteNum; }
            set
            {
                balanceByteNum = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BalanceByteNum"));
            }
        }
        #endregion
    }

    public class DataViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
        public object GetObjectByIndex(byte[] arrData, double byteNum, int byteIndex, int bitIndex = 0)
        {
            if (byteNum == 2)
            {
                //移位默认是32位int数,需先转换成16进制数----有符号

                return Convert.ToInt16(Convert.ToString((arrData[byteIndex] << 8) + arrData[byteIndex + 1], 16), 16);
            }
            else if (byteNum == 1)
            {
                return Convert.ToByte(arrData[byteIndex]);
            }
            else if (byteNum < 1)
            {
                return ((arrData[byteIndex] >> bitIndex) & 0x01);
            }
            else if (byteNum == 4)
            {
                //默认为32位故无需多余转换-------有符号
                return Convert.ToInt32((arrData[byteIndex] << 24) + (arrData[byteIndex + 1] << 16) + (arrData[byteIndex + 2] << 8) + (arrData[byteIndex + 3]));
            }
            return 0;
        }

        private string varName;
        /// <summary>
        /// 变量名称       
        /// </summary>
        public string VarName
        {
            get { return varName; }
            set
            {
                varName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VarName"));
            }
        }

        private int varByteIndex;
        /// <summary>
        /// 变量名称       
        /// </summary>
        public int VarByteIndex
        {
            get { return varByteIndex; }
            set
            {
                varByteIndex = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VarByteIndex"));
            }
        }

        private int varBitIndex;
        /// <summary>
        /// 变量名称       
        /// </summary>
        public int VarBitIndex
        {
            get { return varBitIndex; }
            set
            {
                varBitIndex = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VarBitIndex"));
            }
        }

        private double varByteNum;
        /// <summary>
        /// 变量字节数      
        /// </summary>
        public double VarByteNum
        {
            get { return varByteNum; }
            set
            {
                varByteNum = value;
                OnPropertyChanged(new PropertyChangedEventArgs("VarByteNum"));
            }
        }

    }
}
