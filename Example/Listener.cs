using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using NNanomsg;
using NNanomsg.Protocols;

namespace Example
{
    class Listener
    {
        public static void Execute(string[] args)
        {
            const string socketAddress = "tcp://127.0.0.1:5088";

            if (args[1] == "client")
            {
                using (var req = new RequestSocket())
                {
                    req.Connect(socketAddress);
                    req.Send(Encoding.UTF8.GetBytes("hello from client"));
                    Console.WriteLine("Message from SERVER: " + Encoding.UTF8.GetString(req.Receive()));

                    Console.WriteLine("CLIENT finished");
                }
            }

            else if (args[1] == "server")
            {
                using (var rep = new ReplySocket())
                {
                    rep.Bind(socketAddress);

                    var listener = new NanomsgListener();
                    listener.AddSocket(rep);
                    listener.ReceivedMessage += socketId =>
                        {
                            Console.WriteLine("Message from CLIENT: " + Encoding.UTF8.GetString(rep.Receive()));
                            rep.Send(Encoding.UTF8.GetBytes("hello from server"));
                            Console.WriteLine("SERVER Finished");
                            Environment.Exit(0);
                        };

                    listener.Listen(null);
                }
            }
            else
            {
                Console.WriteLine("Unknown argument: " + args[1]);
            }
        }
    }
}
