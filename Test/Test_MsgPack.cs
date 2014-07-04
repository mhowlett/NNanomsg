using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NNanomsg;
using MsgPack;

namespace Test
{
    static class Test_MsgPack
    {

        const int reqPort = 4444;
        const int repPort = 4445;

        public static void RunDevice(CancellationTokenSource cancellor)
        {

            var deviceThread = new Thread(() =>
            {

                var reqSock = new NanomsgSocket(Domain.SP_RAW, Protocol.REP);
                int reqSockId = reqSock.SocketID;
                reqSock.Bind("tcp://127.0.0.1:" + reqPort);

                var repSock = new NanomsgSocket(Domain.SP_RAW, Protocol.REQ);
                int repSockId = repSock.SocketID;
                repSock.Bind("tcp://127.0.0.1:" + repPort);

                // HACK: The nn_device loop is currently only terminated using NN.Term()
                int repDevice = NN.Device(reqSockId, repSockId);

                reqSock.Dispose();
                repSock.Dispose();
            });
            deviceThread.Start();
            deviceThread.Join(TimeSpan.FromMilliseconds(40));
        }

        public static void RunWorker(CancellationTokenSource cancellor, int workerId)
        {

            var serverThread = new Thread(() =>
            {

                var nanoSock = new NanomsgSocket(Domain.SP, Protocol.REP);

                var nanoListener = new NanomsgListener();
                nanoListener.ReceivedMessage += (socketId) =>
                {

                    string input;

                    using (NanomsgReadStream inStream = nanoSock.ReceiveStream())
                    using (var unpacker = Unpacker.Create(inStream))
                    {
                        if (!unpacker.ReadString(out input))
                            throw new Exception("REQ invalid");
                        Console.WriteLine(input);
                    }

                    using (NanomsgWriteStream outStream = nanoSock.CreateSendStream())
                    using (var packer = Packer.Create(outStream))
                    {
                        packer.PackString("Hello " + input);
                        nanoSock.SendStream(outStream);
                    }
                };

                nanoListener.AddSocket(nanoSock);
                nanoSock.Connect("tcp://127.0.0.1:" + repPort);

                while (!cancellor.IsCancellationRequested)
                {
                    nanoListener.Listen(TimeSpan.FromMilliseconds(250));
                }

                nanoListener.RemoveSocket(nanoSock);
                nanoSock.Dispose();
            });
            serverThread.Start();
            serverThread.Join(TimeSpan.FromMilliseconds(40));

        }

        public static string Request(string input)
        {

            // prepare Socket
            var nanoSock = new NanomsgSocket(Domain.SP, Protocol.REQ);



            string result = null;
            bool messageReceived = false;

            // prepare Listener
            var listener = new NanomsgListener();
            listener.ReceivedMessage += (sockedId) =>
            {
                using (NanomsgReadStream inStream = nanoSock.ReceiveStream())
                using (var unpacker = Unpacker.Create(inStream))
                {
                    string output;
                    if (!unpacker.ReadString(out output))
                        throw new Exception("REP invalid");
                    result = output;
                }
                messageReceived = true;
            };

            // Connect and Send Request
            listener.AddSocket(nanoSock);
            nanoSock.Connect("tcp://127.0.0.1:" + reqPort);
            using (NanomsgWriteStream outStream = nanoSock.CreateSendStream())
            using(var packer = Packer.Create(outStream))
            {
                packer.PackString(input);
                nanoSock.SendStream(outStream);
            }

            var start = DateTime.Now;
            while (!messageReceived && start + TimeSpan.FromSeconds(30) > DateTime.Now)
            {
                listener.Listen(TimeSpan.FromMilliseconds(250));
            }

            listener.RemoveSocket(nanoSock);
            nanoSock.Dispose();

            if (!messageReceived)
            {
                throw new Exception("REQ timed out");
            }

            return result;
        }

        public static void Execute()
        {
            var args = new string[] { "World" };
            // This allows to Cancel the while loop and grateful abort Thread's
            var cancellor = new CancellationTokenSource();

            // First Run the nn_device's
            RunDevice(cancellor);

            // Second Run some REP workers
            for (var i = 0; i < 4; i++)
            {
                RunWorker(cancellor, i);
            }

            // Third Run some multithreaded REQ workers
            Parallel.For(0, 100, h =>
            {
                StringBuilder consoleWriter = new StringBuilder();
                var random = new Random();

                for (var i = 0; i < args.Length; i++)
                {

                    var rnd = random.Next();
                    var arg = args[i] + " " + rnd;
                    // The NanoREQ
                    var message = Request(arg);
                    bool isValid = message == ("Hello " + arg);
                    consoleWriter.AppendFormat("{0} {1,4:D} {2} {3}\r\n", DateTime.Now.ToString("G"), (h * args.Length) + i, isValid, message);

                }
                Console.Write(consoleWriter.ToString());
            });

            //Console.WriteLine("PRESS ANYKEY");
            //Console.ReadKey(true);

            cancellor.Cancel();
            //NNanomsg.NN.Term();
        }
    }
}