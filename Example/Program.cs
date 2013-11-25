using System;
using NNanomsg;
using System.IO;
using NNanomsg.Protocols;

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
                using (var req = new RequestSocket())
                {
                    req.Connect(socketAddress);

                    using (var ms = new MemoryStream())
                    {
                        using (var sw = new StreamWriter(ms))
                        {
                            sw.Write("hello from client");
                        }
                        req.Send(ms.ToArray());
                    }

                    using (var ms = new MemoryStream(req.Receive()))
                    using (var sr = new StreamReader(ms))
                    {
                        Console.WriteLine("Message from SERVER: " + sr.ReadToEnd());
                    }

                    Console.WriteLine("CLIENT finished");
                }
            }

            else if (args[0] == "server")
            {
                using (var rep = new ReplySocket())
                {
                    rep.Bind(socketAddress);

                    var listener = new NanomsgListener();
                    listener.AddSocket(rep);
                    listener.ReceivedMessage += socketId =>
                        {
                            using (var ms = new MemoryStream(rep.Receive()))
                            using (var sr = new StreamReader(ms))
                            {
                                Console.WriteLine("Message from CLIENT: " + sr.ReadToEnd());
                            }

                            using (var ms = new MemoryStream())
                            {
                                using (var sw = new StreamWriter(ms))
                                {
                                    sw.Write("hello from server");
                                }
                                rep.Send(ms.ToArray());
                            }

                            Console.WriteLine("SERVER Finished");
                            Environment.Exit(0);
                        };

                    listener.Listen(null);
                }
            }
            else
            {
                Console.WriteLine("Unknown argument: " + args[0]);
            }
        }
    }
}
