using StreamServer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server s = new Server();
            s.Start();

            Console.Read();
        }
    }
}
