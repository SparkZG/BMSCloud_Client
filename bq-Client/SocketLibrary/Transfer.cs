using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Threading;

namespace SocketLibrary
{
    public class Transfer
    {
        public List<byte> multiByte = new List<byte> { };
        public DispatcherTimer timeoutTimer = new DispatcherTimer();
        /// <summary>
        /// 接收到的包ID
        /// </summary>
        private int receiveNum = 1;
        /// <summary>
        /// 包总长度
        /// </summary>
        private int totalPackNum = 0;
        public SocketEvent ReceiveEvent = null;
        private PackAddress packaddress = new PackAddress();
        public Transfer()
        {
            timeoutTimer.Interval = TimeSpan.FromSeconds(30);
            timeoutTimer.Tick += timeoutTimer_Tick;
        }
        private void timeoutTimer_Tick(object sender, EventArgs e)
        {
            ReceiveEvent = null;
            ClearSet();
        }
        private void ClearSet()
        {
            timeoutTimer.Stop();
            packaddress = new PackAddress();
            receiveNum = 1;
            totalPackNum = 0;
            multiByte = new List<byte> { };
        }
        public void MakeUpByte(byte[] buffer, string[] arr)
        {
            switch (Provider.GetValue<byte>(CSDefineType.headSign, buffer))
            {
                case FlagType.CS_AskReplay:
                    if (CheckSum(true, buffer))
                    {
                        if (Provider.GetValue<byte>(CSDefineType.modeText, buffer) == CS_AskReply.Multi &&
                            Provider.GetValue<byte>(CSDefineType.sendDirection, buffer) == SendDirection.S_C)
                        {
                            ClearSet();
                            packaddress.cust_id = Provider.GetValue<ushort>(CSDefineType.cust, buffer);
                            packaddress.group_id = Provider.GetValue<byte>(CSDefineType.packGroup, buffer);
                            packaddress.pack_id = Provider.GetValue<byte>(CSDefineType.pack, buffer);
                            packaddress.dataType = Provider.GetValue<byte>(CSDefineType.reservedBit1, buffer);
                            totalPackNum = BitConverter.ToInt32(buffer, Provider.CT.defineIndex);
                            timeoutTimer.Start();
                        }
                        else
                        {
                            ReceiveEvent = new SocketEvent(SocketEventType.ReceEvent, buffer, buffer.Length, arr[0], int.Parse(arr[1]));
                        }
                    }
                    break;
                case FlagType.CS_Multi:
                    if (CheckSum(true, buffer))
                    {
                        Thread.Sleep(1);
                        if (receiveNum == Provider.GetValue<ushort>(CSDefineType.multiID, buffer) &&
                             packaddress.cust_id == Provider.GetValue<ushort>(CSDefineType.cust, buffer) &&
                             packaddress.group_id == Provider.GetValue<byte>(CSDefineType.packGroup, buffer) &&
                            packaddress.pack_id == Provider.GetValue<byte>(CSDefineType.pack, buffer) &&
                            packaddress.dataType == Provider.GetValue<byte>(CSDefineType.modeText, buffer))
                        {
                            //此包的数据长度
                            int dataNum = Provider.GetValue<int>(CSDefineType.contentLength, buffer);
                            for (int i = 0; i < dataNum; i++)
                            {
                                multiByte.Add(buffer[i + Provider.CT.defineIndex]);
                            }
                            //if (receiveNum % 50 == 0)
                            //{
                            //    BLLCommon.SendToServer(CS_AskReply.MultiReply, bq_Client.Properties.Settings.Default.DTUIndex,
                            //        Convert.ToByte(packaddress.group_id), Convert.ToByte(packaddress.pack_id), BitConverter.GetBytes(receiveNum));
                            //}
                            receiveNum++;
                            if (receiveNum == totalPackNum + 1)
                            {
                                multiByte.Insert(Provider.HS.defineIndex, FlagType.CS_Multi);
                                multiByte.Insert(Provider.MT.defineIndex, packaddress.dataType);
                                multiByte.Insert(Provider.SD.defineIndex, SendDirection.S_C);
                                multiByte.InsertRange(Provider.CU.defineIndex, BitConverter.GetBytes(packaddress.cust_id));
                                multiByte.Insert(Provider.PC.defineIndex, Convert.ToByte(packaddress.group_id));
                                multiByte.Insert(Provider.PA.defineIndex, Convert.ToByte(packaddress.pack_id));
                                multiByte.InsertRange(Provider.RB1.defineIndex, BitConverter.GetBytes((ushort)0xffff));
                                multiByte.InsertRange(Provider.CL.defineIndex, BitConverter.GetBytes((uint)0xffffffff));
                                ReceiveEvent = new SocketEvent(SocketEventType.ReceEvent, multiByte.ToArray(), multiByte.Count, arr[0], int.Parse(arr[1]));
                                ClearSet();
                            }
                        }
                        else
                            ClearSet();
                    }
                    break;
                case FlagType.BMSPlause:
                    if (CheckSum(false, buffer))
                    {
                        ReceiveEvent = new SocketEvent(SocketEventType.ReceEvent, buffer, buffer.Length, arr[0], int.Parse(arr[1]));
                    }
                    break;
                default:
                    ReceiveEvent = new SocketEvent(SocketEventType.ReceEvent, buffer, buffer.Length, arr[0], int.Parse(arr[1]));
                    break;
            }
        }

        /// <summary>
        /// 校验和：checkSum=buffer[1]+buffer[2]+...+buffer[checksum之前一位]
        /// </summary>
        /// <param name="IsClient"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private bool CheckSum(bool IsClient, byte[] buffer)
        {
            //CS通信
            if (IsClient)
            {
                int reLength = Provider.GetValue<int>(CSDefineType.contentLength, buffer) + Provider.CT.defineIndex;
                Int32 sum = 0;
                for (int i = 1; i < reLength; i++)
                {
                    sum += buffer[i];
                }
                if (sum != BitConverter.ToInt32(buffer, reLength))
                {
                    return false;
                }
            }
            //BMS上传
            else
            {
                int reLength = (buffer[5] << 8) + buffer[6] + 7;
                int sum = 0;
                if (buffer[1]!=0x27)
                {
                    return false;
                }
                for (int i = 1; i < reLength; i++)
                {
                    sum += buffer[i];
                }
                int checkSum = sum & 0x0000FF;
                if (checkSum != buffer[reLength])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
