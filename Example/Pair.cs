using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NNanomsg;
using NNanomsg.Protocols;

namespace Example
{
    public class Pair
    {
        static void SendReceive(PairSocket s)
        {
            NanomsgSocketOptions.SetTimespan(s.SocketID, SocketOptionLevel.Default, SocketOption.RCVTIMEO, TimeSpan.FromMilliseconds(100));
            while (true)
            {
                var data = s.Receive();
                if (data != null)
                {
                    Console.WriteLine("RECEIVED: '" + Encoding.UTF8.GetString(data) + "'");
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
                s.Send(Encoding.UTF8.GetBytes("the message is " + DateTime.Now.ToLongTimeString()));
            }
        }

        static void Node0(string url)
        {
            using (var s = new PairSocket())
            {
                s.Bind(url);
                SendReceive(s);
            }
        }

        static void Node1(string url)
        {
            using (var s = new PairSocket())
            {
                s.Connect(url);
                SendReceive(s);
            }
        }

        public static void Execute(string[] args)
        {
            switch (args[1])
            {
                case "node0": Node0(args[2]);
                    break;
                case "node1": Node1(args[2]);
                    break;
                default:
                    Console.WriteLine("Usage: ...");
                    break;
            }
        }
    }
}
