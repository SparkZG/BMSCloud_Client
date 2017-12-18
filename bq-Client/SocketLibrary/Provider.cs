using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketLibrary
{
    public class FlagType
    {
        /// <summary>
        /// BMS上传的心跳
        /// </summary>
        public const byte BMSPlause = 0x7E;
        /// <summary>
        /// GPRS心跳包
        /// </summary>
        public const byte GPRSPlause = 0x2A;
        /// <summary>
        /// C-S之间通讯标识
        /// </summary>
        public const byte CS_AskReplay = 0x21;
        /// <summary>
        /// 接收多包标志
        /// </summary>
        public const byte CS_Multi = 0x23;
        /// <summary>
        /// 其他信息
        /// </summary>
        public const byte Other = 0xFF;
    }

    public enum CSDefineType
    {
        /// <summary>
        /// 头标识
        /// </summary>
        headSign,
        /// <summary>
        /// 帧模式
        /// </summary>
        modeText,
        /// <summary>
        /// 发送方向
        /// </summary>
        sendDirection,
        /// <summary>
        /// 客户
        /// </summary>
        cust,
        /// <summary>
        /// 电池组
        /// </summary>
        packGroup,
        /// <summary>
        /// pack
        /// </summary>
        pack,
        /// <summary>
        /// 多包帧包ID
        /// </summary>
        multiID,
        /// <summary>
        /// 帧所载内容字节数
        /// </summary>
        contentLength,
        /// <summary>
        /// 帧所载内容
        /// </summary>
        content,
        /// <summary>
        /// 校验和
        /// </summary>
        checkSum,
        /// <summary>
        /// 保留位1（有些帧会用保留位作为他用）
        /// </summary>
        reservedBit1,
        /// <summary>
        /// 保留位2（有些帧会用保留位作为他用）
        /// </summary>
        reservedBit2
    }
    public class CS_Define
    {
        public CS_Define(CSDefineType dn, int di, int db)
        {
            defineName = dn;
            defineIndex = di;
            defineByteNum = db;
        }
        /// <summary>
        /// CS协议define名称
        /// </summary>
        public CSDefineType defineName { get; set; }
        /// <summary>
        /// CS协议define索引
        /// </summary>
        public int defineIndex { get; set; }
        /// <summary>
        /// CS协议define字节数
        /// </summary>
        public int defineByteNum { get; set; }
    }
    public class CS_AskReply
    {
        /// <summary>
        /// 客户端询问服务器Pack信息
        /// </summary>
        public const byte ClientAskPack = 0x00;
        /// <summary>
        /// 服务端返回客户端Pack信息
        /// </summary>
        public const byte ClientReplyPack = 0x01;
        /// <summary>
        /// 客户端询问服务器PackGroup信息
        /// </summary>
        public const byte ClientAskGroup = 0x02;
        /// <summary>
        /// 服务端返回客户端PackGroup信息
        /// </summary>
        public const byte ClientReplyGroup = 0x03;
        /// <summary>
        /// 客户端请求控制
        /// </summary>
        public const byte ClientControl = 0x04;
        /// <summary>
        /// 客户端询问服务器客户信息
        /// </summary>
        public const byte ClientAskCust = 0x05;
        /// <summary>
        /// 服务端返回客户端客户信息
        /// </summary>
        public const byte ClientReplyCust = 0x06;
        /// <summary>
        /// 服务端询问客户端是否在线
        /// </summary>
        public const byte ServerAsk = 0x07;
        /// <summary>
        /// 服务端询问客户端是否在线
        /// </summary>
        public const byte ServerReply = 0x08;
        /// <summary>
        /// 请求发送多包
        /// </summary>
        public const byte Multi = 0x09;
        /// <summary>
        /// 接收到多包信息
        /// </summary>
        public const byte MultiReply = 0x0A;
        /// <summary>
        /// 询问Packs状态
        /// </summary>
        public const byte ClientAskPackStatus = 0x0B;
        /// <summary>
        /// 回复Packs状态
        /// </summary>
        public const byte ClientReplyPackStatus = 0x0C;
        /// <summary>
        /// 询问Groups状态
        /// </summary>
        public const byte ClientAskGroupStatus = 0x0D;
        /// <summary>
        /// 回复Groups状态
        /// </summary>
        public const byte ClientReplyGroupStatus = 0x0E;
        /// <summary>
        /// 错误
        /// </summary>
        public const byte Error = 0xFF;
    }

    public class SendDirection
    {
        /// <summary>
        /// 服务器发送给客户端
        /// </summary>
        public const byte S_C = 0x00;
        /// <summary>
        /// 客户端发送服务器
        /// </summary>
        public const byte C_S = 0x01;
    }

    public class BMSFlagType
    {
        /// <summary>
        /// BMS上传PackGroup信息
        /// </summary>
        public const byte PackGroup = 0xFF;
        /// <summary>
        /// BMS上传Pack信息
        /// </summary>
        public const byte Pack = 0x01;
    }

    public class ClientAskType
    {
        /// <summary>
        /// 客户端询问服务器实时Pack信息
        /// </summary>
        public const byte ClientAskRealTime = 0x00;
        /// <summary>
        /// 客户端询问服务器历史Pack信息
        /// </summary>
        public const byte ClientAskHistory = 0x01;
    }

    public class ClientSign
    {
        /// <summary>
        /// 在线标识
        /// </summary>
        public bool OnlineSign { get; set; }
        /// <summary>
        /// 多包收到回复标识
        /// </summary>
        public bool MultiSign { get; set; }
    }

    public class PackAddress
    {
        /// <summary>
        /// 客户ID
        /// </summary>
        public ushort cust_id { get; set; }
        /// <summary>
        /// 电池组ID
        /// </summary>
        public int group_id { get; set; }
        /// <summary>
        /// PackID
        /// </summary>
        public int pack_id { get; set; }
        /// <summary>
        /// 此消息的数据类型——CS_AskReply
        /// </summary>
        public byte dataType { get; set; }
        /// <summary>
        /// 要获取的数量
        /// </summary>
        public int count { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime statTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime endTime { get; set; }
    }

    public class CustInfo
    {
        /// <summary>
        /// 客户ID
        /// </summary>
        public ushort Cust_id { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string Cust_name { get; set; }
    }

    public class Provider
    {
        /// <summary>
        /// CS_Define链表
        /// </summary>
        static List<CS_Define> DefineList = new List<CS_Define> { };

        #region 声明CS定义
        /// <summary>
        /// 头标识
        /// </summary>
        public static CS_Define HS = new CS_Define(CSDefineType.headSign, 0, 1);
        /// <summary>
        /// 帧模式
        /// </summary>
        public static CS_Define MT = new CS_Define(CSDefineType.modeText, 1, 1);
        /// <summary>
        /// 发送方向
        /// </summary>
        public static CS_Define SD = new CS_Define(CSDefineType.sendDirection, 2, 1);
        /// <summary>
        /// 客户
        /// </summary>
        public static CS_Define CU = new CS_Define(CSDefineType.cust, 3, 2);
        /// <summary>
        /// 电池组
        /// </summary>
        public static CS_Define PC = new CS_Define(CSDefineType.packGroup, 5, 1);
        /// <summary>
        /// pack
        /// </summary>
        public static CS_Define PA = new CS_Define(CSDefineType.pack, 6, 1);
        /// <summary>
        /// 多包帧包ID
        /// </summary>
        public static CS_Define MI = new CS_Define(CSDefineType.multiID, 7, 2);
        /// <summary>
        /// 帧所载内容字节数
        /// </summary>
        public static CS_Define CL = new CS_Define(CSDefineType.contentLength, 9, 4);
        /// <summary>
        /// 帧所载内容
        /// </summary>
        public static CS_Define CT = new CS_Define(CSDefineType.content, 13, 1000);
        /// <summary>
        /// 校验和
        /// </summary>
        public static CS_Define CS = new CS_Define(CSDefineType.checkSum, 1013, 4);
        /// <summary>
        /// 保留位1（有些帧会用保留位作为他用）
        /// </summary>
        public static CS_Define RB1 = new CS_Define(CSDefineType.reservedBit1, 7, 1);
        /// <summary>
        /// 保留位2（有些帧会用保留位作为他用）
        /// </summary>
        public static CS_Define RB2 = new CS_Define(CSDefineType.reservedBit2, 8, 1);

        #endregion

        public static void InitailProvider()
        {
            DefineList.Add(HS);
            DefineList.Add(MT);
            DefineList.Add(SD);
            DefineList.Add(CU);
            DefineList.Add(PC);
            DefineList.Add(PA);
            DefineList.Add(MI);
            DefineList.Add(CL); 
            DefineList.Add(CT);
            DefineList.Add(CS);
            DefineList.Add(RB1);
            DefineList.Add(RB2);

        }

        /// <summary>
        /// 根据协议规定获取指定值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defineName"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static T GetValue<T>(CSDefineType defineName, params byte[] buffer)
        {
            T result = default(T);

            CS_Define desCD = DefineList.Find(cd => cd.defineName == defineName);
            if (desCD != null)
            {
                switch (desCD.defineByteNum)
                {
                    case 1:
                        byte value_byte = buffer[desCD.defineIndex];
                        if (value_byte is T)
                        {
                            result = (T)(object)value_byte; //或 (T)((object)model);
                        }
                        return result;
                    case 2:
                        ushort value_unshort = BitConverter.ToUInt16(buffer, desCD.defineIndex);
                        if (value_unshort is T)
                        {
                            result = (T)(object)value_unshort; //或 (T)((object)model);
                        }
                        return result;
                    case 4:
                        int value_int = BitConverter.ToInt32(buffer, desCD.defineIndex);
                        if (value_int is T)
                        {
                            result = (T)(object)value_int; //或 (T)((object)model);
                        }
                        return result;
                    default:
                        break;
                }
            }
            return result;

        }

        /// <summary>
        /// 字节流转换为时间
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static DateTime BytesToDateTime(byte[] bytes, int offset)
        {
            if (bytes != null)
            {
                long ticks = BitConverter.ToInt64(bytes, offset);
                if (ticks < DateTime.MaxValue.Ticks && ticks > DateTime.MinValue.Ticks)
                {
                    DateTime dt = new DateTime(ticks);
                    return dt;
                }
            }
            return new DateTime();
        }
        /// <summary>
        /// 时间转换成字节流
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static byte[] DateTimeToBytes(DateTime dt)
        {
            return BitConverter.GetBytes(dt.Ticks);
        }
    }

    public enum ServerType { UDP, TCPServer, TCPClient };

    /// <summary>
    /// Socket事件类型
    /// </summary>
    public enum SocketEventType
    {
        /// <summary>
        /// 服务器启动
        /// </summary>
        StartEvent,
        /// <summary>
        /// 服务器停止
        /// </summary>
        StopEvent,
        /// <summary>
        /// 服务器接受远程客户端连接
        /// </summary>
        ConnectEvent,
        /// <summary>
        /// 服务器断开远程客户端连接
        /// </summary>
        DisconnectEvent,
        /// <summary>
        /// 服务器发送数据
        /// </summary>
        SendEvent,
        /// <summary>
        /// 服务器接收数据
        /// </summary>
        ReceEvent,
    }
}
