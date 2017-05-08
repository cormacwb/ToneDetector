using System;

namespace Mp3Reader.Interface
{
    public interface IDispatchMessageRecorder : IDisposable
    {
        string FileName { get; }
        void Record(byte[] rawData, int byteCount, float[] samples, int sampleCount);
        void Close();
    }
}
