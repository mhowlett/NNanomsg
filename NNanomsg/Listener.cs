using System;
using System.Collections.Generic;
using System.Linq;

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

        public void Listen(TimeSpan timeout)
        {
            var res = NN.Poll(_sockets, Events.IN, timeout);
            for (int i = 0; i < res.Length; ++i)
            {
                if (res[i] != 0)
                {
                    ReceivedMessage(_sockets[i]);
                }
            }
        }

    }
}
