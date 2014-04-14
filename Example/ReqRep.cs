using System;
using System.Collections.Generic;
using System.Text;
using NNanomsg;
using NNanomsg.Protocols;

namespace Example
{
    public class ReqRep
    {
        static void Node0(string url)
        {
            using (var s = new ReplySocket())
            {
                s.Bind(url);
                Console.WriteLine("NODE0: RECEIVED: \"" + Encoding.UTF8.GetString(s.Receive()) + "\"");
                const string sendText = "Goodbye.";
                Console.WriteLine("NODE0: SENDING: \"" + sendText + "\"");
                s.Send(Encoding.UTF8.GetBytes(sendText));
            }
        }

        static void Node1(string url)
        {
            using (var s = new RequestSocket())
            {
                s.Connect(url);
                const string sendText = "Hello, World!";
                Console.WriteLine("NODE1: SENDING \"" + sendText + "\"");
                s.Send(Encoding.UTF8.GetBytes(sendText));
                Console.WriteLine("NODE1: RECEIVED: \"" + Encoding.UTF8.GetString(s.Receive()) + "\"");
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
