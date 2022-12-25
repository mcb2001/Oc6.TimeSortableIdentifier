using FluentAssertions.Collections;
using Xunit.Sdk;

namespace Oc6.TimeSortableIdentifier.Tests
{
    internal static class EnumerableExtensions
    {
        internal static AndConstraint<SubsequentOrderingAssertions<long>> PassDefaultTests(this GenericCollectionAssertions<long> assertions, int count)
            => assertions.BeInAscendingOrder()
                .And
                .HaveCount(count)
                .And
                .OnlyHaveUniqueItems()
                .And
                .AllSatisfy(value => value.Should().BePositive());
    }

    public class TsidTests
    {
        private const string StringZero = "0000-0000-0000-0000";
        private const int TsidZero = 0;

        private const string StringOne = "0000-0000-0000-0001";
        private const int TsidOne = 1;

        private const int TsidLow = 0xCDEF;
        private const string StringLow = "0000-0000-0000-CDEF";

        private const string StringHigh = "0123-4567-89AB-CDEF";
        private const long TsidHigh = 0x0123456789ABCDEF;

        private const string StringAlmostMaxValue = "7FFF-FFFF-FFFF-FFFE";
        private const long TsidAlmostMaxValue = long.MaxValue - TsidOne;

        private const string StringMaxValue = "7FFF-FFFF-FFFF-FFFF";
        private const long TsidMaxValue = long.MaxValue;

        [Fact]
        public void Test_Create_None_Should_Be_Negative()
        {
            int count = byte.MaxValue;

            Enumerable
                .Range(0, count)
                .Select(_ => Tsid.Create())
                .Should()
                .PassDefaultTests(count);
        }

        [Fact]
        public void Test_Create_Validate_Sortable_Guarenteed_Up_To_Byte_MaxValue()
        {
            int count = byte.MaxValue;

            List<long> unsorted = Enumerable
                .Range(TsidZero, count)
                .Select(_ => Tsid.Create())
                .ToList();

            unsorted
                .Should()
                .PassDefaultTests(count)
                .And
                .Equal(unsorted.OrderBy(x => x).ToList());
        }

        [Fact]
        public void Test_Create_Validate_Sortable_Guarenteed_Up_To_Byte_MaxValue_MultipleTimes()
        {
            int count = byte.MaxValue * 10;

            List<long> unsorted = Enumerable
                .Range(TsidZero, count)
                .Select(value =>
                {
                    if (value % byte.MaxValue == 0)
                    {
                        Thread.Sleep(100);
                    }

                    return Tsid.Create();
                })
                .ToList();

            unsorted.Should()
                .PassDefaultTests(count)
                .And
                .Equal(unsorted.OrderBy(x => x).ToList());
        }

        [Theory]
        [InlineData(StringZero, TsidZero)]
        [InlineData(StringOne, TsidOne)]
        [InlineData(StringLow, TsidLow)]
        [InlineData(StringHigh, TsidHigh)]
        [InlineData(StringAlmostMaxValue, TsidAlmostMaxValue)]
        [InlineData(StringMaxValue, TsidMaxValue)]
        public void Test_ToString(string expected, long value)
        {
            string actual = Tsid.ToString(value);
            expected.Should().Be(actual);
        }

        [Theory]
        [InlineData(TsidZero, StringZero)]
        [InlineData(TsidOne, StringOne)]
        [InlineData(TsidLow, StringLow)]
        [InlineData(TsidHigh, StringHigh)]
        [InlineData(TsidAlmostMaxValue, StringAlmostMaxValue)]
        [InlineData(TsidMaxValue, StringMaxValue)]
        public void Test_TryParse(long expected, string value)
        {
            Tsid.TryParse(value, out long actual)
                .Should()
                .Be(true);

            expected
                .Should()
                .Be(actual);
        }

        [Theory]
        [InlineData(TsidZero, StringZero)]
        [InlineData(TsidOne, StringOne)]
        [InlineData(TsidLow, StringLow)]
        [InlineData(TsidHigh, StringHigh)]
        [InlineData(TsidAlmostMaxValue, StringAlmostMaxValue)]
        [InlineData(TsidMaxValue, StringMaxValue)]
        public void Test_TryParse_LowerCase(long expected, string value)
        {
            Tsid.TryParse(value.ToLowerInvariant(), out long actual)
                .Should()
                .Be(true);

            expected
                .Should()
                .Be(actual);
        }

        [Theory]
        [InlineData(TsidZero, StringZero)]
        [InlineData(TsidOne, StringOne)]
        [InlineData(TsidLow, StringLow)]
        [InlineData(TsidHigh, StringHigh)]
        [InlineData(TsidAlmostMaxValue, StringAlmostMaxValue)]
        [InlineData(TsidMaxValue, StringMaxValue)]
        public void Test_TryParse_UpperCase(long expected, string value)
        {
            Tsid.TryParse(value.ToUpperInvariant(), out long actual)
                .Should()
                .Be(true);

            expected
                .Should()
                .Be(actual);
        }

        [Theory]
        [InlineData(TsidLow, "0000-0000-0000-CdEf")]
        [InlineData(TsidHigh, "0123-4567-89Ab-CdEf")]
        public void Test_TryParse_MixedCase(long expected, string value)
        {
            Tsid.TryParse(value.ToUpperInvariant(), out long actual)
                .Should()
                .Be(true);

            expected
                .Should()
                .Be(actual);
        }

        [Theory]
        [InlineData(TsidZero)]
        [InlineData(TsidOne)]
        [InlineData(TsidLow)]
        [InlineData(TsidHigh)]
        [InlineData(TsidAlmostMaxValue)]
        [InlineData(TsidMaxValue)]
        public void Test_TryParse_ToString(long expected)
        {
            Tsid.TryParse(Tsid.ToString(expected), out long actual)
                .Should()
                .Be(true);

            expected.Should()
                .Be(actual);
        }

        [Theory]
        [InlineData(-TsidOne)]
        [InlineData(-TsidLow)]
        [InlineData(-TsidHigh)]
        [InlineData(-TsidAlmostMaxValue)]
        [InlineData(-TsidMaxValue)]
        [InlineData(long.MinValue)]
        public void Test_ToShortString_Exception(long value)
        {
            Action fail = () => Tsid.ToString(value);
            fail.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData("")] //empty
        [InlineData("0")] //zero
        [InlineData("0000")] //long zero
        [InlineData("0000-0000-0000")] //too few groups
        [InlineData($"{StringZero}-0000")] //too many groups

        [InlineData($" {StringZero}")] //starts with space
        [InlineData($"\t{StringZero}")] //starts with tab
        [InlineData($"\n {StringZero}")] //starts with unix newline
        [InlineData($"\r\n {StringZero}")] //starts with windows newline

        [InlineData($"0000-0000 0000-0000")] //space in the middle
        [InlineData($"0000-0000\t0000-0000")] //tab in the middle
        [InlineData($"0000-0000\n0000-0000")] //unix newline in the middle
        [InlineData($"0000-0000\r\n0000-0000")] //windows newline in the middle

        [InlineData($"{StringZero} ")] //ends with space
        [InlineData($"{StringZero}\t")] //ends with tab
        [InlineData($"{StringZero}\n")] //ends with unix newline
        [InlineData($"{StringZero}\r\n")] //ends with windows newline

        [InlineData("8000-0000-0000-0000")] //negative (long.MinValue)
        [InlineData("FFFF-FFFF-FFFF-FFFF")] //negative (-1)

        [InlineData("0000-0000-0000-000N")] //NaN

        [InlineData("01234567890123456789")] //Too long
        public void Test_TryParse_False(string value)
        {
            Tsid.TryParse(value, out long _)
                .Should()
                .Be(false);
        }
    }
}