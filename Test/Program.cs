using System;
using NNanomsg;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            const string socketAddress = "tcp://127.0.0.1:5088";
            var buf = new byte[32];

            if (args[0] == "client")
            {
                int req = API.Socket(Domain.SP, Protocol.REQ);
                int rc = API.Connect(req, socketAddress);
                if (rc == -1)
                {
                    Console.WriteLine("There was an error connecting to '" + socketAddress + "': " + API.StrError(API.Errno()));
                }
                API.Send(req, new StringMessage("hello from client").GetBytes(), 0);
                API.Recv(req, buf, 0);
                Console.WriteLine("Message from SERVER: " + new StringMessage(buf).GetString());
                Console.WriteLine("CLIENT finished");
                API.Close(req);
            }
            else if (args[0] == "server")
            {
                int rep = API.Socket(Domain.SP, Protocol.REP);
                int rc = API.Bind(rep, socketAddress);
                if (rc == -1)
                {
                    Console.WriteLine("There was an error binding to '" + socketAddress + "': " + API.StrError(API.Errno()));
                }
                API.Recv(rep, buf, 0);
                Console.WriteLine("Message from CLIENT: " + new StringMessage(buf).GetString());
                API.Send(rep, new StringMessage("hello from server").GetBytes(), 0);
                Console.WriteLine("SERVER Finished");
                API.Close(rep);
            }
            else
            {
                Console.WriteLine("Unknown argument: " + args[0]);
            }
        }
    }
}
