using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace NNanomsg
{
    public unsafe class NanomsgWriteStream : Stream
    {
        NanomsgSocketBase _socket;
        int _length;
        BufferHeader* _first, _current;
        BufferPool _pool;

        #region Buffer
        //[DllImport("kernel32.dll", SetLastError = true)]
        //static extern IntPtr HeapCreate(uint flOptions, UIntPtr dwInitialsize, UIntPtr dwMaximumSize);

        //[DllImport("kernel32.dll", SetLastError = false)]
        //static extern IntPtr HeapAlloc(IntPtr hHeap, uint dwFlags, UIntPtr dwSize);

        //[DllImport("kernel32.dll", SetLastError = false)]
        //static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

        //[DllImport("kernel32.dll", SetLastError = true)]
        //static extern bool HeapDestroy(IntPtr hHeap);

        [StructLayout(LayoutKind.Sequential)]
        unsafe struct BufferHeader
        {
            public BufferHeader* Next;
            public int Size, Used;
            public static readonly int BufferHeaderSize = Marshal.SizeOf(typeof(BufferHeader));
            public static byte* Data(BufferHeader* header)
            {
                return ((byte*)header) + BufferHeader.BufferHeaderSize;
            }
        }

        unsafe class BufferPool
        {
            int _pageSize, _capacity, _threadID;
            readonly Queue<IntPtr> _pool;

            public BufferPool(int pageSize, int maxCached)
            {
                _pageSize = pageSize;
                _capacity = maxCached;
                _pool = new Queue<IntPtr>(maxCached);
                _threadID = Thread.CurrentThread.ManagedThreadId;
            }

            public BufferHeader* Alloc()
            {
                var rightThread = Thread.CurrentThread.ManagedThreadId == _threadID;
                if (rightThread && _pool.Count > 0)
                    return (BufferHeader*)_pool.Dequeue();

                BufferHeader* header = (BufferHeader*)Marshal.AllocHGlobal(_pageSize + BufferHeader.BufferHeaderSize);
                (*header).Next = null;
                (*header).Size = _pageSize;
                (*header).Used = 0;
                return header;
            }

            public BufferHeader* Alloc(int size)
            {
                if (size == _pageSize)
                    return Alloc();

                BufferHeader* header = (BufferHeader*)Marshal.AllocHGlobal(size + BufferHeader.BufferHeaderSize);
                (*header).Next = null;
                (*header).Size = size;
                (*header).Used = 0;
                return header;
            }

            public void Dealloc(BufferHeader* header)
            {
                var rightThread = Thread.CurrentThread.ManagedThreadId == _threadID;
                do
                {
                    var next = (*header).Next;

                    if (rightThread && _pool.Count < _capacity && _pageSize == (*header).Size)
                    {
                        _pool.Enqueue((IntPtr)header);
                        (*header).Used = 0;
                        (*header).Next = null;
                    }
                    else
                        Marshal.FreeHGlobal((IntPtr)header);

                    header = next;
                } while (header != null);
            }
            
        }

        static class ThreadBufferPool
        {
            [ThreadStatic]
            static BufferPool _pool;

            public static BufferPool Pool
            {
                get
                {
                    if (_pool != null)
                        return _pool;
                    return _pool = new BufferPool(4096 - BufferHeader.BufferHeaderSize, 10);
                }
            }
        }

        #endregion

        public NanomsgWriteStream(NanomsgSocketBase socket)
        {
            _socket = socket;
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
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get
            {
                return _length;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void WriteByte(byte value)
        {
            EnsureCapacity();
            var data = BufferHeader.Data(_current) + (*_current).Used;
            (*data) = value;
            ++(*_current).Used;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var initialCount = count;
            fixed (byte* src = buffer)
                while (true)
                {
                    int capacity = EnsureCapacity();
                    int toCopy = Math.Min(count, capacity);
                    MemoryUtils.CopyMemory(src + offset, BufferHeader.Data(_current) + (*_current).Used, toCopy);
                    (*_current).Used += toCopy;
                    count -= toCopy;
                    if (count == 0)
                        break;
                    offset += toCopy;
                }
            _length += initialCount;
        }

        public byte[] FullBuffer()
        {
            var data = new byte[_length];
            var page = _first;
            int position = 0;
            fixed (byte* b = data)
                while (page != null)
                {
                    var length = (*page).Used;
                    MemoryUtils.CopyMemory(BufferHeader.Data(page), b + position, length);
                    position += length;
                    page = (*page).Next;
                }

            return data;
        }

        public struct BufferResult { public int Length; public IntPtr Buffer;}

        public BufferResult FirstPage()
        {
            return new BufferResult() { Length = (*_first).Used, Buffer = (IntPtr)BufferHeader.Data(_first) };
        }

        public BufferResult NextPage(BufferResult result)
        {
            var current = (BufferHeader*)((byte*)result.Buffer - BufferHeader.BufferHeaderSize);
            var next = (*current).Next;
            if (next == null)
                return new BufferResult();

            return new BufferResult() { Length = (*next).Used, Buffer = (IntPtr)BufferHeader.Data(next) };
        }

        int EnsureCapacity()
        {
            var pool = _pool ?? (_pool = ThreadBufferPool.Pool);

            if (_current == null)
            {
                var buffer = pool.Alloc();
                _current = _first = buffer;
                return (*_current).Size;
            }

            var current = (*_current);
            if (current.Used == current.Size)
            {
                var buffer = pool.Alloc();
                (*_current).Next = buffer;
                _current = buffer;
                return (*buffer).Size;
            }

            return current.Size - current.Used;
        }

        protected override void Dispose(bool disposing)
        {
            var pool = Interlocked.Exchange(ref _pool, null);
            var head = _first;
            _first = null;
            _current = null;

            if (head != null && pool != null)
                pool.Dealloc(head);

            base.Dispose(disposing);
        }

        public int PageCount
        {
            get
            {
                int i = 0;
                BufferHeader* header = this._first;
                while (header != null)
                {
                    ++i;
                    header = (*header).Next;
                }
                return i;
            }
        }

        //public 
    }
}
