using Godot;
using NUnit.Framework;
using System;

namespace CorruptedVirtues.Tests.NUnitTests
{
    [TestFixture]
    public class NUnitTestExample
    {
        [Test]
        public void SimpleAddition()
        {
            int result = Add(5, 5);
            Assert.That(result, Is.EqualTo(10));
        }

        private int Add(int a, int b)
        {
            return a + b;
        }
    }
}