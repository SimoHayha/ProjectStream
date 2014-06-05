using Assets.Scripts.BassWrapper;
//using Assets.Scripts.Debug;
using Assets.Scripts.Threads;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.NetworkWrapper
{
    public class StartMusic : ThreadedJob
    {
        private string _Title;

        public StartMusic(string title)
        {
            _Title = title;
        }

        protected override void ThreadFunction()
        {
            if (File.Exists(_Title))
                return;

            byte[] bytes = new byte[8192];
            int bytesReceived;
            MemoryStream ms = new MemoryStream();

            try
            {
                IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 1337);

                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    sender.Connect(remoteEP);
                    //StaticLogger.Log("Connected to " + sender.RemoteEndPoint.ToString());

                    //StaticLogger.Log("Start to receive");
                    bytesReceived = sender.Receive(bytes);
                    while (bytesReceived > 0)
                    {
                        //StaticLogger.Log("Receive " + bytesReceived);
                        ms.Write(bytes, 0, bytesReceived);
                        bytesReceived = sender.Receive(bytes);
                    }
                    //StaticLogger.Log("Received all");

                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                    _Title = "stream.ogg";

                    ms.Position = 0;
                    using (FileStream file = new FileStream(_Title, FileMode.Create, System.IO.FileAccess.Write))
                    {
                        bytes = new byte[ms.Length];
                        ms.Read(bytes, 0, (int)ms.Length);
                        file.Write(bytes, 0, bytes.Length);
                        ms.Close();
                    }
                }
                catch (ArgumentNullException ane)
                {
                    //StaticLogger.Log(ane.ToString());
                }
                catch (SocketException se)
                {
                    //StaticLogger.Log(se.ToString());
                }
                catch (Exception e)
                {
                    //StaticLogger.Log(e.ToString());
                }
            }
            catch (Exception e)
            {
                //StaticLogger.Log(e.ToString());
            }
        }

        protected override void OnFinished()
        {
            try
            {
                BassManager bass = GameObject.FindGameObjectWithTag("BassManager").GetComponent<BassManager>();

                bass.StartMusic(_Title);
            }
            catch (NullReferenceException) { }
        }
    }
}
