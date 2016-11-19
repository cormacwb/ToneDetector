using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Mp3Reader
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = @"C:\Users\Cormac\Music\StationRipper\output\ECOMM Vancouver Scanner\tone.mp3";
            var output = "outputWithoutSilence.wav";
            //Mp3ToWav(path, output);

            //return;
            float max = 0;
            const float threshold = 3.05175781E-05F;
            
            using (var reader = new Mp3FileReader(path))
            {
                //var inputFormat = reader.WaveFormat;
                var sampleProvider = reader.ToSampleProvider();
                using (var writer = new WaveFileWriter(output, sampleProvider.WaveFormat))
                {
                    WriteIntro(writer);

                    Mp3Frame frame;

                    var sig = new SignalGenerator();


                    
                    float[] buffer = new float[10000];
                    
                    while (true)
                    {
                        int bytesRead = sampleProvider.Read(buffer, 0, buffer.Length);
                        if (bytesRead <= 0)
                            return;

                        foreach (var sample in buffer)
                        {

                            if (sample > threshold)
                            {
                                writer.WriteSample(sample);
                            }
                        }
                    }
                    //foreach (var sample in buffer)
                    //{
                    //    if (sample > max)
                    //    {
                    //        max = sample;
                    //    }
                    //}

                }

            }
        }

        private static void WriteIntro(WaveFileWriter writer)
        {
            var quietIntro = Enumerable.Repeat(0.2f, 20000).ToArray(); //new float[20000];

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
