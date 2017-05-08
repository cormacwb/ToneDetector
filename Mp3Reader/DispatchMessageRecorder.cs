using System;
using System.Configuration;
using System.IO;
using log4net;
using Mp3Reader.Interface;
using NAudio.Wave;

namespace Mp3Reader
{
    public class DispatchMessageRecorder : IDisposable, IDispatchMessageRecorder
    {
        private readonly WaveFileWriter _writer;
        private bool _disposed;
        private readonly DateTime _recordingStartTimeUtc;
        
        public string FileName => _writer.Filename;
        private readonly FileTransferTrigger _fileTransferTrigger;

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DispatchMessageRecorder(WaveFormat format)
        {
            _recordingStartTimeUtc = DateTime.UtcNow;

            var apiBaseUri = ConfigurationManager.AppSettings["ApiBaseUri"];
            var outputPath = ConfigurationManager.AppSettings["OutputDirectory"];
            var outputDirectory = new DirectoryInfo(outputPath);
            if (!outputDirectory.Exists) outputDirectory.Create();

            _fileTransferTrigger = new FileTransferTrigger(apiBaseUri, outputPath);
            _writer = new WaveFileWriter(GetFileNameWithRelativePath(outputPath), format);
        }
        
        public void Record(byte[] rawData, int byteCount, float[] samples, int sampleCount)
        {
            _writer.Write(rawData, 0, byteCount);
        }

        public void Close()
        {
            _writer.Close();
            _fileTransferTrigger.SweepOutputDirectory();
            Log.Info($"Finished recording {_recordingStartTimeUtc}");
        }

        private static string GetFileNameWithRelativePath(string path)
        {
            return $"{path}\\dispatch_{DateTime.Now:ddMMyyyy_HHmmssff}.wav";
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
