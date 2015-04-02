using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace NNanomsg
{
    public class NanomsgListener
    {
        private int[] _sockets = new int[1];
        private int[] _results = new int[1];
        private nn_pollfd[] _pollfds = new nn_pollfd[1];
        private int _ct = 0;

        public void AddSocket(NanomsgSocketBase s)
        {
            AddSocket(s.SocketID);
        }

        public void AddSocket(int s)
        {
            var cap = _sockets.Length;
            if (_ct >= cap)
            {
                var new_cap = cap * 2;
                var new_sockets = new int[new_cap];
                var new_results = new int[new_cap];
                var new_pollfds = new nn_pollfd[new_cap];
                Array.Copy(_sockets, new_sockets, _ct);
                Array.Copy(_results, new_results, _ct);
                Array.Copy(_pollfds, new_pollfds, _ct);
                _sockets = new_sockets;
                _results = new_results;
                _pollfds = new_pollfds;
            }
            _sockets[_ct] = s;
            ++_ct;
        }

        public void RemoveSocket(NanomsgSocketBase s)
        {
            RemoveSocket(s.SocketID);
        }

        public void RemoveSocket(int s)
        {
            for (int i = 0; i < _ct; ++i)
            {
                if (_sockets[i] == s)
                {
                    Array.Copy(_sockets, i + 1, _sockets, i, _ct - i - 1);
                    Array.Copy(_results, i + 1, _results, i, _ct - i - 1);
                    Array.Copy(_pollfds, i + 1, _pollfds, i, _ct - i - 1);
                    --_ct;
                    break;
                }
            }
        }

        public delegate void ReceivedDelegate(int socketID);

        public event ReceivedDelegate ReceivedMessage;

        [HandleProcessCorruptedStateExceptions]
        public void Listen(TimeSpan? timeout)
        {
            try
            {
                 NN.Poll(_sockets, _ct, _results, _pollfds, timeout);
            }
            catch (Exception e)
            {
                Console.WriteLine("DEBUG: Poll threw exception, ignoring: " + e);
                Thread.Sleep(TimeSpan.FromSeconds(1)); // This shouldn't ever happen, but when it does (!), this prevents a screen full of text.
                return;
            }

            for (int i = 0; i < _ct; ++i)
            {
                if (_results[i] != 0)
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
