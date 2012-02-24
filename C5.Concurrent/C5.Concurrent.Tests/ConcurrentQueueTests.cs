using NUnit.Framework;

namespace C5.Concurrent.Tests
{
    [TestFixture]
    public class ConcurrentQueueTests
    {
        [Test]
        public void A_new_queue_IsEmpty()
        {
            var q = new ConcurrentQueue<int>();

            Assert.IsTrue(q.IsEmpty);
        }

        [Test]
        public void A_non_empty_queue_not_IsEmpty()
        {
            var q = new ConcurrentQueue<int>();

            q.Enqueue(42);

            Assert.IsFalse(q.IsEmpty);
        }

        [Test]
        public void Peek_returns_None_when_queue_is_empty()
        {
            var q = new ConcurrentQueue<int>();

            var p = q.Peek();

            Assert.IsTrue(p.IsNone);
        }

        [Test]
        public void Peek_returns_head_when_queue_is_not_empty()
        {
            var q = new ConcurrentQueue<int>();

            q.Enqueue(42);

            var p = q.Peek();

            Assert.AreEqual(42, p.Value);
        }

        [Test]
        public void Dequeue_returns_None_when_queue_IsEempty()
        {
            var q = new ConcurrentQueue<int>();

            var p = q.Dequeue();

            Assert.IsTrue(p.IsNone);
        }

        [Test]
        public void Dequeue_returns_head_when_queue_is_not_empty()
        {
            var q = new ConcurrentQueue<int>();

            q.Enqueue(42);

            var p = q.Dequeue();

            Assert.AreEqual(42, p.Value);
        }
    }
}