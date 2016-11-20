using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace Mp3Reader
{
    public class Program
    {
        private const int BufferSize = 1024;
        private const string DefaultPath = @"C:\Users\Cormac\Music\StationRipper\output\ECOMM Vancouver Scanner\tone.mp3";
        private const int TargetFrequency1 = 947;
        private const int TargetFrequency2 = 1270;
        private const double SilenceThreshold = 0.01;

        static void Main(string[] args)
        {
            var path = args.Any() ? args[0] : DefaultPath;
            using (var reader = new Mp3FileReader(path))
            {
                var sampleProvider = reader.ToSampleProvider();
                var toneDetector = new TonePatternDetector(TargetFrequency1, TargetFrequency2,
                    sampleProvider.WaveFormat.SampleRate);
                var buffer = new float[BufferSize];

                while (true)
                {
                    var bytesRead = sampleProvider.Read(buffer, 0, buffer.Length);
                    
                    if (EndOfSamples(bytesRead, buffer)) break;
                    if (ContainsOnlySilence(buffer)) continue;

                    if (toneDetector.Detected(buffer))
                    {
                        Console.WriteLine("File contains tone pattern");
                        Environment.Exit(0);
                    }
                }

                Console.WriteLine("File does not contain tone pattern");
            }
        }

        private static bool EndOfSamples(int bytesRead, float[] buffer)
        {
            return bytesRead < buffer.Length;
        }

        private static bool ContainsOnlySilence(IEnumerable<float> buffer)
        {
            return buffer.All(n => Math.Abs(n) < SilenceThreshold);
        }
    }
}
