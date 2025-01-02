using GdUnit4;
using static GdUnit4.Assertions;

namespace CorruptedVirtues.Tests.GDUnitTests
{
    [TestSuite]
    public class GDUnitExmapleTest
    {
        [TestCase]
        public void StringToLower()
        {
            AssertString("AbcD".ToLower()).IsEqual("abcd");
        }
    }
}