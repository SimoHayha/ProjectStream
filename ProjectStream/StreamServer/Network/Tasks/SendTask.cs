using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamServer.Network.Tasks
{
    public class SendTask
    {
        public event EventHandler Completed = delegate { };

        private readonly Stream _Input;
        private readonly Stream _Output;

        private byte[] _Buffer = new byte[8192];

        public SendTask(Stream input, Stream output)
        {
            _Input = input;
            _Output = output;
        }

        public void Start()
        {
            GetNextChunk();
            Console.WriteLine("Start to send");
        }

        private void GetNextChunk()
        {
            _Input.BeginRead(_Buffer, 0, _Buffer.Length, InputReadComplete, null);
        }

        private void InputReadComplete(IAsyncResult ar)
        {
            int byteRead = _Input.EndRead(ar);

            if (byteRead == 0)
            {
                RaiseCompleted();
                return;
            }

            _Output.Write(_Buffer, 0, byteRead);
            Console.WriteLine("Send " + byteRead);

            GetNextChunk();
        }

        private void RaiseCompleted()
        {
            Console.WriteLine("Send complete");
            Completed(this, EventArgs.Empty);
        }
    }
}
