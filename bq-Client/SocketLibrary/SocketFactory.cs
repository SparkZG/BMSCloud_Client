using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketLibrary
{
    public class SocketFactory
    {
        public SocketFactory() { }

        public MySocket CreateSocket(ServerType socketType, string ip, int port)
        {
            switch (socketType)
            {
                case ServerType.TCPClient:
                    return new TCPClient(ip, port);
                case ServerType.TCPServer:
                    return new TCPServer(ip, port);
                case ServerType.UDP:
                    return null;
                default:
                    return null;
            }
        }
        public MySocket CreateSocket(ServerType socketType)
        {
            switch (socketType)
            {
                case ServerType.TCPClient:
                    return new TCPClient();
                case ServerType.TCPServer:
                    return new TCPServer();
                case ServerType.UDP:
                    return null;
                default:
                    return null;
            }
        }


    }
}
