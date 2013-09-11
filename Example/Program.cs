using System;
using NNanomsg;

namespace Example
{
    class Program
    {
        /// <summary>
        ///     A simple example showing use of NNanomsg.
        ///     For further example usage, check out the Test project.
        /// </summary>
        static void Main(string[] args)
        {
            const string socketAddress = "tcp://127.0.0.1:5088";
            var buf = new byte[32];

            if (args[0] == "client")
            {
                int req = NN.Socket(Domain.SP, Protocol.REQ);
                int rc = NN.Connect(req, socketAddress);
                if (rc == -1)
                {
                    Console.WriteLine("There was an error connecting to '" + socketAddress + "': " + NN.StrError(NN.Errno()));
                }
                NN.Send(req, new StringMessage("hello from client").GetBytes(), 0);
                NN.Recv(req, buf, 0);
                Console.WriteLine("Message from SERVER: " + new StringMessage(buf).GetString());
                Console.WriteLine("CLIENT finished");
                NN.Close(req);
            }
            else if (args[0] == "server")
            {
                int rep = NN.Socket(Domain.SP, Protocol.REP);
                int rc = NN.Bind(rep, socketAddress);
                if (rc == -1)
                {
                    Console.WriteLine("There was an error binding to '" + socketAddress + "': " + NN.StrError(NN.Errno()));
                }
                NN.Recv(rep, buf, 0);
                Console.WriteLine("Message from CLIENT: " + new StringMessage(buf).GetString());
                NN.Send(rep, new StringMessage("hello from server").GetBytes(), 0);
                Console.WriteLine("SERVER Finished");
                NN.Close(rep);
            }
            else
            {
                Console.WriteLine("Unknown argument: " + args[0]);
            }
        }
    }
}
