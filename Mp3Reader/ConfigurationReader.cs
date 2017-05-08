using System;
using System.Collections.Generic;
using System.Configuration;
using Mp3Reader.Interface;

namespace Mp3Reader
{
    public class ConfigurationReader : IConfigurationReader
    {
        public int ReadToneFrequency1()
        {
            return ReadIntSetting("Tone1Frequency");
        }

        private int ReadIntSetting(string key)
        {
            return Convert.ToInt32(ReadStringSetting(key));
        }

        public int ReadToneFrequency2()
        {
            return ReadIntSetting("Tone2Frequency");
        }

        public string ReadStreamUrl()
        {
            return ReadStringSetting("StreamUrl");
        }

        private string ReadStringSetting(string key)
        {
            var value = ConfigurationManager.AppSettings[key];

            if (value == null) throw new KeyNotFoundException(nameof(key));

            return value;
        }
    }
}
