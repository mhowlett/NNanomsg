using System;
using System.Diagnostics;
using System.Threading;
using NNanomsg;

namespace Test
{
    class Test_ReqRep
    {
        public static void Execute()
        {
            Console.WriteLine("Executing ReqRep test");

            const string inprocAddress = "inproc://reqrep_test";

            var buffer1 = new byte[32];
            var buffer2 = new byte[32];

            var clientThread = new Thread(
                () => {
                    var req = NN.Socket(Domain.SP, Protocol.REQ);
                    NN.Connect(req, inprocAddress);
                    NN.Send(req, BitConverter.GetBytes((int) 42), SendRecvFlags.NONE);
                    NN.Recv(req, buffer1, SendRecvFlags.NONE);
                    Debug.Assert(BitConverter.ToInt32(buffer1, 0) == 77);
                });
            clientThread.Start();

            var rep = NN.Socket(Domain.SP, Protocol.REP);
            NN.Bind(rep, inprocAddress);
            NN.Recv(rep, buffer2, SendRecvFlags.NONE);
            Debug.Assert(BitConverter.ToInt32(buffer2, 0) == 42);
            NN.Send(rep, BitConverter.GetBytes((int) 77), SendRecvFlags.NONE);
        }
    }
}
