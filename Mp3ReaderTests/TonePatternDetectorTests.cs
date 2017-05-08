using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Mp3Reader;
using Mp3Reader.Interface;
using Mp3ReaderTests.Helpers;
using NAudio.Wave;
using NUnit.Framework;

namespace Mp3ReaderTests
{
    [TestFixture]
    public class TonePatternDetectorTests
    {
        private const int BufferSize = 1024;
        private IConfigurationReader _mockConfigurationReader;

        private void SetUpMockConfigurationReader(int frequency1, int frequency2)
        {
            var mock = new Mock<IConfigurationReader>();
            mock.Setup(r => r.ReadToneFrequency1()).Returns(frequency1);
            mock.Setup(r => r.ReadToneFrequency2()).Returns(frequency2);
            _mockConfigurationReader = mock.Object;
        }

        [TestCaseSource(nameof(GetPositiveTestCases))]
        public void Detected_DataContainsTonePattern_EventuallyReturnsTrue(string uri, int expectedTimestampInSeconds,
            int frequency1, int frequency2)
        {
            SetUpMockConfigurationReader(frequency1, frequency2);
            var stream = EmbeddedResourceReader.GetStream(uri);

            using (var reader = new Mp3FileReader(stream))
            {
                SecondsUntilPatternConcluded(reader).Should().Be(expectedTimestampInSeconds);
            }
        }

        private static IEnumerable<TestCaseData> GetPositiveTestCases()
        {
            return new List<TestCaseData>
            {
                new TestCaseData("Mp3ReaderTests.TestMp3Files.WithTonePattern.mp3", 2, 947, 1270),
                new TestCaseData("Mp3ReaderTests.TestMp3Files.WithTonePattern2.mp3", 30, 947, 1270),
                new TestCaseData("Mp3ReaderTests.TestMp3Files.With968-1270Pattern.mp3", 38, 968, 1270)
            };
        }

        private int SecondsUntilPatternConcluded(IWaveProvider reader)
        {
            var sampleProvider = reader.ToSampleProvider();
            var toneDetector = new TonePatternDetector(_mockConfigurationReader);
            var buffer = new float[BufferSize];
            long sampleCount = 0;

            while (true)
            {
                var bytesRead = sampleProvider.Read(buffer, 0, buffer.Length);
                sampleCount += bytesRead;

                if (bytesRead < buffer.Length) break;

                if (toneDetector.Detected(buffer, sampleProvider.WaveFormat.SampleRate))
                {
                    return TimeStampHelper.GetElapsedSeconds(sampleProvider.WaveFormat.SampleRate, sampleCount);
                }
            }

            return -1;
        }

        [TestCaseSource(nameof(GetNegativeTestCases))]
        public void Detected_DataDoesNotContainTargetPattern_AlwaysReturnsFalse(string uri, int frequency1,
            int frequency2)
        {
            SetUpMockConfigurationReader(frequency1, frequency2);
            var stream = EmbeddedResourceReader.GetStream(uri);

            using (var reader = new Mp3FileReader(stream))
            {
                SecondsUntilPatternConcluded(reader).Should().Be(-1);
            }
        }

        private static IEnumerable<TestCaseData> GetNegativeTestCases()
        {
            return new List<TestCaseData>
            {
                new TestCaseData("Mp3ReaderTests.TestMp3Files.StaticOnly.mp3", 947, 1270),
                new TestCaseData("Mp3ReaderTests.TestMp3Files.SpeechWithoutTonePattern.mp3", 947, 1270),
                new TestCaseData("Mp3ReaderTests.TestMp3Files.WithFrequenciesInSequenceButNotTargetPattern.mp3", 947, 1270)
            };
        }
    }
}
