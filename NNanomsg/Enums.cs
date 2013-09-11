
namespace NNanomsg
{
    public enum Domain
    {
        SP = 1,
        SP_RAW = 2
    }

    public enum SocketOptions
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
        IPV4ONLY = 14
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

    public enum SendRecvFlags
    {
        NONE = 0,
        DONTWAIT = 1
    }

    public enum Error
    {
        NONE = 0
    }

    internal class Constants
    {
        public const int NN_SOL_SOCKET = 0;

        public const int NN_MSG = -1;

        // pair protocol related constants
        private const int NN_PROTO_PAIR = 1;
        public const int NN_PAIR = NN_PROTO_PAIR * 16 + 0;

        // pubsub protocol related constants
        private const int NN_PROTO_PUBSUB = 2;
        public const int NN_PUB = NN_PROTO_PUBSUB * 16 + 0;
        public const int NN_SUB = NN_PROTO_PUBSUB * 16 + 1;
        public const int NN_SUB_SUBSCRIBE = 1;
        public const int NN_SUB_UNSUBSCRIBE = 2;

        // reqrep protocol related constants
        private const int NN_PROTO_REQREP = 3;
        public const int NN_REQ = NN_PROTO_REQREP * 16 + 0;
        public const int NN_REP = NN_PROTO_REQREP * 16 + 1;
        public const int NN_REQ_RESEND_IVL = 1;

        // pipeline protocol related constants
        private const int NN_PROTO_PIPELINE = 5;
        public const int NN_PUSH = NN_PROTO_PIPELINE * 16 + 0;
        public const int NN_PULL = NN_PROTO_PIPELINE * 16 + 1;

        // survey protocol related constants
        private const int NN_PROTO_SURVEY = 6;
        public const int NN_SURVEYOR = NN_PROTO_SURVEY * 16 + 0;
        public const int NN_RESPONDENT = NN_PROTO_SURVEY * 16 + 1;
        public const int NN_SURVEYOR_DEADLINE = 1;

        // bus protocol related constants
        private const int NN_PROTO_BUS = 7;
        public const int NN_BUS = NN_PROTO_BUS * 16 + 0;
    }
}
