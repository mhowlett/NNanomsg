using System;
using System.Net;

namespace NNanomsg.Protocols
{
    public class SurveyorSocket : NanomsgSocketBase, IConnectSocket, IBindSocket, ISendSocket, IReceiveSocket
    {
        public SurveyorSocketOptions SurveyorOptions { get; private set; }

        public SurveyorSocket()
            : base(Domain.SP, Protocol.SURVEYOR)
        {

            if (SocketID >= 0)
                SurveyorOptions = new SurveyorSocketOptions(SocketID);
        }

        #region Connect
        public NanomsgEndpoint Connect(string address)
        {
            return ConnectImpl(address);
        }

        public NanomsgEndpoint Connect(IPAddress address, int port)
        {
            return ConnectImpl(address, port);
        }
        #endregion

        #region Bind
        public NanomsgEndpoint Bind(string address)
        {
            return BindImpl(address);
        }
        #endregion

        #region Send
        public void Send(byte[] buffer)
        {
            SendImpl(buffer);
        }

        public bool SendImmediate(byte[] buffer)
        {
            return SendImmediateImpl(buffer);
        }

        public NanomsgWriteStream CreateSendStream()
        {
            return CreateSendStreamImpl();
        }

        public void SendStream(NanomsgWriteStream stream)
        {
            SendStreamImpl(stream);
        }

        public bool SendStreamImmediate(NanomsgWriteStream stream)
        {
            return SendStreamImmediateImpl(stream);
        }
        #endregion

        #region Receive
        public byte[] Receive()
        {
            return ReceiveImpl();
        }

        public byte[] ReceiveImmediate()
        {
            return ReceiveImmediateImpl();
        }

        public NanomsgReadStream ReceiveStream()
        {
            return ReceiveStreamImpl();
        }

        public NanomsgReadStream ReceiveStreamImmediate()
        {
            return ReceiveStreamImmediateImpl();
        }
        #endregion
    }

    public class SurveyorSocketOptions
    {
        int _socket;

        public SurveyorSocketOptions(int socket)
        {
            _socket = socket;
        }

        /// <summary>
        /// Specifies how long to wait for responses to the survey. Once the deadline expires, receive function will return ETIMEDOUT error and all subsequent responses to the survey will be silently dropped. The deadline is measured in milliseconds. Option type is int. Default value is 1000 (1 second). 
        /// </summary>
        public TimeSpan Deadline
        {
            get
            {
                return NanomsgSocketOptions.GetTimespan(_socket, SocketOptionLevel.Surveyor, SocketOption.SURVEYOR_DEADLINE).Value;
            }
            set
            {
                NanomsgSocketOptions.SetTimespan(_socket, SocketOptionLevel.Surveyor, SocketOption.SURVEYOR_DEADLINE, value);
            }
        }
    }
}
