using System;
using System.Diagnostics;
using System.Threading;
using NNanomsg;
using System.Runtime.InteropServices;

namespace Test
{
    class Test_SocketObject
    {
        public static void Execute()
        {
            Console.WriteLine("Executing Socket object test");

            const string inprocAddress = "tcp://127.0.0.1:6522";

            var clientThread = new Thread(
                () =>
                {
                    var req = new NanoMsgSocket(Domain.SP, Protocol.REQ);
                    req.Connect(inprocAddress);
                    //Thread.Sleep(TimeSpan.FromSeconds(3));
                    req.Send(new byte[] { 42, 0, 0, 0 });
                    using (var msgStream = req.Receive()) { }
                    var clientSend = BitConverter.GetBytes((int)42);

                    //var iocp = CreateIoCompletionPort(new IntPtr(socket), IntPtr.Zero, UIntPtr.Zero, 0);
                    byte[] streamOutput = new byte[4];

                    while (true)
                    {
                        var sw = Stopwatch.StartNew();
                        for (int i = 0; i < 10000; i++)
                        {
                            req.SendImmediate(clientSend);
                            var stream = req.Receive();
                            stream.Read(streamOutput, 0, 4);
                            stream.Dispose();
                        }
                        sw.Stop();
                        Console.WriteLine(" Time " + (sw.Elapsed.TotalMilliseconds / 10000d).ToString());
                    }

                });
            clientThread.Start();

            {
                var rep = new NanoMsgSocket(Domain.SP, Protocol.REP);
                rep.Bind(inprocAddress);

                var serverSend = BitConverter.GetBytes((int)77);
                byte[] streamOutput = new byte[4];

                //while (true)
                //{
                //    var stream = rep.Receive();
                //    stream.Read(streamOutput, 0, 4);
                //    stream.Dispose();
                //    rep.Send(serverSend);
                //}

                var listener = new Listener();
                listener.AddSocket(rep.SocketID);
                listener.ReceivedMessage += delegate(int s)
                {
                    var stream = rep.ReceiveImmediate();
                    if (stream == null) return;
                    stream.Read(streamOutput, 0, 4);
                    stream.Dispose();
                    rep.SendImmediate(serverSend);
                };
                while (true)
                {
                    listener.Listen(TimeSpan.FromMinutes(30));
                }
            }
        }
    }
}
