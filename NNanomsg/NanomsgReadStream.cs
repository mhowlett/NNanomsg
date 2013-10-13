using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace NNanomsg
{
    public interface INativeDisposer<T>
    {
        void DisposeOf(IntPtr nativeResource, T owner);
    }

    public unsafe class NanomsgReadStream : Stream
    {
        long _length, _position;
        byte* _buffer;
        INativeDisposer<NanomsgReadStream> _disposer;

        public NanomsgReadStream(IntPtr buffer, long length, INativeDisposer<NanomsgReadStream> disposer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (length <= 0)
                throw new ArgumentOutOfRangeException("length");

            _buffer = (byte*)buffer.ToPointer();
            _length = length;
            _disposer = disposer;
        }

        public void Reinitialize(IntPtr buffer, long length)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (length <= 0)
                throw new ArgumentOutOfRangeException("length");

            _length = length;
            _position = 0;
            _buffer = (byte*)buffer.ToPointer();
            GC.ReRegisterForFinalize(this);
        }

        protected override void Dispose(bool disposing)
        {
            var length = Interlocked.Exchange(ref _length, -1);
            var buffer = _buffer;
            _buffer = null;

            if (buffer != null && length > 0 && _disposer != null)
                _disposer.DisposeOf((IntPtr)buffer, this);
            

            base.Dispose(disposing);
        }

        public override bool CanRead
        {
            get { return _position < _length; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
            }
        }

        /// <summary>
        /// This isn't the fastest memory copy method for large reads (4k or above), but it has reliably good performance on all platforms.
        /// 
        /// This *is* the fastest method for reads up to around 512 bytes, on the platforms I tested.
        /// 
        /// Given that deserialization will be the most common consumption of this (protocol buffers and such), we will be far 
        /// more likely to see many small reads.  We could consider swapping to a platform-specific bulk copy path for larger sizes.
        /// </summary>
        unsafe static void CopyMemory(byte* src, byte* dest, int length)
        {
            if (length >= 16)
            {
                do
                {
                    *(long*)dest = *(long*)src;
                    *(long*)(dest + 8) = *(long*)(src + 8);
                    dest += 16;
                    src += 16;
                }
                while ((length -= 16) >= 16);
            }
            if (length > 0)
            {
                if ((length & 8) != 0)
                {
                    *(long*)dest = *(long*)src;
                    dest += 8;
                    src += 8;
                }
                if ((length & 4) != 0)
                {
                    *(int*)dest = *(int*)src;
                    dest += 4;
                    src += 4;
                }
                if ((length & 2) != 0)
                {
                    *(short*)dest = *(short*)src;
                    dest += 2;
                    src += 2;
                }
                if ((length & 1) != 0)
                {
                    byte* finalByte = dest;
                    dest = finalByte + 1;
                    byte* finalByteSrc = src;
                    src = finalByteSrc + 1;
                    *finalByte = *finalByteSrc;
                }
            }
        }

        unsafe static void PinAndCopyMemory(byte* src, int srcIndex, byte[] dest, int destIndex, int len)
        {
            if (len == 0)
                return;

            fixed (byte* ptr = dest)
                CopyMemory(src + srcIndex, ptr + destIndex, len);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var remaining = _length - _position;
            if (remaining < count)
                count = (int)remaining;
            if (count <= 0)
                return 0;
            PinAndCopyMemory(_buffer, (int)_position, buffer, offset, count);
            _position += count;
            return count;
        }

        public override int ReadByte()
        {
            if (_position >= _length)
                return -1;

            return *(_buffer + _position++);
        }

        public int? ReadInt32()
        {
            if (_position > _length - 4)
                return null;

            int result = *((int*)(_buffer + _position));
            _position += 4;
            return result;
        }

        public long? ReadInt64()
        {
            if (_position > _length - 8)
                return null;

            var result = *((long*)(_buffer + _position));
            _position += 8;
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _position = offset;
                    break;
                case SeekOrigin.Current:
                    _position += offset;
                    break;
                case SeekOrigin.End:
                    _position = _length + offset;
                    break;
            }

            return _position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
