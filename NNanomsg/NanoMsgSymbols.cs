using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace NNanomsg
{
    public static class NanoMsgSymbols
    {
        static NanoMsgSymbols()
        {
            Type thisType = typeof(NanoMsgSymbols);
            for (int i = 0; ; ++i)
            {
                int value;
                string symbolText = Interop.nn_symbol(i, out value);

                if (symbolText == null)
                    break;

                FieldInfo field =  thisType.GetField(symbolText, BindingFlags.Static | BindingFlags.Public);
                field.SetValue(null, value);
            }
        }

        public static readonly int
            NN_VERSION_CURRENT,
            NN_VERSION_REVISION,
            NN_VERSION_AGE,
            AF_SP,
            AF_SP_RAW,
            NN_INPROC,
            NN_IPC,
            NN_TCP,
            NN_PAIR,
            NN_PUB,
            NN_SUB,
            NN_REP,
            NN_REQ,
            NN_PUSH,
            NN_PULL,
            NN_SURVEYOR,
            NN_RESPONDENT,
            NN_BUS,
            NN_SOCKADDR_MAX,
            NN_SOL_SOCKET,
            NN_LINGER,
            NN_SNDBUF,
            NN_RCVBUF,
            NN_SNDTIMEO,
            NN_RCVTIMEO,
            NN_RECONNECT_IVL,
            NN_RECONNECT_IVL_MAX,
            NN_SNDPRIO,
            NN_SNDFD,
            NN_RCVFD,
            NN_DOMAIN,
            NN_PROTOCOL,
            NN_IPV4ONLY,
            NN_SUB_SUBSCRIBE,
            NN_SUB_UNSUBSCRIBE,
            NN_REQ_RESEND_IVL,
            NN_SURVEYOR_DEADLINE,
            NN_TCP_NODELAY,
            NN_DONTWAIT,
            EADDRINUSE,
            EADDRNOTAVAIL,
            EAFNOSUPPORT,
            EAGAIN,
            EBADF,
            ECONNREFUSED,
            EFAULT,
            EFSM,
            EINPROGRESS,
            EINTR,
            EINVAL,
            EMFILE,
            ENAMETOOLONG,
            ENETDOWN,
            ENOBUFS,
            ENODEV,
            ENOMEM,
            ENOPROTOOPT,
            ENOTSOCK,
            ENOTSUP,
            EPROTO,
            EPROTONOSUPPORT,
            ETERM,
            ETIMEDOUT,
            EACCES,
            ECONNABORTED,
            ECONNRESET,
            EHOSTUNREACH,
            EMSGSIZE,
            ENETRESET,
            ENETUNREACH,
            ENOTCONN;

    }
}
