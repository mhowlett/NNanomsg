using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NNanomsg;

namespace Example
{
  class Device
  {

    const string host = "tcp://127.0.0.1:";
    // const string host = "inproc://NanoTest";
    const int reqPort = 4444;
    const int repPort = 4445;

    public static void RunDevice(CancellationTokenSource cancellor)
    {

      var deviceThread = new Thread(() =>
      {

        // Bind REP to incoming REQ
        var reqSock = new NanomsgSocket(Domain.SP_RAW, Protocol.REP);
        int reqSockId = reqSock.SocketID;
        reqSock.Bind(host + reqPort);

        // Bind REQ to outgoing REP
        var repSock = new NanomsgSocket(Domain.SP_RAW, Protocol.REQ);
        int repSockId = repSock.SocketID;
        repSock.Bind(host + repPort);

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

          // Receive REQ
          using (NanomsgReadStream inStream = nanoSock.ReceiveStream())
          {
            byte[] inMessage = new byte[inStream.Length];
            inStream.Read(inMessage, 0, (int)inStream.Length);

            // Read the REQ
            input = Encoding.UTF8.GetString(inMessage);
          }


          NanomsgWriteStream outStream = nanoSock.CreateSendStream();

          // Write the REP
          string output = "Hello " + input;

          byte[] outMessage = Encoding.UTF8.GetBytes(output);
          outStream.Write(outMessage, 0, outMessage.Length);

          // Send REP
          using (outStream)
          {
            nanoSock.SendStream(outStream);
          }
        };

        nanoListener.AddSocket(nanoSock);
        nanoSock.Connect(host + repPort);

        // Poll
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

      // Prepare Socket
      var nanoSock = new NanomsgSocket(Domain.SP, Protocol.REQ);

      NanomsgWriteStream outStream = nanoSock.CreateSendStream();

      // Write the REQ
      byte[] outMessage = Encoding.UTF8.GetBytes(input);
      outStream.Write(outMessage, 0, outMessage.Length);

      string result = null;
      bool messageReceived = false;

      // Prepare Listener
      var listener = new NanomsgListener();
      listener.ReceivedMessage += (sockedId) =>
      {
        // Receive REP
        using (NanomsgReadStream inStream = nanoSock.ReceiveStream())
        {
          byte[] inMessage = new byte[inStream.Length];
          inStream.Read(inMessage, 0, (int)inStream.Length);

          // Read the REP
          result = Encoding.UTF8.GetString(inMessage);
        }
        messageReceived = true;
      };

      // Connect
      listener.AddSocket(nanoSock);
      nanoSock.Connect(host + reqPort);

      // Send REQ
      using (outStream)
      {
        nanoSock.SendStream(outStream);
        outStream.Close();
      }

      // Poll
      var start = DateTime.Now;
      while (!messageReceived && start + TimeSpan.FromSeconds(30) > DateTime.Now)
      {
        listener.Listen(TimeSpan.FromMilliseconds(250));
      }

      listener.RemoveSocket(nanoSock);
      nanoSock.Dispose();

      if (!messageReceived)
      {
        // TODO HACK: just report null or throw a better Exception
        throw new Exception("REQ timed out");
      }

      return result;
    }

    public static void Execute(string[] args)
    {

      if (args == null || args.Length == 0)
      {
        args = new string[] { "World" };
      }

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

          // The Nanomsg REQuest
          var message = Request(arg);

          bool isValid = message == ("Hello " + arg);
          consoleWriter.AppendFormat("{0} {1,4:D} {2} {3}\r\n", DateTime.Now.ToString("G"), (h * args.Length) + i, isValid, message);

        }
        Console.Write(consoleWriter.ToString());
      });

      // Console.WriteLine("PRESS ANYKEY");
      // Console.ReadKey(true);

      cancellor.Cancel();
      NNanomsg.NN.Term();
    }
  }

}
