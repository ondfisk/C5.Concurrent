// Type: System.Collections.Concurrent.ConcurrentQueue`1
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace C5.Concurrent
{
    [ComVisible(false)]
    [DebuggerDisplay("Count = {Count}")]
    //[DebuggerTypeProxy(typeof(SystemCollectionsConcurrent_ProducerConsumerCollectionDebugView<>))]
    [Serializable]
    [HostProtection(SecurityAction.LinkDemand, ExternalThreading = true, Synchronization = true)]
    public class SystemCollectionsConcurrentConcurrentQueue<T> : IProducerConsumerCollection<T>
    {
        [NonSerialized]
        private volatile Segment _head;

        [NonSerialized]
        private volatile Segment _tail;

        private T[] _serializationArray;

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                throw new NotSupportedException(/*Environment.GetResourceString("ConcurrentCollection_SyncRoot_NotSupported")*/);
            }
        }

        public bool IsEmpty
        {
            get
            {
                Segment segment = _head;
                if (!segment.IsEmpty)
                {
                    return false;
                }
                if (segment.Next == null)
                {
                    return true;
                }
                var spinWait = new SpinWait();
                for (; segment.IsEmpty; segment = _head)
                {
                    if (segment.Next == null)
                    {
                        return true;
                    }
                    spinWait.SpinOnce();
                }
                return false;
            }
        }

        public int Count
        {
            get
            {
                Segment head;
                Segment tail;
                int headLow;
                int tailHigh;
                GetHeadTailPositions(out head, out tail, out headLow, out tailHigh);
                if (head == tail)
                {
                    return tailHigh - headLow + 1;
                }

                return 32 - headLow + 32 * (int)(tail.Index - head.Index - 1L) + (tailHigh + 1);
            }
        }

        public SystemCollectionsConcurrentConcurrentQueue()
        {
            _head = _tail = new Segment(0L);
        }

        public SystemCollectionsConcurrentConcurrentQueue(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            InitializeFromCollection(collection);
        }

        private void InitializeFromCollection(IEnumerable<T> collection)
        {
            _head = _tail = new Segment(0L);
            int num = 0;
            foreach (T obj in collection)
            {
                _tail.UnsafeAdd(obj);
                ++num;
                if (num >= 32)
                {
                    _tail = _tail.UnsafeGrow();
                    num = 0;
                }
            }
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            _serializationArray = ToArray();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            InitializeFromCollection(_serializationArray);
            _serializationArray = null;
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            ToList().CopyTo((T[])array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            Enqueue(item);
            return true;
        }

        bool IProducerConsumerCollection<T>.TryTake(out T item)
        {
            return TryDequeue(out item);
        }

        public T[] ToArray()
        {
            return ToList().ToArray();
        }

        private List<T> ToList()
        {
            Segment head;
            Segment tail;
            int headLow;
            int tailHigh;
            GetHeadTailPositions(out head, out tail, out headLow, out tailHigh);
            if (head == tail)
            {
                return head.ToList(headLow, tailHigh);
            }
            var list = new List<T>(head.ToList(headLow, 31));
            for (var next = head.Next; next != tail; next = next.Next)
            {
                list.AddRange(next.ToList(0, 31));
            }
            list.AddRange(tail.ToList(0, tailHigh));
            return list;
        }

        private void GetHeadTailPositions(out Segment head, out Segment tail, out int headLow, out int tailHigh)
        {
            head = _head;
            tail = _tail;
            headLow = head.Low;
            tailHigh = tail.High;
            var spinWait = new SpinWait();
            while (head != _head || tail != _tail || (headLow != head.Low || tailHigh != tail.High) || head.Index > tail.Index)
            {
                spinWait.SpinOnce();
                head = _head;
                tail = _tail;
                headLow = head.Low;
                tailHigh = tail.High;
            }
        }

        public void CopyTo(T[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            ToList().CopyTo(array, index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ToList().GetEnumerator();
        }

        public void Enqueue(T item)
        {
            var spinWait = new SpinWait();
            while (!_tail.TryAppend(item, ref _tail))
            {
                spinWait.SpinOnce();
            }
        }

        public bool TryDequeue(out T result)
        {
            while (!IsEmpty)
            {
                if (_head.TryRemove(out result, ref _head))
                    return true;
            }
            result = default(T);
            return false;
        }

        public bool TryPeek(out T result)
        {
            while (!IsEmpty)
            {
                if (_head.TryPeek(out result))
                {
                    return true;
                }
            }
            result = default(T);
            return false;
        }

        private class Segment
        {
            private volatile T[] _array;
            private volatile int[] _state;
            private volatile Segment _next;
            internal readonly long Index;
            private volatile int _low;
            private volatile int _high;

            internal Segment Next
            {
                get
                {
                    return _next;
                }
            }

            internal bool IsEmpty
            {
                get
                {
                    return Low > High;
                }
            }

            internal int Low
            {
                get
                {
                    return Math.Min(_low, 32);
                }
            }

            internal int High
            {
                get
                {
                    return Math.Min(_high, 31);
                }
            }

            internal Segment(long index)
            {
                _array = new T[32];
                _state = new int[32];
                _high = -1;
                Index = index;
            }

            internal void UnsafeAdd(T value)
            {
                ++_high;
                _array[_high] = value;
                _state[_high] = 1;
            }

            internal Segment UnsafeGrow()
            {
                var segment = new Segment(Index + 1L);
                _next = segment;
                return segment;
            }

            internal void Grow(ref Segment tail)
            {
                _next = new Segment(Index + 1L);
                tail = _next;
            }

            internal bool TryAppend(T value, ref Segment tail)
            {
                if (_high >= 31)
                {
                    return false;
                }
                int index;
                try
                {
                }
                finally
                {
                    index = Interlocked.Increment(ref _high);
                    if (index <= 31)
                    {
                        _array[index] = value;
                        _state[index] = 1;
                    }
                    if (index == 31)
                    {
                        Grow(ref tail);
                    }
                }
                return index <= 31;
            }

            internal bool TryRemove(out T result, ref Segment head)
            {
                var spinWait1 = new SpinWait();
                var low = Low;
                for (var high = High; low <= high; high = High)
                {
                    if (Interlocked.CompareExchange(ref _low, low + 1, low) == low)
                    {
                        var spinWait2 = new SpinWait();
                        while (_state[low] == 0)
                        {
                            spinWait2.SpinOnce();
                        }
                        result = _array[low];
                        if (low + 1 >= 32)
                        {
                            var spinWait3 = new SpinWait();
                            while (_next == null)
                            {
                                spinWait3.SpinOnce();
                            }
                            head = _next;
                        }
                        return true;
                    }
                    spinWait1.SpinOnce();
                    low = Low;
                }
                result = default(T);
                return false;
            }

            internal bool TryPeek(out T result)
            {
                result = default(T);
                var low = Low;
                if (low > High)
                {
                    return false;
                }
                var spinWait = new SpinWait();
                while (_state[low] == 0)
                {
                    spinWait.SpinOnce();
                }
                result = _array[low];
                return true;
            }

            internal List<T> ToList(int start, int end)
            {
                var list = new List<T>();
                for (int index = start; index <= end; ++index)
                {
                    var spinWait = new SpinWait();
                    while (_state[index] == 0)
                    {
                        spinWait.SpinOnce();
                    }
                    list.Add(_array[index]);
                }
                return list;
            }
        }
    }
}
