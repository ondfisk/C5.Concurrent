
namespace C5.Concurrent
{
    public interface IQueue<T>
    {
        bool IsEmpty { get; }
        void Enqueue(T item);
        Option<T> Dequeue();
        Option<T> Peek();
        void Clear();
    }
}
