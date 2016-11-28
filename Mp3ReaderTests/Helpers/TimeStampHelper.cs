namespace Mp3ReaderTests.Helpers
{
    public class TimeStampHelper
    {
        public static int GetElapsedSeconds(int sampleRate, long sampleCount)
        {
            return (int)(sampleCount / sampleRate);
        }
    }
}
