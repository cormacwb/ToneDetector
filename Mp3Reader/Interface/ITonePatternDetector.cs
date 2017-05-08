namespace Mp3Reader.Interface
{
    public interface ITonePatternDetector
    {
        bool Detected(float [] samples, int sampleRate);
        void Reset();
    }
}
