using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;

namespace bq_Client.ViewModels
{
    /// <summary>
    /// 定义的遥信（状态信息）数据类型和方法
    /// </summary>
    public class StateVarViewModel : DataViewModel
    {
        public StateVarViewModel() { }

        public StateVarViewModel(DataRow dr)
        {
            VarName = dr["caption"].ToString();
            if (dr["type"].ToString().Contains("Warn"))
            {
                StateType = "Warn";
            }
            else
            {
                StateType = "Protect";
            }
            VarByteIndex = Convert.ToInt32(dr["byteIndex"]);
            VarBitIndex = Convert.ToInt32(dr["bitIndex"]);
            VarByteNum = Convert.ToDouble(dr["byte"]);
            StateValue = false;
        }

        public void UpdateData(byte[] arrData)
        {
            try
            {
                if (Convert.ToInt32(GetObjectByIndex(arrData, VarByteNum, VarByteIndex, VarBitIndex)) != 0)
                {
                    StateValue = true;
                }
                else
                {
                    StateValue = false;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private Boolean stateValue;
        /// <summary>
        /// 遥信量的状态值
        /// </summary>
        public Boolean StateValue
        {
            get { return stateValue; }
            set
            {
                stateValue = value;
                VarPromptStatus = stateType + (stateValue ? "On" : "Off");
                OnPropertyChanged(new PropertyChangedEventArgs("StateValue"));
            }
        }


        private string stateType;
        /// <summary>
        /// 遥信量类型
        /// </summary>
        public string StateType
        {
            get { return stateType; }
            set
            {
                stateType = value;
                OnPropertyChanged(new PropertyChangedEventArgs("StateType"));
            }
        }

        private string varPromptStatus;
        /// <summary>
        /// 遥信量实时状态
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

    }


}