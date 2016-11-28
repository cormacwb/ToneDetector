using System.IO;
using FluentAssertions;
using Mp3Reader;
using Mp3ReaderTests.Helpers;
using NAudio.Wave;
using NUnit.Framework;

namespace Mp3ReaderTests
{
    [TestFixture]
    public class DispatchMessageRecorderTests
    {
        private const int BufferSize = 1024;
        private DispatchMessageRecorder _recorder;

        [TearDown]
        public void TearDown()
        {
            File.Delete(_recorder.FileName);
            _recorder.Dispose();
        }
        
        [TestCase("Mp3ReaderTests.TestMp3Files.WithTonePattern.mp3", 25)]
        public void Record_RecordsUntilSilence(string uri, int expectedFinishingTimeInSeconds)
        {
            var stream = EmbeddedResourceReader.GetStream(uri);

            using (var reader = new Mp3FileReader(stream))
            {
                var sampleProvider = reader.ToSampleProvider();
                _recorder = new DispatchMessageRecorder(sampleProvider.WaveFormat);
                var buffer = new float[BufferSize];
                long sampleCount = 0;

                while (true)
                {
                    var bytesRead = sampleProvider.Read(buffer, 0, buffer.Length);
                    sampleCount += bytesRead;

                    if (bytesRead < buffer.Length) break;

                    _recorder.Record(buffer, bytesRead);

                    if (_recorder.IsFinishedRecording)
                    {
                        var finishingTime = TimeStampHelper.GetElapsedSeconds(sampleProvider.WaveFormat.SampleRate, sampleCount);

                        finishingTime.Should().Be(expectedFinishingTimeInSeconds);
                        return;
                    }
                }

                Assert.Fail("Failed to detect end of message");
            }
        }
    }
}
