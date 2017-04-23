namespace Mp3Reader.Interface
{
    public interface IConfigurationReader
    {
        string ReadStringSetting(string key);
        int ReadIntSetting(string key);
    }
}