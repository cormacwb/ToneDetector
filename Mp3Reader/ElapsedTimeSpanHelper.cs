using System;

namespace Mp3Reader
{
    public class ElapsedTimeSpanHelper
    {
        public static TimeSpan GetElapsedTimeSpan(int sampleRate, long elapsedSampleCount)
        {
            if (elapsedSampleCount < 0) throw new ArgumentException(nameof(elapsedSampleCount));
            if (sampleRate <= 0) throw new ArgumentException(nameof(sampleRate));

            var elapsedSeconds = (double)elapsedSampleCount / sampleRate;

            return TimeSpan.FromSeconds(elapsedSeconds);
        }
    }
}
