using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.Dsp;
using NAudio.Wave;

namespace Mp3Reader
{
    public class Program
    {
        private const int BufferSize = 1024;
        private const string Path = @"C:\Users\Cormac\Music\StationRipper\output\ECOMM Vancouver Scanner\tone.mp3";
        const int TargetFrequency1 = 947;

        static void Main(string[] args)
        {
            var prominentFrequencies = new List<int>();
            using (var reader = new Mp3FileReader(Path))
            {
                var sampleProvider = reader.ToSampleProvider();
                var buffer = new float[BufferSize];

                var bytesRead = 1;
                while (true)
                {
                    bytesRead = sampleProvider.Read(buffer, 0, buffer.Length);
                    
                    if (bytesRead < buffer.Length)
                        break;
                    if (buffer.All(n => Math.Abs(n) < 0.01)) continue;

                    var fft = CreateFftBuffer(buffer);

                    FastFourierTransform.FFT(true, GetLog(), fft);

                    prominentFrequencies.Add(GetMostProminentFrequency(fft, sampleProvider));
                }
            }

            var x = prominentFrequencies;
            var y = x;
        }

        private static int GetLog()
        {
            return (int)Math.Log(BufferSize, 2);
        }

        private static int GetMostProminentFrequency(Complex[] fft, ISampleProvider sampleProvider)
        {
            var indexOfMaxMagnitude = GetIndexOfMaxMagnitude(fft);
            return GetFrequency(indexOfMaxMagnitude, sampleProvider.WaveFormat.SampleRate, fft.Length);
        }

        private static int GetIndexOfMaxMagnitude(Complex[] fft)
        {
            double maxMagnitude = 0;
            int indexOfMaxMagnitude = 0;
            for (var i = 0; i < fft.Length; i++)
            {
                var magnitude = CalculateMagnitude(fft[i]);
                if (magnitude > maxMagnitude)
                {
                    indexOfMaxMagnitude = i;
                    maxMagnitude = magnitude;
                }
            }
            return indexOfMaxMagnitude;
        }

        private static int GetFrequency(int indexOfMaxMagnitude, int sampleRate, int bufferSize)
        {
            return indexOfMaxMagnitude*sampleRate/bufferSize;
        }

        private static double CalculateMagnitude(Complex complex)
        {
            return Math.Sqrt((complex.X * complex.X) + (complex.Y * complex.Y));
        }

        private static Complex[] CreateFftBuffer(float[] buffer)
        {
            var fft = new Complex[BufferSize];
            for (var i = 0; i < buffer.Length; i++)
            {
                var fftComplexInput = CreateComplexInput(buffer, i);
                fft[i] = fftComplexInput;
            }
            return fft;
        }

        private static Complex CreateComplexInput(float[] buffer, int index)
        {
            var real = CreateRealPart(buffer, index);
            const int imaginary = 0;

            return new Complex {X = real, Y = imaginary};
        }

        private static float CreateRealPart(IReadOnlyList<float> buffer, int index)
        {
            return (float) (buffer[index]*FastFourierTransform.HammingWindow(index, BufferSize));
        }

        private static void WriteIntro(WaveFileWriter writer)
        {
            var quietIntro = Enumerable.Repeat(0.2f, 20000).ToArray();

            writer.WriteSamples(quietIntro, 0, quietIntro.Length);
        }

        public static void Mp3ToWav(string mp3File, string outputFile)
        {
            using (Mp3FileReader reader = new Mp3FileReader(mp3File))
            {
                using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(reader))
                {
                    WaveFileWriter.CreateWaveFile(outputFile, pcmStream);
                }
            }
        }
    }
}
