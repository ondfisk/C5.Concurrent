namespace C5.Concurrent
{
    public class Queue<T> : IQueue<T>
    {
        private Node _head;
        private Node _tail;

        public bool IsEmpty
        {
            get { return _head == null; }
        }

        public void Enqueue(T item)
        {
            if (IsEmpty)
            {
                _head = _tail = new Node(item);
            }
            else
            {
                _tail = _tail.Next = new Node(item);
            }
        }

        public Option<T> Dequeue()
        {
            if (IsEmpty)
            {
                return Option<T>.None;
            }

            var item = _head.Value;

            _head = _head.Next;

            return Option<T>.Some(item);
        }

        public Option<T> Peek()
        {
            if (IsEmpty)
            {
                return Option<T>.None;
            }

            var item = _head.Value;

            return Option<T>.Some(item);
        }

        private class Node
        {
            internal T Value { get; private set; }
            internal Node Next { get; set; }

            internal Node(T item)
            {
                Value = item;
            }
        }

        public void Clear()
        {
            _head = _tail = null;
        }
    }
}