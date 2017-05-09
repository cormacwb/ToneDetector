using System;

namespace Mp3Reader.Interface
{
    public interface IConfigurationReader
    {
        int ReadToneFrequency1();
        int ReadToneFrequency2();
        string ReadStreamUrl();
        TimeSpan ReadSleepTime();
    }
}
