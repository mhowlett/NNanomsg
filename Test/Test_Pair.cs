using System;
using System.Diagnostics;
using System.Threading;
using NNanomsg;

namespace Test
{
    class Test_Pair
    {
        public static void Execute()
        {
            Console.WriteLine("Executing Pair test");

            const string inprocAddress = "inproc://pair_test";

            var buffer1 = new byte[32];
            var buffer2 = new byte[32];

            var clientThread = new Thread(
                () =>
                {
                    var s1 = NN.Socket(Domain.SP, Protocol.PAIR);
                    NN.Connect(s1, inprocAddress);
                    NN.Send(s1, BitConverter.GetBytes((int)42), SendRecvFlags.NONE);
                    NN.Recv(s1, buffer1, SendRecvFlags.NONE);
                    Debug.Assert(BitConverter.ToInt32(buffer1, 0) == 77);
                });
            clientThread.Start();

            var s2 = NN.Socket(Domain.SP, Protocol.PAIR);
            NN.Bind(s2, inprocAddress);
            NN.Recv(s2, buffer2, SendRecvFlags.NONE);
            Debug.Assert(BitConverter.ToInt32(buffer2, 0) == 42);
            NN.Send(s2, BitConverter.GetBytes((int)77), SendRecvFlags.NONE);   

            // TODO: Test that a second client cannot be connected.
        }
    }
}
