using System;
using System.Windows.Input;
using DevExpress.Mvvm;
using System.Windows.Media.Imaging;
using SocketLibrary;
using DevExpress.Xpf.Core;
using System.Windows;
using System.Windows.Threading;
using System.Threading;
using System.Collections.Generic;
using bq_Client.DataModel;
using System.Data;
using DevExpress.Xpf.Grid;
using System.Collections.ObjectModel;
using bq_Client.DataFactory;

namespace bq_Client.ViewModels
{
    public class ViewModel : ViewModelBase
    {
        ICommand onViewLoadedCommand;
        ICommand navigateCommand;
        ICommand navigateToMaster;
        ICommand navigateToPack;
        
        /// <summary>
        /// 是否是第一次点击，防止二次点击发生异常
        /// </summary>
        public bool IsFirst = true;

        #region Pack信息数据定义
        /// <summary>
        /// 电池总压
        /// </summary>
        public static DigitVarViewModel BatVoltDigitVar = new DigitVarViewModel();
        /// <summary>
        /// 充放电流
        /// </summary>
        public static DigitVarViewModel BatAmpDigitVar = new DigitVarViewModel();
        /// <summary>
        /// 单体最高电压
        /// </summary>
        public static DigitVarViewModel cellMaxVoltDigit = new DigitVarViewModel();
        /// <summary>
        /// 单体平均电压
        /// </summary>
        public static DigitVarViewModel cellAvrVoltDigit = new DigitVarViewModel();
        /// <summary>
        /// 单体最低电压
        /// </summary>
        public static DigitVarViewModel cellMinVoltDigit = new DigitVarViewModel();
        /// <summary>
        /// 工作模式
        /// </summary>
        public static DigitVarViewModel modeTextDigit = new DigitVarViewModel();
        /// <summary>
        /// 遥测量电芯链表，左侧ListBox的绑定Source
        /// </summary>
        public static List<DigitVarViewModel> teleMeterCellList = new List<DigitVarViewModel>();
        /// <summary>
        /// 遥测量其他信息链表，右侧ListBox的绑定Source
        /// </summary>
        public static List<DigitVarViewModel> teleMeterOtherList = new List<DigitVarViewModel>();

        /// <summary>
        /// 主要信息：包括SOC,总电压，总电流等
        /// </summary>
        public static List<DigitVarViewModel> MainListDigit = new List<DigitVarViewModel>();
        /// <summary>
        /// 工作模式字典
        /// </summary>
        public static Dictionary<Byte, String> modeDict = new Dictionary<Byte, String>();

        #endregion


        #region 主机信息数据定义
        /// <summary>
        /// Master实时数据缓存
        /// </summary>
        public static DataTable dataCurrentMaster = new DataTable();
        /// <summary>
        /// Master缓存和历史数据的列
        /// </summary>
        public static List<GridColumn> MasterListColumn = new List<GridColumn> { };
        /// <summary>
        /// Master数据类型list
        /// </summary>
        public static List<string> MasterListComobox = new List<string> { };
        /// <summary>
        /// 协议中信息列表
        /// </summary>
        public static List<SummaryVarViewModel> packGroupList = new List<SummaryVarViewModel>();
        /// <summary>
        /// 协议中所有遥信状态值（warn，protect）
        /// </summary>
        public static ObservableCollection<StateVarViewModel> allstateList = new ObservableCollection<StateVarViewModel>();
        #endregion

        /// <summary>
        /// 根据pack中包含的电芯数量和温度数量动态初始化界面和相应变量索引
        /// </summary>
        /// <param name="_cellNum"> 电芯数量</param>
        /// <param name="_temperatureNum"> 温度数量</param>
        /// <param name="cellList">电芯列表</param>
        /// <param name="otherList">其他信息列表</param>
        /// <param name="tempList">温度列表</param>
        /// <param name="mainList">主要信息列表</param>
        public static void InitialPackViewModelData(int _cellNum, int _temperatureNum, ref List<DigitVarViewModel> cellList, ref List<DigitVarViewModel> otherList, ref List<DigitVarViewModel> tempList, ref List<DigitVarViewModel> mainList)
        {
            int cellDnum = _cellNum - cellList.Count;
            int templateDnum = _temperatureNum - tempList.Count;
            //如果加载的协议和实际上传的数据格式不一致
            if (cellDnum != 0 || templateDnum != 0)
            {
                //给第一个电芯、温度赋字节索引值
                int NO1CellByteIndex = cellList[0].VarByteIndex;
                int NO1CellStatusByteIndex = cellList[0].StatusByteIndex + (cellDnum + templateDnum) * 2;
                int NO1CellBalanceByteIndex = cellList[0].BalanceByteIndex + (cellDnum + templateDnum) * 3;
                int NO1TempByteIndex = tempList[0].VarByteIndex + cellDnum * 1;
                int NO1TempStatusByteIndex = cellList[0].StatusByteIndex + cellDnum * 3 + templateDnum * 2; ;


                if (cellList.Count > _cellNum)
                {
                    for (int i = cellList.Count - 1; i >= _cellNum; i--)
                    {
                        cellList.RemoveAt(i);
                    }
                }
                else if (cellList.Count < _cellNum)
                {
                    for (int i = cellList.Count; i < _cellNum; i++)
                    {
                        DigitVarViewModel dv = new DigitVarViewModel("电芯" + (i + 1).ToString().PadLeft(2, '0'), 2, "V", 0.001, true, true);
                        cellList.Add(dv);
                    }
                }

                for (int i = 0; i < cellList.Count; i++)
                {
                    //10代表第一个数据的字节索引，电压数据占2字节，温度数据占2字节，21代表中间的不可变变量（其他信息和主要信息）的字节总数，告警位占1个字节，13代表其他不可变告警信息所占字节数
                    //cellList[i].VarByteIndex = 10 + i * 2;
                    cellList[i].VarByteIndex = NO1CellByteIndex + i * 2;
                    cellList[i].VarBitIndex = 0;
                    //cellList[i].StatusByteIndex = 10 + _cellNum * 2 + 1 + _temperatureNum * 2 + 21 + i * 1;
                    cellList[i].StatusByteIndex = NO1CellStatusByteIndex + i * 1;
                    cellList[i].StatusBitIndex = 0;
                    //cellList[i].BalanceByteIndex = cellList[0].StatusByteIndex + _cellNum * 1 + _temperatureNum * 1 + 13 + (int)(i * 0.125);
                    cellList[i].BalanceByteIndex = NO1CellBalanceByteIndex;
                    if (i < 8)
                    {
                        cellList[i].BalanceBitIndex = i;
                    }
                    else
                    {
                        cellList[i].BalanceBitIndex = i - 8;
                    }
                }

                //温度数量索引+=增加或减少的电芯数量*2
                otherList[1].VarByteIndex += cellDnum * 2;

                if (tempList.Count > _temperatureNum)
                {
                    for (int i = tempList.Count - 1; i >= _temperatureNum; i--)
                    {
                        tempList.RemoveAt(i);
                    }
                }
                else if (tempList.Count < _temperatureNum)
                {
                    for (int i = tempList.Count; i < _temperatureNum; i++)
                    {
                        DigitVarViewModel dv = new DigitVarViewModel("电池温度" + (i + 1).ToString().PadLeft(2, '0'), 1, "℃", 1, true, false);
                        tempList.Add(dv);
                    }
                }

                for (int i = 2; i < otherList.Count; i++)
                {
                    otherList[i].VarByteIndex += (cellDnum + templateDnum) * 2;
                }

                for (int i = tempList.Count - 1; i >= 0; i--)
                {
                    //10代表第一个数据的字节索引，电压数据占2字节，温度数据占2字节，21代表中间的不可变变量（其他信息和主要信息）的字节总数，告警位占1个字节，13代表其他不可变告警信息所占字节数
                    tempList[i].VarByteIndex = NO1TempByteIndex + i * 2;//1代表“温度数量”所占字节数
                    tempList[i].VarBitIndex = 0;
                    tempList[i].StatusByteIndex = NO1TempStatusByteIndex + i * 1;
                    tempList[i].StatusBitIndex = 0;
                }

                foreach (var item in mainList)
                {
                    item.VarByteIndex += (cellDnum + templateDnum) * 2;
                    if (item.HasPromptStatus)
                    {
                        item.StatusByteIndex += (cellDnum + templateDnum) * 3;
                    }
                }
            }

        }


        /// <summary>
        /// 实例化主委托1，现在不需要！！！
        /// </summary>
        //public static DeleFunc2 FuncDoAction = null;

        /// <summary>
        /// 实例化刷新状态栏委托
        /// </summary>
        public static DeleFunc1 FuncUpdateStatus = null;

        /// <summary>
        /// 实例化刷新在线标识委托
        /// </summary>
        public static DeleFunc1 FuncRefershOffLine = null;

        /// <summary>
        /// 用来判断当前处于哪个viewModel!1-MasterItems,2-MasterDetail,3-ItemDetail,4-HistoryDataPage,用来防止当接收数据延迟时造成的混乱，目前不需要！！！
        /// </summary>
        //public static int modelIndex = 1;

        //主定时器
        public static DispatcherTimer ExcuteTimer = new DispatcherTimer();
        //询问Cust定时器
        public static DispatcherTimer ExcuteTimerCust = new DispatcherTimer();

        public static ObservableCollection<CustInfoModel> CustListSource = new ObservableCollection<CustInfoModel> { };

        /// <summary>
        /// 与服务器连接着但是没有读到数据的次数，满三次就代表是所读模块没有数据
        /// </summary>
        public int equalNullNum = 0;
        /// <summary>
        /// 全局Socket
        /// </summary>
        public static MySocket socketClient = null;




        public bool StartConnent()
        {
            if (socketClient == null)
            {
                SocketFactory factory = new SocketFactory();
                string localIp = Properties.Settings.Default.LocalIP;
                int localPort = Properties.Settings.Default.LocalPort;
                socketClient = factory.CreateSocket(ServerType.TCPClient, localIp, localPort);

                string log = socketClient.Run();
                if (log != null)
                {
                    socketClient = null;
                    DXMessageBox.Show("连接服务器失败/n" + log, "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                socketClient.SocketEventHandler += new EventHandler(SocketEventHandler);
                ExcuteTimerCust_Tick(this, null);
                ExcuteTimerCust.Start();
            }
            return true;
        }

        public void ReConnect()
        {
            if (StartConnent())
            {
                ExcuteTimer.Start();
                if (FuncRefershOffLine != null)
                {
                    FuncRefershOffLine(false);
                }
            }
        }

        public void ExcuteTimerCust_Tick(object sender, EventArgs e)
        {
            if (socketClient != null)
            {
                string cmdText = "select * from customers;";
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    DataTable dtCust = new DataTable();
                    try
                    {
                        dtCust = MySqlHelper.GetDataSet(MySqlHelper.GetConn(), System.Data.CommandType.Text, cmdText, null).Tables[0];
                    }
                    catch (Exception)
                    {
                        DXMessageBox.Show("服务器连接异常！", "提示", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    foreach (DataRow item in dtCust.Rows)
                    {
                        CustInfoModel CIM = null;
                        for (int i = 0; i < CustListSource.Count; i++)
                        {
                            if (CustListSource[i].CustId == Convert.ToInt32(item["cust_id"]))
                            {
                                CIM = CustListSource[i];
                                break;
                            }
                        }

                        if (CIM == null)
                        {
                            CustListSource.Add(new CustInfoModel(
                                Convert.ToInt32(item["cust_id"]),
                                Convert.ToInt32(item["group_num"]),
                                Convert.ToInt32(item["pack_num"]),
                                item["cust_name"].ToString(),
                                item["cust_status"].ToString(),
                                Convert.ToByte(item["cust_fault"])
                                ));
                        }
                        else
                        {
                            CIM.CustId = Convert.ToInt32(item["cust_id"]);
                            CIM.GroupNum = Convert.ToInt32(item["group_num"]);
                            CIM.PackNum = Convert.ToInt32(item["pack_num"]);
                            CIM.CustName = item["cust_name"].ToString();
                            CIM.CustStatus = item["cust_status"].ToString();
                            CIM.CustFault = Convert.ToByte(item["cust_fault"]);

                            if (CIM.CustId == Properties.Settings.Default.DTUIndex)
                            {
                                //刷新当前状态栏数据                
                                FuncUpdateStatus(CIM);
                            }

                        }
                    }
                });
                //BLLCommon.SendToServer(CS_AskReply.ClientAskCust, Convert.ToByte(0xff), Convert.ToByte(0xff), Convert.ToByte(0xff), BitConverter.GetBytes(1));
            }
        }

        public void SocketEventHandler(object sender, EventArgs e)
        {
            SocketEvent evt = e as SocketEvent;
            if (evt == null)
            {
                return;
            }
            switch (evt.eventType)
            {
                case SocketEventType.StartEvent:

                    break;
                case SocketEventType.ConnectEvent:

                    break;
                case SocketEventType.ReceEvent:
                    byte remark = evt.remark;
                    if (remark == CS_AskReply.ServerAsk)
                    {
                        BLLCommon.SendToServer(CS_AskReply.ServerReply, 0xffff, 0xff, 0xff, System.Text.Encoding.ASCII.GetBytes("0n"));
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            BLLCommon.CloseWaitWindow(true);
                        });
                    }
                    #region 与服务器之间的通信，现在直接去掉！
                    /*if (remark == CS_AskReply.ClientReplyGroup)
                    {
                        if (modelIndex == 2 || modelIndex == 4)
                        {
                            if (FuncDoAction!=null)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(FuncDoAction, evt.message, remark);
                            }                            
                        }
                    }
                    else if (remark == CS_AskReply.ClientReplyPackStatus)
                    {
                        if (modelIndex == 2 || modelIndex == 4)
                        {
                            if (FuncDoAction != null)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(FuncDoAction, evt.message, remark);
                            } 
                        }
                    }
                    else if (remark == CS_AskReply.ClientReplyGroupStatus)
                    {
                        if (modelIndex == 1)
                        {
                            if (FuncDoAction != null)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(FuncDoAction, evt.message, remark);
                            } 
                        }
                    }
                    else if (remark == CS_AskReply.ClientReplyPack)
                    {
                        if (modelIndex == 3 || modelIndex == 4)
                        {
                            if (FuncDoAction != null)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(FuncDoAction, evt.message, remark);
                            } 
                        }
                    }                    
                    else if (remark == CS_AskReply.ClientReplyCust)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            DataTable dtCust = evt.message as DataTable;
                            foreach (DataRow item in dtCust.Rows)
                            {
                                CustInfoModel CIM = null;
                                for (int i = 0; i < CustListSource.Count; i++)
                                {
                                    if (CustListSource[i].CustId == Convert.ToInt32(item["cust_id"]))
                                    {
                                        CIM = CustListSource[i];
                                        break;
                                    }
                                }

                                if (CIM == null)
                                {
                                    CustListSource.Add(new CustInfoModel(
                                        Convert.ToInt32(item["cust_id"]),
                                        Convert.ToInt32(item["group_num"]),
                                        Convert.ToInt32(item["pack_num"]),
                                        item["cust_name"].ToString(),
                                        item["cust_status"].ToString(),
                                        Convert.ToByte(item["cust_fault"])
                                        ));
                                }
                                else
                                {
                                    CIM.CustId = Convert.ToInt32(item["cust_id"]);
                                    CIM.GroupNum = Convert.ToInt32(item["group_num"]);
                                    CIM.PackNum = Convert.ToInt32(item["pack_num"]);
                                    CIM.CustName = item["cust_name"].ToString();
                                    CIM.CustStatus = item["cust_status"].ToString();
                                    CIM.CustFault = Convert.ToByte(item["cust_fault"]);

                                    if (CIM.CustId == Properties.Settings.Default.DTUIndex)
                                    {
                                        //刷新当前状态栏数据                
                                        FuncUpdateStatus(CIM);
                                    }

                                }
                            }
                        });
                        
                    }*/
                    #endregion                    

                    break;
                case SocketEventType.SendEvent:

                    break;
                case SocketEventType.DisconnectEvent:
                    if (socketClient != null)
                    {
                        socketClient.Stop();
                        if (FuncRefershOffLine != null)
                        {
                            FuncRefershOffLine(true);
                        }
                        socketClient = null;
                        ExcuteTimer.Stop();
                        ExcuteTimerCust.Stop();
                    }
                    break;
                case SocketEventType.StopEvent:
                    break;
            }
        }

        Timer _timer = null;
        byte infoFlag = 0xA1;

        public void SendControlByte()
        {
            if (socketClient != null)
            {
                _timer = new Timer(_ => SendControl(), null, 0, 1000);
            }
            BLLCommon.ShowWaitWindow();
        }

        private void SendControl()
        {
            if (socketClient != null)
            {
                List<byte> sendControl = new List<byte> { 0x7E, 0x27, 0x00, 0x46, 0x45, 0x00, 0x04 };
                sendControl.Add(infoFlag);
                sendControl.Add(0x00);
                sendControl.Add(0x80);
                if (false)
                {
                    //sendControl.Add(0x10);
                }
                else
                {
                    sendControl.Add(0x1f);
                }
                int sumControl = 0;
                for (int i = 1; i < sendControl.Count; i++)
                {
                    sumControl += sendControl[i];
                }
                sendControl.Add(Convert.ToByte(sumControl & 0x00ff));
                sendControl.Add(0x0D);

                BLLCommon.SendToServer(CS_AskReply.ClientControl, Properties.Settings.Default.DTUIndex, 0xff, 0xff, sendControl.ToArray());
            }
            infoFlag++;
            if (infoFlag == 0xA4)
            {
                infoFlag = 0xA1;
                _timer.Dispose();
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(delegate()
                {
                    BLLCommon.CloseWaitWindow(false);  
                    DXMessageBox.Show("已下发紧急操作命令！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }));

            }
        }





        public ICommand OnViewLoadedCommand
        {
            get
            {
                if (onViewLoadedCommand == null)
                    onViewLoadedCommand = new DelegateCommand(OnViewLoaded);
                return onViewLoadedCommand;
            }
        }
        public ICommand NavigateCommand
        {
            get
            {
                if (navigateCommand == null)
                    navigateCommand = new DelegateCommand<string>(Navigate);
                return navigateCommand;
            }
        }

        public ICommand NavigateToMaster
        {
            get
            {
                if (navigateToMaster == null)
                    navigateToMaster = new DelegateCommand<string>(NavigateMasterDetail, target => target != null);
                return navigateToMaster;
            }
        }
        public void NavigateMasterDetail(string target)
        {
            NavigationService.Navigate("MasterDetailPage", target, this);
        }

        public ICommand NavigateToPack
        {
            get
            {
                if (navigateToPack == null)
                    navigateToPack = new DelegateCommand<string>(NavigateItemDetail, target => target != null);
                return navigateToPack;
            }
        }
        public void NavigateItemDetail(string target)
        {
            NavigationService.Navigate("ItemDetailPage", target, this);
        }
        public void Navigate(string target)
        {
            NavigationService.Navigate(target, null, this);
        }
        protected INavigationService NavigationService { get { return GetService<INavigationService>(); } }
        protected virtual void OnViewLoaded() { }

        private static Uri _baseUri = new Uri("pack://application:,,,");
        public static BitmapImage GetImage(string path)
        {
#if SILVERLIGHT
            return new BitmapImage(new Uri("../"  + path, UriKind.RelativeOrAbsolute));
#else
            return new BitmapImage(new Uri(_baseUri, path));
#endif
        }
    }
}
