
using System;

namespace NNanomsg
{
    public enum Domain
    {
        SP = 1,
        SP_RAW = 2
    }

    public enum SocketOption
    {
        LINGER = 1,
        SNDBUF = 2,
        RCVBUF = 3,
        SNDTIMEO = 4,
        RCVTIMEO = 5,
        RECONNECT_IVL = 6,
        RECONNECT_IVL_MAX = 7,
        SNDPRIO = 8,
        SNDFD = 10,
        RCVFD = 11,
        DOMAIN = 12,
        PROTOCOL = 13,
        IPV4ONLY = 14,
        TCP_NODELAY = Constants.NN_TCP_NODELAY,
        SURVEYOR_DEADLINE = Constants.NN_SURVEYOR_DEADLINE,
        REQ_RESEND_IVL = Constants.NN_REQ_RESEND_IVL,
        SUB_SUBSCRIBE = Constants.NN_SUB_SUBSCRIBE,
        SUB_UNSUBSCRIBE = Constants.NN_SUB_UNSUBSCRIBE
    }

    public enum SocketOptionTcp
    {
        NoDelay = Constants.NN_TCP_NODELAY
    }

    public enum SocketOptionSurvey
    {
        SurveyorDeadline = Constants.NN_SURVEYOR_DEADLINE
    }

    public enum SocketOptionRequest
    {
        RequestResendInterval = Constants.NN_REQ_RESEND_IVL
    }

    public enum SocketOptionSub
    {
        Subscribe = Constants.NN_SUB_SUBSCRIBE,
        Unsubscribe = Constants.NN_SUB_UNSUBSCRIBE
    }

    public enum SocketOptionLevel
    {
        Default = Constants.NN_SOL_SOCKET,
        Ipc = Constants.NN_IPC,
        InProcess = Constants.NN_INPROC,
        Tcp = Constants.NN_TCP,
        Pair = Constants.NN_PAIR,
        Publish = Constants.NN_PUB,
        Subscribe = Constants.NN_SUB,
        Request = Constants.NN_REQ,
        Reply = Constants.NN_REP,
        Push = Constants.NN_PUSH,
        Pull = Constants.NN_PULL,
        Surveyor = Constants.NN_SURVEYOR,
        Respondent = Constants.NN_RESPONDENT,
        Bus = Constants.NN_BUS
    }

    public enum Protocol
    {
        PAIR = Constants.NN_PAIR,
        PUB = Constants.NN_PUB,
        SUB = Constants.NN_SUB,
        REQ = Constants.NN_REQ,
        REP = Constants.NN_REP,
        PUSH = Constants.NN_PUSH,
        PULL = Constants.NN_PULL,
        SURVEYOR = Constants.NN_SURVEYOR,
        RESPONDENT = Constants.NN_RESPONDENT,
        BUS = Constants.NN_BUS
    }

    public enum Transport
    {
        IPC = Constants.NN_IPC,
        INPROC = Constants.NN_INPROC,
        TCP = Constants.NN_TCP
    }

    public enum SendRecvFlags
    {
        NONE = 0,
        DONTWAIT = 1
    }

    public enum Error
    {
        NONE = 0
    }

    [Flags]
    public enum Events
    {
        In = 0x01,
        Out = 0x02
    }

    public class Constants
    {
        public const int

        NN_SOL_SOCKET = 0,

        NN_MSG = -1,

        // pair protocol related constants
        NN_PROTO_PAIR = 1,
        NN_PAIR = NN_PROTO_PAIR * 16 + 0,

        // pubsub protocol related constants
        NN_PROTO_PUBSUB = 2,
        NN_PUB = NN_PROTO_PUBSUB * 16 + 0,
        NN_SUB = NN_PROTO_PUBSUB * 16 + 1,
        NN_SUB_SUBSCRIBE = 1,
        NN_SUB_UNSUBSCRIBE = 2,

        // reqrep protocol related constants
        NN_PROTO_REQREP = 3,
        NN_REQ = NN_PROTO_REQREP * 16 + 0,
        NN_REP = NN_PROTO_REQREP * 16 + 1,
        NN_REQ_RESEND_IVL = 1,

        // pipeline protocol related constants
        NN_PROTO_PIPELINE = 5,
        NN_PUSH = NN_PROTO_PIPELINE * 16 + 0,
        NN_PULL = NN_PROTO_PIPELINE * 16 + 1,

        // survey protocol related constants
        NN_PROTO_SURVEY = 6,
        NN_SURVEYOR = NN_PROTO_SURVEY * 16 + 0,
        NN_RESPONDENT = NN_PROTO_SURVEY * 16 + 1,
        NN_SURVEYOR_DEADLINE = 1,

        // bus protocol related constants
        NN_PROTO_BUS = 7,
        NN_BUS = NN_PROTO_BUS * 16 + 0,

        // tcp transport related constants
        NN_TCP = -3,
        NN_TCP_NODELAY = 1,

        NN_IPC = -2,
        NN_INPROC = -1;
    }
}
