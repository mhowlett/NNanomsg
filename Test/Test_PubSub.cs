using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NNanomsg;
using NNanomsg.Protocols;
using System.Threading;
using System.Diagnostics;

namespace Test
{
    public static class Test_PubSub
    {
        const string InprocAddress = "inproc://127.0.0.1:6519";
        const int DataSize = TestConstants.DataSize, BufferSize = 1024 * 4, Iter = TestConstants.Iterations * 10;
        public static void Execute()
        {
            Console.WriteLine("Executing pubsub test ");
            int receiveCount = 0;
            var clientThread = new Thread(
                () =>
                {
                    var subscriber = new SubscribeSocket();
                    subscriber.Connect(InprocAddress);
                    subscriber.Subscribe("TestMessage");

                    byte[] streamOutput = new byte[BufferSize];
                    while (true)
                    {
                        var sw = Stopwatch.StartNew();
                        for (int i = 0; i < Iter; i++)
                        {
                            int read = 0;
                            streamOutput = subscriber.Receive();
                            read = streamOutput.Length;
                            //using (var stream = subscriber.ReceiveStream())
                            //    while (stream.Length != stream.Position)
                            //    {
                            //        read += stream.Read(streamOutput, 0, streamOutput.Length);
                            //        var message = Encoding.ASCII.GetString(streamOutput, 0, read);
                            //        Trace.Assert(message.StartsWith("TestMessage"));
                            //        
                            //        break;
                            //    }

                            ++receiveCount;
                        }
                        sw.Stop();
                        var secondsPerSend = sw.Elapsed.TotalSeconds / (double)Iter;
                        Console.WriteLine("Time {0} us, {1} per second, {2} mb/s ",
                            (int)(secondsPerSend * 1000d * 1000d),
                            (int)(1d / secondsPerSend),
                            (int)(DataSize * 2d / (1024d * 1024d * secondsPerSend)));
                    }
                });
            clientThread.Start();

            
            {
                var publisher = new PublishSocket();
                publisher.Bind(InprocAddress);
                Thread.Sleep(100);
                var sw = Stopwatch.StartNew();
                int sendCount = 0;
                var text = "TestMessage" + new string('q', 10);
                var data = Encoding.ASCII.GetBytes(text);
                while (sw.Elapsed.TotalSeconds < 10)
                {
                    publisher.Send(data);
                    ++sendCount;
                }
                Thread.Sleep(100);
                clientThread.Abort();

                Console.WriteLine("Send count {0} receive count {1}", sendCount, receiveCount);
                publisher.Dispose();
            }
        }
    }
}
