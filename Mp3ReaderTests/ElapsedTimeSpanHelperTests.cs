using System;
using System.Collections.Generic;
using FluentAssertions;
using Mp3Reader;
using NUnit.Framework;

namespace Mp3ReaderTests
{

    [TestFixture]
    public class ElapsedTimeSpanHelperTests
    {
        [TestCase(-20)]
        [TestCase(-5)]
        [TestCase(-0)]
        public void GetElapsedTimeSpan_NegativeOrZeroSampeRate_Throws(int invalidSampleRate)
        {
            const int testSampleCount = 1024;

            Action testCall = () => ElapsedTimeSpanHelper.GetElapsedTimeSpan(invalidSampleRate, testSampleCount);

            testCall.ShouldThrow<ArgumentException>();
        }

        [TestCase(-20)]
        [TestCase(-5)]
        [TestCase(-0)]
        public void GetElapsedTimeSpan_NegativeOrZeroElapseSampleCount_Throws(int invalidElapsedSampleCount)
        {
            const int testSampleRate = 22050;

            Action testCall = () => ElapsedTimeSpanHelper.GetElapsedTimeSpan(testSampleRate, invalidElapsedSampleCount);

            testCall.ShouldThrow<ArgumentException>();
        }

        [TestCaseSource(nameof(GetValidTestCases))]
        public void GetElapsedTimeSpan_ValidSampleCountAndSampleRate_ReturnsCorrectValue(int sampleRate,
            int elapsedSampleCount, TimeSpan expectedResult)
        {
            var result = ElapsedTimeSpanHelper.GetElapsedTimeSpan(sampleRate, elapsedSampleCount);

            result.Should().Be(expectedResult);
        }

        public static List<TestCaseData> GetValidTestCases => new List<TestCaseData>
        {
            new TestCaseData(22050, 500, TimeSpan.FromMilliseconds(23)),
            new TestCaseData(44100, 44100, TimeSpan.FromSeconds(1)),
            new TestCaseData(88200, 441000, TimeSpan.FromSeconds(5))
        };
    }
}
