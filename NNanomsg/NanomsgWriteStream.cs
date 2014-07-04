using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace NNanomsg
{

    public class NanomsgWriteStream : Stream
    {

        MemoryStream memoryStream;
        bool frozen = false;
        List<nn_iovec> iovec_list;
        long wholeLength = 0;

        public NanomsgWriteStream()
        {
          memoryStream = new MemoryStream();
          iovec_list = new List<nn_iovec>();
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
          if (frozen) return;

          if (memoryStream != null)
          {
            int length = (int)memoryStream.Length;
            if (length > 0)
            {
              var iovec = new nn_iovec();
              iovec.iov_len = length;

              // Reset Capacity, MemoryStream may allocate more than used
              memoryStream.Capacity = length;

              IntPtr pointer = Interop.nn_allocmsg(length, 0);
              unsafe
              {
                iovec.iov_base = pointer.ToPointer();
                Marshal.Copy(memoryStream.GetBuffer(), 0, pointer, length);
              }

              iovec_list.Add(iovec);
              wholeLength += length;

              using (memoryStream) memoryStream.Close();
              memoryStream = null;
            }
          }
        }

        public virtual void Freeze()
        {
          if (frozen) return;

          Flush();
          frozen = true;
        }

        internal virtual nn_iovec[] GetBuffers()
        {
          Flush();

          return iovec_list.ToArray();
        }

        internal virtual void ClearBuffers(bool free)
        {
          if (free)
          {
            unsafe
            {
              nn_iovec[] list = GetBuffers();
              foreach (nn_iovec iovec in list)
              {
                Interop.nn_freemsg((IntPtr)iovec.iov_base);
              }
            }
          }
          iovec_list.Clear();
        }

        public override long Length
        {
          get { return wholeLength; }
        }

        public override long Position
        {
            get
            {
              return wholeLength;
            }
            set
            {
              throw new InvalidOperationException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
          throw new InvalidOperationException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
          throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
          throw new InvalidOperationException();
        }

        public override void WriteByte(byte value)
        {
          if (frozen) return;

          if (memoryStream == null)
          {
            memoryStream = new MemoryStream();
          }
          memoryStream.WriteByte(value);
          wholeLength++;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
          if (frozen) return;

          if (memoryStream == null)
          {
            memoryStream = new MemoryStream();
          }
          memoryStream.Write(buffer, offset, count);
          wholeLength += count;
        }

        protected override void Dispose(bool disposing)
        {
          if (disposing)
          {
            if (memoryStream != null)
            {
              memoryStream.Dispose();
              memoryStream = null;
            }
            if (iovec_list != null)
            {
              if (iovec_list.Count > 0)
              {
                ClearBuffers(true);
              }
              iovec_list = null;
            }
          }
          base.Dispose(disposing);
        }
    }
}
