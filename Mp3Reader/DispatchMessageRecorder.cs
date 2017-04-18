using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using NAudio.Wave;

namespace Mp3Reader
{
    public class DispatchMessageRecorder : IDisposable
    {
        private const double SilenceThreshold = 0.01;
        private int _lengthOfSilenceInSamples;

        private readonly WaveFileWriter _writer;
        private bool _disposed = false;
        
        public bool IsFinishedRecording { get; private set; }
        public string FileName => _writer.Filename;

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DispatchMessageRecorder(WaveFormat format)
        {
            _writer = new WaveFileWriter(GetFileName(), format);
            IsFinishedRecording = false;
            _lengthOfSilenceInSamples = 0;
        }
        
        public void Record(float[] samples, int bytesRead)
        {
            if (!Done(samples))
            {
                _writer.WriteSamples(samples, 0, bytesRead);
            }
            else
            {
                IsFinishedRecording = true;
                _writer.Close();
            }
        }

        private static string GetFileName()
        {
            return $"dispatch_{DateTime.Now.ToString("ddMMyyyy_HHmmssff")}.wav";
        }

        private static bool EndOfSamples(int bytesRead, IReadOnlyCollection<float> buffer)
        {
            return bytesRead < buffer.Count;
        }

        private bool Done(IReadOnlyCollection<float> buffer)
        {
            var containsOnlySilence = buffer.All(n => Math.Abs(n) < SilenceThreshold);

            if (containsOnlySilence)
            {
                _lengthOfSilenceInSamples += buffer.Count;
                var secondsOfSilence = _lengthOfSilenceInSamples/_writer.WaveFormat.SampleRate;
                return secondsOfSilence > 2;
            }

            _lengthOfSilenceInSamples = 0;
            return false;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _writer.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
