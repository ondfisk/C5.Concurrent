using NUnit.Framework;

namespace C5.Concurrent.Tests
{
    [TestFixture]
    public class AtomicReferenceTests
    {
        [Test]
        public void Constructor_when_not_given_a_value_instantiates_with_null()
        {
            var obj = new AtomicReference<object>();

            Assert.IsNull(obj.Value);
        }

        [Test]
        public void Constructor_when_given_a_value_instantiates_with_it()
        {
            var value = new object();
            var obj = new AtomicReference<object>(value);

            var actual = obj.Value;

            Assert.AreEqual(value, actual);
        }

        [Test]
        public void ToString_returns_a_string_representation_of_the_object()
        {
            var shared = new AtomicReference<object>();

            const string expected = "AtomicReference<System.Object>";

            Assert.AreEqual(expected, shared.ToString());
        }

        [Test]
        public void GetAndSet_returns_old_value()
        {
            var value = new object();
            var obj = new AtomicReference<object>(value);

            var actual = obj.GetAndSet(new object());

            Assert.AreEqual(value, actual);
        }

        [Test]
        public void GetAndSet_sets_new_value()
        {
            var obj = new AtomicReference<object>();
            var value = new object();

            obj.GetAndSet(value);

            Assert.AreEqual(value, obj.Value);
        }

        [Test]
        public void CompareAndSet_when_not_equal_returns_false()
        {
            var obj = new AtomicReference<object>();

            Assert.IsFalse(obj.CompareAndSet(new object(), new object()));
        }

        [Test]
        public void CompareAndSet_when_equal_returns_true()
        {
            var expected = new object();
            var obj = new AtomicReference<object>(expected);

            Assert.IsTrue(obj.CompareAndSet(expected, new object()));
        }

        [Test]
        public void CompareAndSet_when_not_equal_does_not_set_value()
        {
            var initial = new object();
            var obj = new AtomicReference<object>(initial);

            obj.CompareAndSet(new object(), new object());

            Assert.AreEqual(initial, obj.Value);
        }

        [Test]
        public void CompareAndSet_when_equal_sets_value()
        {
            var oldValue = new object();
            var obj = new AtomicReference<object>(oldValue);
            var newValue = new object();

            obj.CompareAndSet(oldValue, newValue);

            Assert.AreEqual(newValue, obj.Value);
        }
    }
}
