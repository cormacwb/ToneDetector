using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using DryIoc;
using NAudio.Wave;
using log4net;
using log4net.Config;
using Mp3Reader.Interface;

namespace Mp3Reader
{
    public class Program
    {
        private const int SampleBufferSize = 1024;

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {
            var container = CreateContainer();
            XmlConfigurator.Configure();
            Log.Info("Starting...");
            var request = (HttpWebRequest) WebRequest.Create(container.Resolve<IConfigurationReader>().ReadStreamUrl());

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                Log.Info("Connected");
            }
            catch (WebException e)
            {
                Log.Error($"Could not read stream: {e.Message}");
                Environment.Exit(1);
            }

            using (var responseStream = response.GetResponseStream())
            {
                var sampleBuffer = new float[SampleBufferSize];
                var recorders = new List<IDispatchMessageRecorder>();
                var byteBuffer = new byte[16384 * 4];
                var readFullyStream = new ReadFullyStream(responseStream);
                var decompressor = CreateDecompressor(readFullyStream);
                var bufferedWaveProvider = CreateBufferedWaveProvider(decompressor);
                var toneDetector = container.Resolve<ITonePatternDetector>();
                var sampleProvider = bufferedWaveProvider.ToSampleProvider();
                var sampleRate = bufferedWaveProvider.WaveFormat.SampleRate;
                var silenceDetector = container.Resolve<ISilenceDetector>();

                while (true)
                {
                    var frame = Mp3Frame.LoadFromStream(readFullyStream);
                    var bytesReadCount = decompressor.DecompressFrame(frame, byteBuffer, 0);
                    bufferedWaveProvider.AddSamples(byteBuffer, 0, bytesReadCount);
                    var sampleCount = sampleProvider.Read(sampleBuffer, 0, sampleBuffer.Length);

                    if (EndOfSamples(sampleCount, sampleBuffer)) break;
                    
                    if (toneDetector.Detected(sampleBuffer, sampleRate))
                    {
                        Log.Info($"Tone detected at {DateTime.UtcNow}");
                        recorders.Add(new DispatchMessageRecorder(bufferedWaveProvider.WaveFormat));
                        toneDetector.Reset();
                    }

                    foreach (var recorder in recorders)
                    {
                        recorder.Record(byteBuffer, bytesReadCount, sampleBuffer, sampleCount);
                    }

                    if(silenceDetector.IsRecordingComplete(sampleBuffer, sampleRate))
                    {
                        recorders.ForEach(r => r.Dispose());
                        recorders.Clear();
                    }
                }

                recorders.ForEach(r => r.Dispose());

                Log.Info("End of stream");
            }
        }

        private static IContainer CreateContainer()
        {
            var container = new Container();
            container.Register<IDispatchMessageRecorder, DispatchMessageRecorder>(Reuse.Transient);
            container.Register<ITonePatternDetector, TonePatternDetector>(Reuse.Singleton);
            container.Register<ISilenceDetector, SilenceDetector>();

            return container;
        }

        private static IMp3FrameDecompressor CreateDecompressor(Stream readFullyStream)
        {
            var firstFrame = Mp3Frame.LoadFromStream(readFullyStream);
            var decompressor = CreateFrameDecompressor(firstFrame);
            return decompressor;
        }

        private static BufferedWaveProvider CreateBufferedWaveProvider(IMp3FrameDecompressor decompressor)
        {
            return new BufferedWaveProvider(decompressor.OutputFormat)
            {
                BufferDuration = TimeSpan.FromSeconds(1)
            };
        }

        private static IMp3FrameDecompressor CreateFrameDecompressor(Mp3Frame frame)
        {
            var waveFormat = new Mp3WaveFormat(frame.SampleRate, frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                frame.FrameLength, frame.BitRate);
            return new AcmMp3FrameDecompressor(waveFormat);
        }

        private static bool EndOfSamples(int bytesRead, IReadOnlyCollection<float> buffer)
        {
            return bytesRead < buffer.Count;
        }
    }
}
