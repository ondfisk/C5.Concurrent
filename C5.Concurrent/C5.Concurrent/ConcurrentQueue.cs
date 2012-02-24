using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace C5.Concurrent
{
    /// <summary>
    /// Relies heavily on:
    /// Lock Free Queue implementation in C++ and C# by Idaho Edokpayi, 24 Feb 2008 (http://www.codeproject.com/KB/cpp/lockfreeq.aspx) 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentQueue<T> : IQueue<T>, IEnumerable<T>
    {
        private Pointer _head;
        private Pointer _tail;

        public bool IsEmpty
        {
            get { return _head.Node.Next.Node == null; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public ConcurrentQueue()
        {
            // Allocate a free node
            // Make it the only node in the linked list
            var node = new Node();

            // Both Head and Tail point to it
            _head.Node = _tail.Node = node;
        }

        public void Enqueue(T item)
        {
            // Allocate a new node from the free list
            // Copy enqueued value into node
            // Set next pointer of node to NULL
            var node = new Node {Value = item};
            Pointer tail;

            // Keep trying until Enqueue is done
            while (true)
            {
                // Read Tail.ptr and Tail.count together
                tail = _tail;

                // Read next ptr and count fields together
                var next = tail.Node.Next;

                // Are tail and next consistent?
                if (tail.Count == _tail.Count && tail.Node == _tail.Node)
                {
                    // Was Tail pointing to the last node?
                    if (next.Node == null)
                    {
                        // Try to link node at the end of the linked list
                        if (CompareAndSwap(ref tail.Node.Next, next, new Pointer(node, next.Count + 1)))
                        {
                            // Enqueue is done. Exit loop
                            break;
                        }
                    }
                    // Tail was not pointing to the last node
                    else
                    {
                        // Try to swing Tail to the next node
                        CompareAndSwap(ref _tail, tail, new Pointer(next.Node, tail.Count + 1));
                    }
                }
            }
            // Enqueue is done. Try to swing Tail to the inserted node
            CompareAndSwap(ref _tail, tail, new Pointer(node, tail.Count + 1));
        }

        public Option<T> Dequeue()
        {
            T item;

            // Keep trying until Dequeue is done
            while (true)
            {
                // Read Head
                var head = _head;

                // Read Tail
                var tail = _tail;

                // Read Head.ptr–>next
                var next = head.Node.Next;

                // Are head, tail, and next consistent?
                if (head.Count == _head.Count && head.Node == _head.Node)
                {
                    // Is queue empty or Tail falling behind?
                    if (head.Node == tail.Node)
                    {
                        // Is queue empty?
                        if (next.Node == null)
                        {
                            // Queue is empty, couldn’t dequeue
                            return Option<T>.None;
                        }

                        // Tail is falling behind. Try to advance it
                        CompareAndSwap(ref _tail, tail, new Pointer(next.Node, tail.Count + 1));
                    }
                    // No need to deal with Tail
                    else
                    {
                        // Read value before CAS, otherwise another dequeue might free the next node *pvalue = next.ptr–>value
                        item = next.Node.Value;

                        // Try to swing Head to the next node
                        if (CompareAndSwap(ref _head, head, new Pointer(next.Node, head.Count + 1)))
                        {
                            // Dequeue is done. Exit loop
                            break;
                        }
                    }
                }
            }
            // It is safe now to free the old dummy node
            // Queue was not empty, dequeue succeeded
            return Option<T>.Some(item);
        }

        public Option<T> Peek()
        {
            var node = _head.Node.Next.Node;

            if (node == null)
            {
                return Option<T>.None;
            }

            var item = node.Value;

            return Option<T>.Some(item);
        }

        public void Clear()
        {
            var node = new Node();
            _head.Node = _tail.Node = node;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerable<T> AsEnumerable()
        {
            var node = _tail.Node.Next.Node;

            while (node != null)
            {
                yield return node.Value;
                node = node.Next.Node;
            }
        }

        private static bool CompareAndSwap(ref Pointer destination, Pointer compared, Pointer exchange)
        {
            if (compared.Node == Interlocked.CompareExchange(ref destination.Node, exchange.Node, compared.Node))
            {
                Interlocked.Exchange(ref destination.Count, exchange.Count);

                return true;
            }

            return false;
        }

        private class Node
        {
            internal T Value;
            internal Pointer Next;
        }

        private struct Pointer
        {
            internal long Count;
            internal Node Node;

            internal Pointer(Node node, long count)
            {
                Node = node;
                Count = count;
            }
        }
    }
}
