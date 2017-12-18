using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace SocketLibrary
{
    public class TCPClient : MySocket
    {
        private string ip;
        private int port;
        //客户端套接字
        private Socket ClientSocket = null;
        //客户端 负责 接收 服务端发来的数据消息的线程
        private Thread ClientThread = null;
        public TCPClient(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public TCPClient()
        {
            string hostName = Dns.GetHostName();
            this.ip = Dns.GetHostEntry(hostName).AddressList[1].ToString();
            this.port = 8080;
        }

        /// <summary>
        /// 客户端发送连接请求到服务器
        /// </summary>
        public override string Run()
        {
            if (ClientSocket == null)
            {
                //创建 客户端 的 套接字，参数（使用IP4寻址协议，使用流式连接，使用TCP协议传输数据）
                ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                try
                {
                    //向 指定的IP和端口 发送连接请求
                    ClientSocket.Connect(remoteEP);
                    if (ClientThread == null)
                    {
                        //客户端 创建线程 监听服务端 发来的消息
                        ClientThread = new Thread(new ThreadStart(ReceiveMsg));
                        ClientThread.IsBackground = true;
                        ClientThread.Start();
                    }

                    //触发连接成功事件
                    string[] arr1 = ClientSocket.LocalEndPoint.ToString().Split(new char[] { ':' });
                    string[] arr2 = ClientSocket.RemoteEndPoint.ToString().Split(new char[] { ':' });
                    this.SendEvent(new SocketEvent(SocketEventType.ConnectEvent, arr1[0], int.Parse(arr1[1]), arr2[0], int.Parse(arr2[1])));
                    OffLine = false;
                }
                catch (Exception ex)
                {
                    if (ClientSocket.Connected)
                    {
                        //连接成功之后发生异常则终止
                        ClientSocket.Shutdown(SocketShutdown.Both);
                        ClientSocket.Close();
                        throw new Exception(ex.Message);
                    }
                    else
                    {
                        return ex.ToString();
                    }

                }
            }
            return null;
        }


        /// <summary>
        /// 监听服务端 发来的消息
        /// </summary>
        private void ReceiveMsg()
        {
            Transfer tf = new Transfer();
            while (true)
            {
                //定义一个 接收用的 缓存区(1K字节数组)
                byte[] buffer = new byte[1024];
                int length = -1;
                bool closed = false;
                try
                {
                    //为防止在调用close后socket继续向网络缓存区读取数据而引发异常
                    //if (ClientSocket.Available <= 0) continue; 
                    //将接收到的数据 存入 buffer 数组,并返回 真正接收到的数据 的长度
                    length = ClientSocket.Receive(buffer);
                    if (length <= 0)
                    {
                        //连接异常则终止连接
                        closed = true;
                        break;
                    }
                    else
                    {
                        string[] arr = ClientSocket.RemoteEndPoint.ToString().Split(new char[] { ':' });

                        tf.MakeUpByte(buffer, arr);
                        if (tf.ReceiveEvent != null)
                        {
                            //触发事件，将收到的数据（图片）回显到richbox中
                            this.SendEvent(tf.ReceiveEvent);
                            tf.ReceiveEvent = null;
                        } 
                    }
                }
                catch (Exception)
                {
                    break;
                }
                finally
                {
                    if (closed)
                    {
                        //触发终止事件
                        string[] arr = ClientSocket.RemoteEndPoint.ToString().Split(new char[] { ':' });
                        this.SendEvent(new SocketEvent(SocketEventType.DisconnectEvent, this.ip, this.port, arr[0], int.Parse(arr[1])));
                    }
                }
            }
        }


        /// <summary>
        /// 发送数据
        /// </summary>
        public override string Send(byte[] buffer)
        {
            if (ClientSocket != null)
            {
                try
                {
                    int count = ClientSocket.Send(buffer, buffer.Length, SocketFlags.None);
                    if (count > 0)
                    {
                        //发送数据成功触发事件
                        return null;
                    }
                    else
                        return "连接中断";
                }
                catch (Exception ex)
                {
                    //触发终止事件
                    string[] arr = ClientSocket.RemoteEndPoint.ToString().Split(new char[] { ':' });
                    this.SendEvent(new SocketEvent(SocketEventType.DisconnectEvent, this.ip, this.port, arr[0], int.Parse(arr[1])));
                    return ex.ToString();
                }
            }
            else
                return "未建立连接";
        }

        public override string Send(string destination, byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            if (ClientSocket != null)
            {
                if (ClientSocket.Connected)
                {
                    ClientSocket.Shutdown(SocketShutdown.Both);
                }
                ClientSocket.Close();
                ClientSocket.Dispose();
                if (ClientThread != null)
                {
                    try
                    {
                        ClientThread.Abort();
                    }
                    catch
                    {
                        //此处有问题：正在终止线程
                    }
                }
                ClientSocket = null;
                ClientThread = null;
                OffLine = true;
            }
        }
    }
}
