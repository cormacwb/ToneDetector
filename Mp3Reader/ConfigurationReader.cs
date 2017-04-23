using System;
using System.Collections.Generic;
using System.Configuration;
using Mp3Reader.Interface;

namespace Mp3Reader
{
    public class ConfigurationReader : IConfigurationReader
    {
        public string ReadStringSetting(string key)
        {
            var value = ConfigurationManager.AppSettings[key];

            if (value == null) throw new KeyNotFoundException(nameof(key));

            return value;
        }

        public int ReadIntSetting(string key)
        {
            return Convert.ToInt32(ReadStringSetting(key));
        }
    }
}
