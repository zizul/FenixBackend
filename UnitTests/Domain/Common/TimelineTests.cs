using Domain.Common;

namespace UnitTests.Domain.Common
{
    public class TimelineTests
    {
        [Fact]
        public void Count_ReturnsCorrectNumber()
        {
            var timeline = new Timeline<TestItem>();

            Assert.Equal(0, timeline.Count);

            timeline.AddEntry(new TestItem { Value = 1 });
            Assert.Equal(1, timeline.Count);

            timeline.AddEntry(new TestItem { Value = 2 });
            timeline.AddEntry(new TestItem { Value = 3 });
            Assert.Equal(3, timeline.Count);
        }

        [Fact]
        public void Get_ReturnsCorrectItem()
        {
            var timeline = new Timeline<TestItem>();
            timeline.AddEntry(new TestItem { Value = 1 });
            timeline.AddEntry(new TestItem { Value = 2 });
            timeline.AddEntry(new TestItem { Value = 3 });

            Assert.Equal(1, timeline.Get(0).Value);
            Assert.Equal(2, timeline.Get(1).Value);
            Assert.Equal(3, timeline.Get(2).Value);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(1)]  // Equal to Count
        [InlineData(2)]  // Greater than Count
        public void Get_ThrowsArgumentOutOfRangeException(int index)
        {
            var timeline = new Timeline<TestItem>();
            timeline.AddEntry(new TestItem { Value = 1 });

            Assert.Throws<ArgumentOutOfRangeException>(() => timeline.Get(index));
        }


        [Fact]
        public void AddEntry_WithItem_AddsToEntries()
        {
            var timeline = new Timeline<TestItem>();
            var item = new TestItem { Value = 1 };

            var result = timeline.AddEntry(item);

            Assert.Single(timeline.Entries);
            Assert.Same(item, result);
            Assert.Same(item, timeline.Entries[0]);
        }

        [Fact]
        public void AddEntry_WithoutItem_CreatesNewItemWhenEmpty()
        {
            var timeline = new Timeline<TestItem>();

            var result = timeline.AddEntry();

            Assert.Single(timeline.Entries);
            Assert.Equal(0, result.Value);
        }

        [Fact]
        public void AddEntry_WithoutItem_ClonesLastItemWhenNotEmpty()
        {
            var timeline = new Timeline<TestItem>();
            timeline.AddEntry(new TestItem { Value = 5 });

            var result = timeline.AddEntry();

            Assert.Equal(2, timeline.Entries.Count);
            Assert.Equal(5, result.Value);
            Assert.NotSame(timeline.Entries[0], timeline.Entries[1]);
        }

        [Fact]
        public void Last_ReturnsLastEntry()
        {
            var timeline = new Timeline<TestItem>();
            timeline.AddEntry(new TestItem { Value = 1 });
            timeline.AddEntry(new TestItem { Value = 2 });

            var result = timeline.Last();

            Assert.Equal(2, result.Value);
        }

        [Fact]
        public void Last_ThrowsWhenEmpty()
        {
            var timeline = new Timeline<TestItem>();

            Assert.Throws<InvalidOperationException>(() => timeline.Last());
        }
    }

    public class TestItem : ICloneable
    {
        public int Value { get; set; }

        public object Clone()
        {
            return new TestItem { Value = Value };
        }
    }
}