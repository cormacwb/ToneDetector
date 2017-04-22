using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using NAudio.Wave;
using log4net;
using log4net.Config;


namespace Mp3Reader
{
    public class Program
    {
        private const int SampleBufferSize = 1024;
        private static string _streamUrl;
        private static int _toneFrequency1;
        private static int _toneFrequency2;

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {
            ReadApplicationSettings();
            XmlConfigurator.Configure();
            Log.Info("Starting...");
            var request = (HttpWebRequest)WebRequest.Create(_streamUrl);

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
                var recorders = new List<DispatchMessageRecorder>();
                var byteBuffer = new byte[16384 * 4];
                var readFullyStream = new ReadFullyStream(responseStream);
                var decompressor = CreateDecompressor(readFullyStream);
                var bufferedWaveProvider = CreateBufferedWaveProvider(decompressor);
                var toneDetector = new TonePatternDetector(_toneFrequency1, _toneFrequency2,
                    bufferedWaveProvider.WaveFormat.SampleRate);
                var sampleProvider = bufferedWaveProvider.ToSampleProvider();

                while (true)
                {
                    var frame = Mp3Frame.LoadFromStream(readFullyStream);
                    var bytesReadCount = decompressor.DecompressFrame(frame, byteBuffer, 0);
                    bufferedWaveProvider.AddSamples(byteBuffer, 0, bytesReadCount);
                    var sampleCount = sampleProvider.Read(sampleBuffer, 0, sampleBuffer.Length);

                    if (EndOfSamples(sampleCount, sampleBuffer)) break;
                    
                    if (toneDetector.Detected(sampleBuffer))
                    {
                        Log.Info($"Tone detected at {DateTime.UtcNow}");
                        recorders.Add(new DispatchMessageRecorder(bufferedWaveProvider.WaveFormat));
                        toneDetector.Reset();
                    }

                    foreach (var recorder in recorders)
                    {
                        recorder.Record(byteBuffer, bytesReadCount, sampleBuffer, sampleCount);
                    }

                    recorders = RefreshRecorderList(recorders).ToList();
                }

                recorders.ForEach(r => r.Dispose());

                Log.Info("End of stream");
            }
        }

        private static void ReadApplicationSettings()
        {
            _streamUrl = ConfigurationManager.AppSettings["StreamUrl"];
            _toneFrequency1 = Convert.ToInt32(ConfigurationManager.AppSettings["Tone1Frequency"]);
            _toneFrequency2 = Convert.ToInt32(ConfigurationManager.AppSettings["Tone2Frequency"]);
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

        private static bool EndOfSamples(int bytesRead, IReadOnlyCollection<float> buffer)
        {
            return bytesRead < buffer.Count;
        }
    }
}
