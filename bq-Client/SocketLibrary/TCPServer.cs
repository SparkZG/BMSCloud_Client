using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.IO;
using System.Collections.Concurrent;

namespace SocketLibrary
{
    public class TCPServer : MySocket
    {
        private string ip;
        private int port;
        //负责监听的 套接字
        private Socket ServerSocket = null;
        //负责监听 客户端 连接请求的 线程
        private Thread ServerThread = null;

        /// <summary>
        /// 保存客户端连接的Socket和维持会话线程的集合
        /// </summary>
        private ConcurrentDictionary<Socket, Thread> dict = new ConcurrentDictionary<Socket, Thread>();
        public TCPServer(string ip, int port)
        {
            this.ip = ip;
            this.port = port;

        }

        public TCPServer()
        {
            //默认本地连接
            string hostName = Dns.GetHostName();
            this.ip = Dns.GetHostEntry(hostName).AddressList[2].ToString();
            this.port = 9555;
        }


        public override string Run()
        {
            if (ServerSocket == null)
            {
                //创建 服务端 负责监听的 套接字，参数（使用IP4寻址协议，使用流式连接，使用TCP协议传输数据）
                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(ip);
                //创建 包含 ip 和 port 的网络节点对象
                IPEndPoint localEP = new IPEndPoint(ipAddress, port);
                try
                {
                    //将 负责监听 的套接字 绑定到 唯一的IP和端口上
                    ServerSocket.Bind(localEP);
                }
                catch (Exception ex)
                {
                    return ex.ToString();
                    throw new Exception(ex.Message);
                }
                //设置监听队列的长度
                ServerSocket.Listen(10);
                //开启线程等待远程客户端进行连接
                if (ServerThread == null)
                {
                    //创建 负责监听的线程，并传入监听方法
                    ServerThread = new Thread(new ThreadStart(WatchConnecting));
                    //设置为后台线程
                    ServerThread.IsBackground = true;
                    ServerThread.Start();
                }
                //触发事件
                this.SendEvent(new SocketEvent(SocketEventType.StartEvent, this.ip, this.port, "", 0));
            }
            return null;
        }

        /// <summary>
        /// 侦听客户端远程连接
        /// </summary>
        private void WatchConnecting()
        {
            while (true)
            {
                try
                {
                    //开始监听 客户端 连接请求，注意：Accept方法，会阻断当前的线程！
                    Socket client = ServerSocket.Accept();//一旦监听到客户端的请求，就返回一个负责和该客户端通信的套接字 client

                    //创建 通信线程
                    Thread session = new Thread(ReceiveMsg);
                    session.IsBackground = true;
                    //启动线程 并为线程要调用的方法ReceiveMsg 传入参数client
                    session.Start(client);


                    //触发事件
                    //client.RemoteEndPoint 中保存的是 当前连接客户端的 Ip和端口
                    string[] arr = client.RemoteEndPoint.ToString().Split(new char[] { ':' });
                    this.SendEvent(new SocketEvent(SocketEventType.ConnectEvent, this.ip, this.port, arr[0], int.Parse(arr[1])));
                    //保存远程客户端和对应会话的线程
                    //将线程 保存在 字典里，方便以后做“踢人”功能的时候用
                    dict.TryAdd(client, session);
                }
                catch (Exception)
                {
                    break;
                }

            }
        }

        /// <summary>
        /// 接收客户端数据
        /// </summary>
        /// <param name="value">与客户端建立会话连接的Socket</param>
        private void ReceiveMsg(Object obj)
        {
            Socket client = obj as Socket;
            string[] arr = client.RemoteEndPoint.ToString().Split(new char[] { ':' });
            Transfer tf = new Transfer();
            while (true)
            {
                //额度是1k
                byte[] buffer = new byte[1024];
                //将接收到的数据 存入 buffer 数组,并返回 真正接收到的数据 的长度
                int length = -1;
                bool closed = false;
                try
                {
                    //if (client.Available <= 0) continue; 
                    length = client.Receive(buffer, buffer.Length, 0);
                    //连接异常则终止连接
                    if (length <= 0)
                    {
                        closed = true;
                        Thread session = dict[client];
                        session.Abort();
                        break;
                    }
                    else
                    {
                        tf.MakeUpByte(buffer,arr);
                        if (tf.ReceiveEvent!=null)
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
                        if (client != null)
                        {
                            //触发终止事件，并删除 被中断连接的 通信套接字对象                           
                            this.SendEvent(new SocketEvent(SocketEventType.DisconnectEvent, this.ip, this.port, arr[0], int.Parse(arr[1])));
                            client.Close();
                            Thread thread;
                            dict.TryRemove(client, out thread);
                        }
                    }
                }
            }


        }



        public override string Send(byte[] buffer)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        public override string Send(string destination, byte[] buffer)
        {
            //通过key，找到 字典集合中对应的 与某个客户端通信的 套接字的 send方法，发送数据给对方
            Socket client = GetClientSocket(destination);            
            if (client == null)
            {
                return "未建立此连接";
            }
            try
            {
                int length = client.Send(buffer);
                if (length > 0)
                {
                    //发送成功
                    return null;
                }
                else
                    return "连接中断";
            }
            catch (Exception ex)
            {
                //触发终止事件，并删除 被中断连接的 通信套接字对象                
                if (client != null)
                {
                    string[] arr = client.RemoteEndPoint.ToString().Split(new char[] { ':' });
                    this.SendEvent(new SocketEvent(SocketEventType.DisconnectEvent, this.ip, this.port, arr[0], int.Parse(arr[1])));
                    client.Close();
                    Thread thread;
                    dict.TryRemove(client, out thread);
                }
                return ex.ToString();
            }
        }


        private Socket GetClientSocket(string address)
        {
            Socket client = null;

            foreach (Socket s in dict.Keys)
            {
                if (s.RemoteEndPoint.ToString() == address.Trim())
                {
                    client = s;
                    break;
                }
            }

            return client;
        }

        /// <summary>
        /// 终止服务
        /// </summary>
        public override void Stop()
        {
            if (ServerSocket != null)
            {               

                ServerSocket.Close();
                ServerSocket.Dispose();
                ServerSocket = null;
                if (ServerThread != null)
                {
                    ServerThread.Abort();
                }
                ServerThread = null;
                foreach (Socket s in dict.Keys)
                {
                    if (s.Connected)
                    {
                        s.Shutdown(SocketShutdown.Both);
                    }
                    s.Close();
                    s.Dispose();
                    dict[s].Abort();
                }
                dict.Clear();
                this.SendEvent(new SocketEvent(SocketEventType.StopEvent, this.ip, this.port, "", 0));
            }
        }
    }
}
