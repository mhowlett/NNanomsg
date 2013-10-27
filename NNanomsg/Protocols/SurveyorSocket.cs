using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNanomsg.Protocols
{
    public class SurveyorSocket : NanomsgSocket
    {
        public SurveyorSocketOptions SurveyorOptions { get; private set; }

        public SurveyorSocket()
            : base(Domain.SP, Protocol.SURVEYOR)
        {

            if (SocketID >= 0)
                SurveyorOptions = new SurveyorSocketOptions(SocketID);
        }
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
