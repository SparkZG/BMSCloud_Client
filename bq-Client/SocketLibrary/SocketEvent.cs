using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Drawing;
using System.Data;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace SocketLibrary
{
    public class SocketEvent : EventArgs
    {
        /// <summary>
        /// 通信的实体
        /// </summary>
        public object message { get; set; }
        /// <summary>
        /// 通信的类型
        /// </summary>
        public byte flagtype { get; set; }
        /// <summary>
        /// 通信标识，若为0xff代表为“Other”信息！
        /// </summary>
        public byte remark { get; set; }
        /// <summary>
        /// 事件类型
        /// </summary>
        public SocketEventType eventType { get; set; }
        /// <summary>
        /// 通信的Pack信息
        /// </summary>
        public PackAddress packaddress = new PackAddress();
        /// <summary>
        /// 通信的客户信息
        /// </summary>
        public CustInfo custInfo = new CustInfo();
        /// <summary>
        /// 本地打开Ip
        /// </summary>
        public string localIp { get; set; }
        /// <summary>
        /// 本地打开的Port
        /// </summary>
        public int localPort { get; set; }

        /// <summary>
        /// 远程连接的Ip
        /// </summary>
        public string remoteIp { get; set; }
        /// <summary>
        /// 远程连接的Port
        /// </summary>
        public int remotePort { get; set; }
        /// <summary>
        /// 处理BMS上传的心跳包
        /// </summary>
        /// <param name="buffer"></param>
        private void HandleBMSPlause(params byte[] buffer)
        {
            //BMS上传的心跳包格式中 5 6 两位代表内容长度，加上9代表长度之前还有9个字节
            for (int j = 0; j < (buffer[5] << 8) + buffer[6] + 9; j++)
            {
                message += string.Format("{0:X2} ", buffer[j]);
            }
            //buffer[8]为0Xff代表填充保留，说明是PackGroup信息
            if (buffer[8] == BMSFlagType.PackGroup)
            {
                packaddress.group_id = buffer[2];
                remark = BMSFlagType.PackGroup;
            }
            else
            {
                packaddress.group_id = buffer[2];
                packaddress.pack_id = buffer[8];
                remark = BMSFlagType.Pack;
            }
        }
        /// <summary>
        /// 服务器与客户端之间的通信，包括ask，reply，control
        /// </summary>
        /// <param name="buffer"></param>
        private void HandleAskReplay(params byte[] buffer)
        {
            //客户ID
            packaddress.cust_id = Provider.GetValue<ushort>(CSDefineType.cust, buffer);
            //电池组ID
            packaddress.group_id = Provider.GetValue<byte>(CSDefineType.packGroup, buffer);
            //packID
            packaddress.pack_id = Provider.GetValue<byte>(CSDefineType.pack, buffer);
            //模式
            byte modeText = Provider.GetValue<byte>(CSDefineType.modeText, buffer);
            //方向
            byte sendDirection = Provider.GetValue<byte>(CSDefineType.sendDirection, buffer);


            if (modeText == CS_AskReply.ClientAskPack && sendDirection == SendDirection.C_S)
            {
                packaddress.count = BitConverter.ToInt32(buffer, Provider.CT.defineIndex);
                if (Provider.GetValue<byte>(CSDefineType.reservedBit1, buffer) == ClientAskType.ClientAskRealTime)
                {
                    remark = CS_AskReply.ClientAskPack;
                    message = ClientAskType.ClientAskRealTime;
                }
                else
                {
                    //开始时间
                    packaddress.statTime = Provider.BytesToDateTime(buffer, Provider.CT.defineIndex + 4);
                    //结束时间
                    packaddress.endTime = Provider.BytesToDateTime(buffer, Provider.CT.defineIndex + 12);
                    remark = CS_AskReply.ClientAskPack;
                    message = ClientAskType.ClientAskHistory;
                }
            }
            else if (modeText == CS_AskReply.ClientReplyPack && sendDirection == SendDirection.S_C)
            {
                remark = CS_AskReply.ClientReplyPack;
                GetDataTable(buffer);
            }
            else if (modeText == CS_AskReply.ClientAskGroup && sendDirection == SendDirection.C_S)
            {
                packaddress.count = BitConverter.ToInt32(buffer, Provider.CT.defineIndex);
                if (Provider.GetValue<byte>(CSDefineType.reservedBit1, buffer) == ClientAskType.ClientAskRealTime)
                {
                    remark = CS_AskReply.ClientAskGroup;
                    message = ClientAskType.ClientAskRealTime;
                }
                else
                {
                    //开始时间
                    packaddress.statTime = Provider.BytesToDateTime(buffer, Provider.CT.defineIndex + 4);
                    //结束时间
                    packaddress.endTime = Provider.BytesToDateTime(buffer, Provider.CT.defineIndex + 12);
                    remark = CS_AskReply.ClientAskGroup;
                    message = ClientAskType.ClientAskHistory;
                }
            }
            else if (modeText == CS_AskReply.ClientReplyGroup && sendDirection == SendDirection.S_C)
            {
                remark = CS_AskReply.ClientReplyGroup;
                GetDataTable(buffer);
            }
            //模式为Control
            else if (modeText == CS_AskReply.ClientControl)
            {
                message = GetS_CText(buffer, Provider.GetValue<int>(CSDefineType.contentLength, buffer), Provider.CT.defineIndex); ;
                remark = CS_AskReply.ClientControl;
            }
            else if (modeText == CS_AskReply.ClientAskCust && sendDirection == SendDirection.C_S)
            {
                remark = CS_AskReply.ClientAskCust;
                message = remark;
            }
            else if (modeText == CS_AskReply.ClientReplyCust && sendDirection == SendDirection.S_C)
            {
                remark = CS_AskReply.ClientReplyCust;
                GetDataTable(buffer);
            }
            else if (modeText == CS_AskReply.ServerAsk && sendDirection == SendDirection.S_C)
            {
                //服务器发送给客户端是否在线
                byte[] getByte = GetS_CText(buffer, Provider.GetValue<int>(CSDefineType.contentLength, buffer), Provider.CT.defineIndex);
                message = System.Text.Encoding.ASCII.GetString(getByte, 0, getByte.Length);
                remark = CS_AskReply.ServerAsk;
            }
            else if (modeText == CS_AskReply.ServerReply && sendDirection == SendDirection.C_S)
            {
                message = "On";
                remark = CS_AskReply.ServerReply;
            }
            else if (modeText == CS_AskReply.MultiReply && sendDirection == SendDirection.C_S)
            {
                remark = CS_AskReply.MultiReply;
                message = remark;
            }
            else if (modeText == CS_AskReply.ClientAskPackStatus && sendDirection == SendDirection.C_S)
            {
                remark = CS_AskReply.ClientAskPackStatus;
                message = remark;
            }
            else if (modeText == CS_AskReply.ClientReplyPackStatus && sendDirection == SendDirection.S_C)
            {
                remark = CS_AskReply.ClientReplyPackStatus;
                GetDataTable(buffer);
            }
            else if (modeText == CS_AskReply.ClientAskGroupStatus && sendDirection == SendDirection.C_S)
            {
                remark = CS_AskReply.ClientAskGroupStatus;
                message = remark;
            }
            else if (modeText == CS_AskReply.ClientReplyGroupStatus && sendDirection == SendDirection.S_C)
            {
                remark = CS_AskReply.ClientReplyGroupStatus;
                GetDataTable(buffer);
            }
            else
            {
                remark = CS_AskReply.Error;
                message = remark;
            }
        }
        /// <summary>
        /// 反序列化Datatable
        /// </summary>
        /// <param name="buffer"></param>
        private void GetDataTable(params byte[] buffer)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(GetS_CText(buffer, Provider.GetValue<int>(CSDefineType.contentLength, buffer), Provider.CT.defineIndex));

            DataTable dt = bf.Deserialize(ms) as DataTable;
            message = dt;
        }
        /// <summary>
        /// 将整合的多包信息反序列化
        /// </summary>
        /// <param name="buffer"></param>
        public void HandleMulti(params byte[] buffer)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(GetS_CText(buffer, buffer.LongLength - Provider.CT.defineIndex, Provider.CT.defineIndex));

            DataTable dt = bf.Deserialize(ms) as DataTable;
            message = dt;
            remark = Provider.GetValue<byte>(CSDefineType.modeText, buffer);
        }
        /// <summary>
        /// 处理GPRS心跳包
        /// </summary>
        /// <param name="buffer"></param>
        public void HandleGPRSPlause(params byte[] buffer)
        {
            string recvStr = Encoding.ASCII.GetString(buffer);
            string[] strHeadArray = recvStr.Split('|');
            if (strHeadArray.Length < 3)
            {
                remark = 0xff;
                message = System.Text.Encoding.ASCII.GetString(buffer, 0, buffer.Length);
            }
            else
            {
                custInfo.Cust_id = Convert.ToUInt16(strHeadArray[1]);
                custInfo.Cust_name = strHeadArray[2].Substring(0, strHeadArray[2].IndexOf("\0"));
                message = remark + custInfo.Cust_id;
            }
        }
        public SocketEvent(SocketEventType eventType, byte[] buffer, int len, string ip, int port)
        {
            //此时 是将 数组 所有的元素 都转成字符串，而真正接收到的 只有服务端发来的几个字符
            switch (Provider.GetValue<byte>(CSDefineType.headSign, buffer))
            {
                case FlagType.BMSPlause:
                    flagtype = FlagType.BMSPlause;
                    HandleBMSPlause(buffer);
                    break;
                case FlagType.CS_AskReplay:
                    flagtype = FlagType.CS_AskReplay;
                    HandleAskReplay(buffer);
                    break;
                case FlagType.CS_Multi:
                    flagtype = FlagType.CS_Multi;
                    HandleMulti(buffer);
                    break;
                case FlagType.GPRSPlause:
                    flagtype = FlagType.GPRSPlause;
                    HandleGPRSPlause(buffer);
                    break;
                default:
                    flagtype = FlagType.Other;
                    message = System.Text.Encoding.ASCII.GetString(buffer, 0, len);
                    break;
            }

            this.eventType = eventType;
            this.remoteIp = ip;
            this.remotePort = port;
        }
        /// <summary>
        /// 根据字节长度以及开始位置得到发送的真实有用字节
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <param name="startLocation"></param>
        /// <returns></returns>
        private byte[] GetS_CText(byte[] buffer, long length, int startLocation)
        {
            byte[] getByte = new byte[length];
            for (int i = 0; i < length; i++)
            {
                getByte[i] = buffer[i + startLocation];
            }
            return getByte;
        }
        public SocketEvent(SocketEventType eventType, string localIp, int localPort, string remoteIp, int remotePort)
        {
            this.eventType = eventType;
            this.localIp = localIp;
            this.localPort = localPort;
            this.remoteIp = remoteIp;
            this.remotePort = remotePort;
        }
    }
}
