using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace NNanomsg
{
    public class NanomsgListener
    {
        private int[] _sockets = new int[0];

        public void AddSocket(NanomsgSocketBase s)
        {
            AddSocket(s.SocketID);
        }

        public void AddSocket(int s)
        {
            var sockets = new List<int>(_sockets);
            sockets.Add(s);
            _sockets = sockets.ToArray();
        }

        public void RemoveSocket(NanomsgSocketBase s)
        {
            RemoveSocket(s.SocketID);
        }

        public void RemoveSocket(int s)
        {
            var sockets = _sockets.ToList();
            sockets.Remove(s);
            _sockets = sockets.ToArray();
        }

        public delegate void ReceivedDelegate(int socketID);

        public event ReceivedDelegate ReceivedMessage;

        [HandleProcessCorruptedStateExceptions]
        public void Listen(TimeSpan? timeout)
        {
            int[] res;
            try
            {
                res = NN.Poll(_sockets, timeout);
            }
            catch (Exception e)
            {
                Console.WriteLine("DEBUG: Poll threw exception, ignoring: " + e);
                Thread.Sleep(TimeSpan.FromSeconds(1)); // This shouldn't ever happen, but when it does (!), this prevents a screen full of text.
                return;
            }

            for (int i = 0; i < res.Length; ++i)
            {
                if (res[i] != 0)
                {
                    if (ReceivedMessage != null)
                    {
                        ReceivedMessage(_sockets[i]);
                    }
                }
            }
        }

    }
}
