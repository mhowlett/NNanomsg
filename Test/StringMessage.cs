using System;
using System.Text;

namespace Test
{
    public class StringMessage
    {
        private string _msg;

        public StringMessage(string msg)
        {
            _msg = msg;
        }

        public StringMessage(byte[] msg)
        {
            int len = BitConverter.ToInt32(msg, 0);
            var strBytes = new byte[len];
            Array.Copy(msg, sizeof(int), strBytes, 0, len);
            _msg = Encoding.UTF8.GetString(strBytes);
        }

        public byte[] GetBytes()
        {
            var enc = Encoding.UTF8.GetBytes(_msg);
            var result = new byte[enc.Length + sizeof (int)];
            var lenEnc = BitConverter.GetBytes(enc.Length);
            Array.Copy(lenEnc, result, sizeof(int));
            Array.Copy(enc, 0, result, sizeof(int), enc.Length);
            return result;
        }

        public string GetString()
        {
            return _msg;
        }
    }
}
