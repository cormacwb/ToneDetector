namespace Mp3Reader.Interface
{
    public interface ITonePatternDetector
    {
        bool Detected(float [] samples);
    }
}
