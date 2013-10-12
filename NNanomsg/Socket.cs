using System;

namespace NNanomsg
{
    public class Socket : IDisposable
    {
        private int _socket;

        public Socket(Domain domain, Protocol protocol)
        {
            _socket = NN.Socket(domain, protocol);
            if (_socket < 0)
            {
                throw new NanomsgException();
            }
        }

        public void Dispose()
        {
            var rc = NN.Close(_socket);
            if (rc < 0)
            {
                // silently ignore - dispose should not throw exceptions.
                // TODO: what is correct here?
            }
        }

        public void Close()
        {
            var rc = NN.Close(_socket);
            if (rc < 0)
            {
                throw new NanomsgException();
            }
        }

        public int Bind(string addr)
        {
            var rc = NN.Bind(_socket, addr);
            if (rc < 0)
            {
                throw new NanomsgException();
            }
            return rc;
        }

        public int Connect(string addr)
        {
            var rc = NN.Connect(_socket, addr);
            if (rc < 0)
            {
                throw new NanomsgException();
            }
            return rc;
        }

        #region SetSockOpt
        public void SetSockOpt(Protocol level, int option, int val)
        {
            var rc = NN.SetSockOpt(_socket, level, option, val);
            if (rc < 0)
            {
                throw new NanomsgException();
            }
        }

        /// <summary>
        ///     TODO: I think the string version of this is never needed.
        /// </summary>
        public void SetSockOpt(Protocol level, int option, string val)
        {
            var rc = NN.SetSockOpt(_socket, level, option, val);
            if (rc < 0)
            {
                throw new NanomsgException();
            }
        }

        public void SetSockOpt(SocketOptions option, int val)
        {
            var rc = NN.SetSockOpt(_socket, option, val);
            if (rc < 0)
            {
                throw new NanomsgException();
            }
        }

        /// <summary>
        ///     TODO: I think the string version of this is never needed.
        /// </summary>
        public void SetSockOpt(SocketOptions option, string val)
        {
            var rc = NN.SetSockOpt(_socket, option, val);
            if (rc < 0)
            {
                throw new NanomsgException();
            }
        }
        #endregion

        #region GetSockOpt
        public int GetSockOpt(Protocol level, int option)
        {
            int val;
            var rc = NN.GetSockOpt(_socket, level, option, out val);
            if (rc == -1)
            {
                throw new NanomsgException();
            }
            return val;
        }

        public int GetSockOpt(SocketOptions option)
        {
            int val;
            var rc = NN.GetSockOpt(_socket, option, out val);
            if (rc == -1)
            {
                throw new NanomsgException();
            }
            return val;
        }
        #endregion

        public void Shutdown(int how)
        {
            var rc = NN.Shutdown(_socket, how);
            if (rc < 0)
            {
                throw new NanomsgException();
            }
        }

        public int Send(byte[] buf, SendRecvFlags flags)
        {
            var rc = NN.Send(_socket, buf, flags);
            if (rc < 0)
            {
                throw new NanomsgException();
            }
            return rc;
        }

        #region Recv
        public int Recv(byte[] buf, SendRecvFlags flags)
        {
            var rc = NN.Recv(_socket, buf, flags);
            if (rc < 0)
            {
                throw new NanomsgException();
            }
            return rc;
        }

        public byte[] Recv(SendRecvFlags flags)
        {
            byte[] buf;
            var rc = NN.Recv(_socket, out buf, flags);
            if (rc < 0)
            {
                throw new NanomsgException();
            }
            return buf;
        }
        #endregion

        public void Term()
        {
            NN.Term();
        }
    }
}
