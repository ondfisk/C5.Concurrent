using System;
using System.Reflection;
using NUnit.Framework;

namespace C5.Concurrent.Tests
{
    [TestFixture]
    public class OptionTests
    {
        [Test]
        public void None_returns_an_option_which_IsNone()
        {
            var none = Option<int>.None;

            Assert.IsTrue(none.IsNone);
        }

        [Test]
        public void None_returns_an_option_which_is_not_Some()
        {
            var none = Option<int>.None;

            Assert.IsFalse(none.IsSome);
        }

        [Test]
        public void None_throws_a_NullReferenceException_when_calling_Value()
        {
            var none = Option<int>.None;

            Assert.Throws<NullReferenceException>(() => { var v = none.Value; });
        }

        [Test]
        public void Parameterized_constructor_creates_some_option()
        {
            var ctor = typeof(Option<int>).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[1];

            var some = (Option<int>)ctor.Invoke(new object[] {42 });

            Assert.IsTrue(some.IsSome);
        }

        [Test]
        public void Parameterized_constructor_creates_some_option_with_value()
        {
            var ctor = typeof(Option<int>).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[1];

            var some = (Option<int>)ctor.Invoke(new object[] { 42 });

            Assert.AreEqual(42, some.Value);
        }

        [Test]
        public void Parameterless_constructor_creates_none_option()
        {
            var ctor = typeof(Option<int>).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];

            var none = (Option<int>)ctor.Invoke(new object[0]);

            Assert.IsTrue(none.IsNone);
        }

        [Test]
        public void Some_returns_an_option_which_IsSome()
        {
           var some = Option<int>.Some(42);

            Assert.IsTrue(some.IsSome);
        }

        [Test]
        public void Some_returns_an_option_which_is_not_None()
        {
            var some = Option<int>.Some(42);

            Assert.IsFalse(some.IsNone);
        }

        [Test]
        public void Some_returns_value_when_calling_Value()
        {
            var some = Option<int>.Some(42);

            Assert.AreEqual(42, some.Value);
        }
    }
}
