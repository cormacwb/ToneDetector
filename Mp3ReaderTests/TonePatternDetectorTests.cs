using System.IO;
using System.Reflection;
using FluentAssertions;
using Mp3Reader;
using NAudio.Wave;
using NUnit.Framework;

namespace Mp3ReaderTests
{
    [TestFixture]
    public class TonePatternDetectorTests
    {
        private const int BufferSize = 1024;
        private const int TargetFrequency1 = 947;
        private const int TargetFrequency2 = 1270;

        [TestCase("Mp3ReaderTests.TestMp3Files.WithTonePattern.mp3", 2)]
        [TestCase("Mp3ReaderTests.TestMp3Files.WithTonePattern2.mp3", 30)]
        public void Detected_DataContainsTonePattern_EventuallyReturnsTrue(string uri, int expectedTimestampInSeconds)
        {
            var stream = GetEmbeddedResourceStream(uri);
            
            using (var reader = new Mp3FileReader(stream))
            {
                SecondsUntilPatternConcluded(reader).Should().Be(expectedTimestampInSeconds);
            }
        }

        private static int SecondsUntilPatternConcluded(IWaveProvider reader)
        {
            var sampleProvider = reader.ToSampleProvider();
            var toneDetector = new TonePatternDetector(TargetFrequency1, TargetFrequency2,
                sampleProvider.WaveFormat.SampleRate);
            var buffer = new float[BufferSize];
            long sampleCount = 0;

            while (true)
            {
                var bytesRead = sampleProvider.Read(buffer, 0, buffer.Length);
                sampleCount += bytesRead;

                if (bytesRead < buffer.Length) break;

                if (toneDetector.Detected(buffer))
                {
                    return GetElapsedSeconds(sampleProvider.WaveFormat.SampleRate, sampleCount);
                }
            }

            return -1;
        }

        private static int GetElapsedSeconds(int sampleRate, long sampleCount)
        {
            return (int)(sampleCount / sampleRate);
        }

        private static Stream GetEmbeddedResourceStream(string resourceName)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        }

        [TestCase("Mp3ReaderTests.TestMp3Files.StaticOnly.mp3")]
        [TestCase("Mp3ReaderTests.TestMp3Files.SpeechWithoutTonePattern.mp3")]
        public void Detected_DataDoesNotContainTargetPattern_AlwaysReturnsFalse(string uri)
        {
            var stream = GetEmbeddedResourceStream(uri);

            using (var reader = new Mp3FileReader(stream))
            {
                SecondsUntilPatternConcluded(reader).Should().Be(-1);
            }
        }
    }
}
