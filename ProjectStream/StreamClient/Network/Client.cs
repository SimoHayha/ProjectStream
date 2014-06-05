using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Un4seen.Bass;

namespace StreamClient.Network
{
    internal class StateObject
    {
        public Socket WorkSocket;
        public const int BufferSize = 256;
        public byte[] Buffer = new byte[BufferSize];
        public MemoryStream MS = new MemoryStream();
    }

    public class Client
    {
        private ManualResetEvent _ConnectDone;
        private ManualResetEvent _SendDone;
        private ManualResetEvent _ReceiveDone;
        
        public Client()
        {
            _ConnectDone = new ManualResetEvent(false);
            _SendDone = new ManualResetEvent(false);
            _ReceiveDone = new ManualResetEvent(false);
        }

        public void Start()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 1337);

                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                _ConnectDone.WaitOne();

                Receive(client);
                _ReceiveDone.WaitOne();

                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;

                client.EndConnect(ar);

                _ConnectDone.Set();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.WorkSocket = client;

                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.WorkSocket;

                int byteRead = client.EndReceive(ar);

                if (byteRead > 0)
                {
                    Console.WriteLine("Received " + byteRead);
                    state.MS.Write(state.Buffer, 0, byteRead);

                    client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    if (state.MS.Length > 1)
                    {
                        Console.WriteLine("Received all " + state.MS.Length);
                        Play(state.MS);
                    }
                    _ReceiveDone.Set();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        private void Play(MemoryStream ms)
        {
            ms.Position = 0;
            using (FileStream file = new FileStream("music.ogg", FileMode.Create, System.IO.FileAccess.Write))
            {
                byte[] bytes = new byte[ms.Length];
                ms.Read(bytes, 0, (int)ms.Length);
                file.Write(bytes, 0, bytes.Length);
                ms.Close();
            }

            if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
            {
                BassNet.Registration("delporte.valentin@gmail.com", "2X9231427152222");
                int stream = Bass.BASS_StreamCreateFile("music.ogg", 0, 0, BASSFlag.BASS_SAMPLE_LOOP);
                if (stream != 0)
                {
                    Bass.BASS_ChannelPlay(stream, false);
                }
                else
                {
                    Console.WriteLine("Stream error: {0}", Bass.BASS_ErrorGetCode());
                }

                float[] buffer = new float[1024];
                Bass.BASS_ChannelGetData(stream, buffer, 1024);

                Console.WriteLine("Press key to stop");
                Console.ReadKey(false);
                Bass.BASS_StreamFree(stream);
                Bass.BASS_Free();
            }
        }
    }
}
