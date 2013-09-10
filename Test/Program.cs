using System;

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
                int req = NNanomsg.Socket(NNanomsg.AF_SP, NNanomsg.NN_REQ);
                int rc = NNanomsg.Connect(req, socketAddress);
                if (rc == -1)
                {
                    Console.WriteLine("There was an error connecting to '" + socketAddress + "': " + NNanomsg.StrError(NNanomsg.Errno()));
                }
                NNanomsg.Send(req, new StringMessage("hello from client").GetBytes(), 0);
                NNanomsg.Recv(req, buf, 0);
                Console.WriteLine("Message from SERVER: " + new StringMessage(buf).GetString());
                Console.WriteLine("CLIENT finished");
                NNanomsg.Close(req);
            }
            else if (args[0] == "server")
            {
                int rep = NNanomsg.Socket(NNanomsg.AF_SP, NNanomsg.NN_REP);
                int rc = NNanomsg.Bind(rep, socketAddress);
                if (rc == -1)
                {
                    Console.WriteLine("There was an error binding to '" + socketAddress + "': " + NNanomsg.StrError(NNanomsg.Errno()));
                }
                NNanomsg.Recv(rep, buf, 0);
                Console.WriteLine("Message from CLIENT: " + new StringMessage(buf).GetString());
                NNanomsg.Send(rep, new StringMessage("hello from server").GetBytes(), 0);
                Console.WriteLine("SERVER Finished");
                NNanomsg.Close(rep);
            }
            else
            {
                Console.WriteLine("Unknown argument: " + args[0]);
            }
        }
    }
}
