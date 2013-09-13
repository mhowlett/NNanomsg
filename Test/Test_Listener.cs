using System;
using System.Diagnostics;
using System.Threading;
using NNanomsg;

namespace Test
{
    class Test_Listener
    {
        public static void Execute()
        {
            Console.WriteLine("Executing Listener test");

            const string inprocAddress = "tcp://127.0.0.1:6522";
            const string unusedAddress = "tcp://127.0.0.1:6521";

            byte[] buffer1;
            byte[] buffer2;

            var clientThread = new Thread(
                () => {
                    var req1 = NN.Socket(Domain.SP, Protocol.REQ);
                    NN.Connect(req1, unusedAddress);
                    var req = NN.Socket(Domain.SP, Protocol.REQ);
                    NN.Connect(req, inprocAddress);
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                    NN.Send(req, BitConverter.GetBytes((int)42), SendRecvFlags.NONE);
                    NN.Recv(req, out buffer1, SendRecvFlags.NONE);
                    Debug.Assert(BitConverter.ToInt32(buffer1, 0) == 77);
                    Console.WriteLine("Response: " + BitConverter.ToInt32(buffer1, 0));
                });
            clientThread.Start();

            var unused = NN.Socket(Domain.SP, Protocol.REP);
            NN.Bind(unused, unusedAddress);
            var rep = NN.Socket(Domain.SP, Protocol.REP);
            NN.Bind(rep, inprocAddress);

            var listener = new Listener();
            listener.AddSocket(unused);
            listener.AddSocket(rep);
            listener.ReceivedMessage += delegate(int s)
                {
                    NN.Recv(s, out buffer2, SendRecvFlags.NONE);
                    Console.WriteLine("Message: " + BitConverter.ToInt32(buffer2, 0));
                    NN.Send(s, BitConverter.GetBytes((int)77), SendRecvFlags.NONE);
                };

            while (true)
            {
                listener.Listen(TimeSpan.FromMinutes(30));
            }


        }

            
    }
}
