using System.Runtime.InteropServices;

public partial class NNanomsg
{
    internal class Interop_Linux
    {
        [DllImport("libnanomsg.so")]
        public static extern int nn_socket(int domain, int protocol);

        [DllImport("libnanomsg.so")]
        public static extern int nn_connect(int s, [MarshalAs(UnmanagedType.LPTStr)]string addr);

        [DllImport("libnanomsg.so")]
        public static extern int nn_bind(int s, [MarshalAs(UnmanagedType.LPTStr)]string addr);

        [DllImport("libnanomsg.so")]
        public static extern int nn_send(int s, byte[] buf, int len, int flags);

        [DllImport("libnanomsg.so")]
        public static extern int nn_recv(int s, byte[] buf, int len, int flags);

        [DllImport("libnanomsg.so", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nn_errno();
    }
}
