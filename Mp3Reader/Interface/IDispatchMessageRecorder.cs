namespace Mp3Reader.Interface
{
    public interface IDispatchMessageRecorder
    {
        bool IsFinishedRecording { get; }
        string FileName { get; }
        void Record(byte[] rawData, int byteCount, float[] samples, int sampleCount);
        void Dispose();
    }
}
