using NUnit.Framework;

namespace C5.Concurrent.Tests
{
    [TestFixture]
    public class SharedTests
    {
        [Test]
        public void Constructor_when_not_given_a_value_instantiates_with_null()
        {
            var obj = new Shared<object>();

            Assert.IsNull(obj.Value);
        }

        [Test]
        public void Constructor_when_given_a_value_instantiates_with_it()
        {
            var value = new object();
            var shared = new Shared<object>(value);

            var actual = shared.Value;

            Assert.AreEqual(value, actual);
        }

        [Test]
        public void ToString_returns_a_string_representation_of_the_object()
        {
            var shared = new Shared<object>();

            const string expected = "Shared<System.Object>";

            Assert.AreEqual(expected, shared.ToString());
        }
    }
}
