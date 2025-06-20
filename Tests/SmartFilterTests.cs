using Operating_Timer.Components;
using Xunit;

namespace OperatingTimer.Tests
{
    public class SmartFilterTests
    {
        [Fact]
        public void Feed_ValidData_AveragesComputedCorrectly()
        {
            var filter = new SmartFilter(bufferSize: 5);
            filter.Feed(20f, 50f);
            filter.Feed(22f, 52f);
            filter.Feed(21f, 51f);

            Assert.True(filter.IsValid());
            Assert.Equal(21f, filter.GetAverageTemp(), 1);
            Assert.Equal(51f, filter.GetAverageRh(), 1);
        }

        [Fact]
        public void Feed_InvalidData_IgnoredInAverage()
        {
            var filter = new SmartFilter(bufferSize: 5);
            filter.Feed(20f, 50f);
            // large outlier should be marked invalid and not update last valid/avg
            for (int i = 0; i < 5; i++)
            {
                filter.Feed(50f, 90f);
            }
            filter.Feed(21f, 51f);

            Assert.True(filter.IsValid());
            // average should consider 20 and 21 only when bufferSize 5 but invalid removed by median check
            Assert.InRange(filter.GetAverageTemp(), 20f, 21f);
            Assert.InRange(filter.GetAverageRh(), 50f, 51f);
        }

        [Fact]
        public void Reset_ClearsState()
        {
            var filter = new SmartFilter();
            filter.Feed(20f, 50f);
            filter.Reset();

            Assert.False(filter.IsValid());
            Assert.Equal(-1f, filter.GetAverageTemp());
            Assert.Equal(-1f, filter.GetAverageRh());
        }

        [Fact]
        public void Feed_PhysicallyImpossibleData_DoesNotChangeState()
        {
            var filter = new SmartFilter();
            filter.Feed(20f, 50f);
            filter.Feed(-10f, 200f); // impossible

            Assert.True(filter.IsValid());
            Assert.InRange(filter.GetAverageTemp(), 20f, 20f);
            Assert.InRange(filter.GetAverageRh(), 50f, 50f);
        }
    }
}
