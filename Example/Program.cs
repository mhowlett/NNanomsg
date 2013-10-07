using System;
using NNanomsg;

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
                using (var req = new Socket(Domain.SP, Protocol.REQ))
                {
                    req.Connect(socketAddress);
                    req.Send(new StringMessage("hello from client").GetBytes(), 0);
                    var buf = req.Recv(0);
                    Console.WriteLine("Message from SERVER: " + new StringMessage(buf).GetString());
                    Console.WriteLine("CLIENT finished");
                }
            }
            else if (args[0] == "server")
            {
                using (var rep = new Socket(Domain.SP, Protocol.REP))
                {
                    rep.Bind(socketAddress);
                    var buf = rep.Recv(0);
                    Console.WriteLine("Message from CLIENT: " + new StringMessage(buf).GetString());
                    rep.Send(new StringMessage("hello from server").GetBytes(), 0);
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
