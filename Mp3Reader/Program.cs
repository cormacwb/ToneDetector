using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace Mp3Reader
{
    public class Program
    {
        private const int BufferSize = 1024;
        private const string DefaultPath = @"C:\Users\Cormac\Music\RadioSure Recordings\Vancouver dispatch\MultipleDispatches.mp3";
        private const int TargetFrequency1 = 947;
        private const int TargetFrequency2 = 1270;

        static void Main(string[] args)
        {
            var path = args.Any() ? args[0] : DefaultPath;

            long sampleCount = 0;
            
            using (var reader = new Mp3FileReader(path))
            {
                
                var sampleProvider = reader.ToSampleProvider();
                var buffer = new float[BufferSize];

                var toneDetector = new TonePatternDetector(TargetFrequency1, TargetFrequency2, sampleProvider.WaveFormat.SampleRate);
                
                var recorders = new List<DispatchMessageRecorder>();
                
                while (true)
                {
                    var bytesRead = sampleProvider.Read(buffer, 0, buffer.Length);
                    sampleCount += bytesRead;

                    if (EndOfSamples(bytesRead, buffer)) break;
                    
                    if (toneDetector.Detected(buffer))
                    {
                        recorders.Add(new DispatchMessageRecorder(reader.WaveFormat));
                        toneDetector.Reset();
                    }

                    foreach (var recorder in recorders)
                    {
                        recorder.Record(buffer, bytesRead);
                    }

                    recorders = RefreshRecorderList(recorders).ToList();
                }

                RefreshRecorderList(recorders);


                Console.WriteLine("Complete");
            }
        }

        private static IEnumerable<DispatchMessageRecorder> RefreshRecorderList(IEnumerable<DispatchMessageRecorder> recorders)
        {
            foreach (var recorder in recorders)
            {
                if (recorder.IsFinishedRecording)
                {
                    recorder.Dispose();
                }
                else
                {
                    yield return recorder;
                }
            }
        }

        private static string GetTimestamp(long sampleCount)
        {
            const int samplesPerSecond = 22050;

            var seconds = sampleCount/samplesPerSecond;

            var minutes = seconds/60;
            var secondsOnly = seconds%60;

            return $"{minutes}:{secondsOnly}";
        }

        private static bool EndOfSamples(int bytesRead, IReadOnlyCollection<float> buffer)
        {
            return bytesRead < buffer.Count;
        }
    }
}
