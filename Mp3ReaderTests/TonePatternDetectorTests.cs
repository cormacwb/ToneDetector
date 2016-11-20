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

        [TestCase("Mp3ReaderTests.TestMp3Files.WithTonePattern.mp3")]
        [TestCase("Mp3ReaderTests.TestMp3Files.WithTonePattern2.mp3")]
        public void Detected_DataContainsTonePattern_EventuallyReturnsTrue(string uri)
        {
            var stream = GetEmbeddedResourceStream(uri);
            
            using (var reader = new Mp3FileReader(stream))
            {
                TonePatternDetected(reader).Should().BeTrue();
            }
        }

        private static bool TonePatternDetected(IWaveProvider reader)
        {
            var sampleProvider = reader.ToSampleProvider();
            var toneDetector = new TonePatternDetector(TargetFrequency1, TargetFrequency2,
                sampleProvider.WaveFormat.SampleRate);
            var buffer = new float[BufferSize];

            while (true)
            {
                var bytesRead = sampleProvider.Read(buffer, 0, buffer.Length);
                if (bytesRead < buffer.Length) break;

                if (toneDetector.Detected(buffer))
                {
                    return true;
                }
            }

            return false;
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
                TonePatternDetected(reader).Should().BeFalse();
            }
        }

    }
}
