using StreamServer.Network.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StreamServer.Network
{
    internal class StateObject
    {
        public Socket WorkSocket = null;
        public const int BufferSize = 8192;
        public byte[] Buffer = new byte[BufferSize];
    }

    public class Server
    {
        private Socket _Listener;
        private bool _Alive;
        private ManualResetEvent _AllDone;

        public Server()
        {
            _AllDone = new ManualResetEvent(false);
        }

        public void Start()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 1337);
            _Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _Listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _Listener.Bind(localEndPoint);
            _Listener.Listen(10);

            _Alive = true;
            ThreadPool.QueueUserWorkItem(new WaitCallback(AcceptConnection));
        }

        private void AcceptConnection(object obj)
        {
            Console.WriteLine("Ready to accept new connection");

            while (_Alive)
            {
                try
                {
                    _AllDone.Reset();

                    Console.WriteLine("Waiting for a connection ...");
                    _Listener.BeginAccept(new AsyncCallback(AcceptCallback), _Listener);
                    _AllDone.WaitOne();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            _AllDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            if (listener == null)
                return;
            Socket handler = null;
            try
            {
                handler = listener.EndAccept(ar);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            if (handler == null)
                return;

            FileStream fs = File.Open("lol.ogg", FileMode.Open);
            NetworkStream ns = new NetworkStream(handler);
            SendTask sendTask = new SendTask(fs, ns);
            sendTask.Start();
            sendTask.Completed += delegate
            {
                fs.Close();
                ns.Close();
                handler.Close();
            };

            //StateObject state = new StateObject();
            //state.WorkSocket = handler;
            //handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.WorkSocket;
            int byteRead = 0;

            try
            {
                byteRead = handler.EndReceive(ar);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            if (byteRead > 0)
            {
                string msg = Encoding.ASCII.GetString(state.Buffer);
                System.Diagnostics.Debug.WriteLine(msg);

                try
                {
                    handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }
        }
    }
}
