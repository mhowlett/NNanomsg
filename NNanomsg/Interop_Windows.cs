using System.Runtime.InteropServices;

public partial class NNanomsg
{
    internal class Interop_Windows
    {
        [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nn_socket(int domain, int protocol);

        [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nn_connect(int s, string addr);

        [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nn_bind(int s, string addr);

        [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nn_send(int s, byte[] buf, int len, int flags);

        [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nn_recv(int s, byte[] buf, int len, int flags);

        [DllImport("Nanomsg.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nn_errno();
    }
}
