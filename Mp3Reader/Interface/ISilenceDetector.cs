using System.Collections.Generic;

namespace Mp3Reader.Interface
{
    public interface ISilenceDetector
    {
        bool IsRecordingComplete(IReadOnlyCollection<float> buffer);
    }
}
