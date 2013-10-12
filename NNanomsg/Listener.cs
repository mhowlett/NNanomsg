using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace NNanomsg
{
    public class Listener
    {
        private int[] _sockets = new int[0];

        public void AddSocket(int s)
        {
            var sockets = new List<int>(_sockets);
            sockets.Add(s);
            _sockets = sockets.ToArray();
        }

        public void RemoveSocket(int s)
        {
            var sockets = _sockets.ToList();
            sockets.Remove(s);
            _sockets = sockets.ToArray();
        }

        public delegate void ReceivedDelegate(int s);

        public event ReceivedDelegate ReceivedMessage;

        [HandleProcessCorruptedStateExceptions]
        public void Listen(TimeSpan? timeout)
        {
            int[] res = null;
            try
            {
                
                res = NN.Poll(_sockets, Events.IN, timeout);
            }
            catch (Exception)
            {
                // I don't believe this ever happens.
                Console.WriteLine("DEBUG: Poll threw exception, ignoring.");
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
