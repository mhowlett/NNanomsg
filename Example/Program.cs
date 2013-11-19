using System;
using NNanomsg;
using System.IO;
using NNanomsg.Protocols;

namespace Example
{
    class Program
    {
        /// <summary>
        ///     A simple example showing use of NNanomsg.Socket
        ///     For further example usage, check out the Test project.
        /// </summary>
        static void Main(string[] args)
        {
            const string socketAddress = "tcp://127.0.0.1:5088";

            if (args[0] == "client")
            {
                using (var req = new RequestSocket())
                {
                    req.Connect(socketAddress);
                    req.Send(new StringMessage("hello from client").GetBytes());
                    using (var buf = req.ReceiveStream())
                        Console.WriteLine("Message from SERVER: " + new StringMessage(new StreamReader(buf).ReadToEnd()).GetString());
                    Console.WriteLine("CLIENT finished");
                }
            }
            else if (args[0] == "server")
            {
                using (var rep = new ReplySocket())
                {
                    rep.Bind(socketAddress);
                    using (var buf = rep.ReceiveStream())
                        Console.WriteLine("Message from CLIENT: " + new StringMessage(new StreamReader(buf).ReadToEnd()).GetString());
                    rep.Send(new StringMessage("hello from server").GetBytes());
                    Console.WriteLine("SERVER Finished");
                }
            }
            else
            {
                Console.WriteLine("Unknown argument: " + args[0]);
            }
        }
    }
}
