using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

namespace SocketLibrary
{
    public abstract class MySocket
    {
        public bool OffLine = true;

        public event EventHandler SocketEventHandler;

        public abstract string Run();

        public abstract string Send(byte[] buffer);

        public abstract string Send(string destination, byte[] buffer);

        public abstract void Stop();

        protected void SendEvent(EventArgs args)
        {
            //push到线程池
            //using (var cts = new CancellationTokenSource())
            //{
            //    CancellationToken token = cts.Token;

            //    ThreadPool.QueueUserWorkItem(_ => SendThread(args));
            //}

            //进入单线程单元，这样在此线程中可以调用UI控件元素
            Thread NetServer = new Thread(new ThreadStart(() => SendThread(args)));
            NetServer.SetApartmentState(ApartmentState.STA);
            NetServer.IsBackground = true;
            NetServer.Start();
        }
        private void SendThread(EventArgs args)
        {
            if (SocketEventHandler != null)
            {
                SocketEventHandler(this, args);
            }
        }
    }
}
