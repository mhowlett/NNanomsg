using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NNanomsg
{
    public static class NanomsgSymbols
    {
        static NanomsgSymbols()
        {
            Type thisType = typeof(NanomsgSymbols);
            for (int i = 0; ; ++i)
            {
                int value;

                var ptr = Interop.nn_symbol(i, out value);
                
                string symbolText = Marshal.PtrToStringAnsi(ptr);

                if (symbolText == null)
                    break;

                FieldInfo field =  thisType.GetField(symbolText, BindingFlags.Static | BindingFlags.Public);

                if (field != null)
                    field.SetValue(null, value);
                else
                    System.Diagnostics.Debug.Fail("Unused symbol " + symbolText);
            }
        }

        public static readonly int

            NN_NS_NAMESPACE,
            NN_NS_VERSION,
            NN_NS_DOMAIN,
            NN_NS_TRANSPORT,
            NN_NS_PROTOCOL,
            NN_NS_OPTION_LEVEL,
            NN_NS_SOCKET_OPTION,
            NN_NS_TRANSPORT_OPTION,
            NN_NS_OPTION_TYPE,
            NN_NS_FLAG,
            NN_NS_ERROR,
            NN_NS_LIMIT,
            NN_TYPE_NONE,
            NN_TYPE_INT,
            NN_TYPE_STR,
            NN_UNIT_NONE,
            NN_UNIT_BYTES,
            NN_UNIT_MILLISECONDS,
            NN_UNIT_PRIORITY,
            NN_UNIT_BOOLEAN,
            NN_SOCKET_NAME,

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
