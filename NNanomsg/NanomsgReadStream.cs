using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace NNanomsg
{

    public class NanomsgReadStream : Stream
    {
        UnmanagedMemoryStream unmanagedStream;
        IntPtr unmanagedPointer;

        unsafe public NanomsgReadStream(IntPtr buffer, long length)
        {
            if (buffer == null || buffer == IntPtr.Zero)
                throw new ArgumentNullException("buffer");

            if (length <= 0)
                throw new ArgumentOutOfRangeException("length");

            this.unmanagedPointer = buffer;
            this.unmanagedStream = new UnmanagedMemoryStream((byte*)buffer.ToPointer(), length);
        }

        protected override void Dispose(bool disposing)
        {
          if (unmanagedStream != null)
          {
            Interop.nn_freemsg(unmanagedPointer);

            unmanagedStream.Close();
            unmanagedStream.Dispose();
            unmanagedStream = null;
          }

          base.Dispose(disposing);
        }

        public override bool CanRead
        {
            get { return unmanagedStream.CanRead; }
        }

        public override bool CanSeek
        {
          get { return unmanagedStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
          get { return unmanagedStream.Length; }
        }

        public override long Position
        {
            get
            {
              return unmanagedStream.Position;
            }
            set
            {
              unmanagedStream.Position = value;
            }
        }

        public unsafe override int Read(byte[] buffer, int offset, int count)
        {
          return unmanagedStream.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
          return unmanagedStream.ReadByte();
        }

        public int? ReadInt32()
        {
          var buffer = new byte[4];

          if (this.Read(buffer, (int)Position, 4) < 4)
            return null;

          return BitConverter.ToInt32(buffer, 0);
        }

        public long? ReadInt64()
        {
          var buffer = new byte[8];

          if (this.Read(buffer, (int)Position, 8) < 8)
            return null;

          return BitConverter.ToInt64(buffer, 0);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
          return unmanagedStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
          throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
          throw new InvalidOperationException();
        }
    }
}
