using System;
using System.Collections.Generic;
using FluentAssertions;
using Mp3Reader;
using Mp3Reader.Interface;
using NUnit.Framework;

namespace Mp3ReaderTests
{
    [TestFixture]
    public class ConfigurationReaderTests
    {
        private IConfigurationReader _configurationReader;

        [SetUp]
        public void SetUp()
        {
            _configurationReader = new ConfigurationReader();
        }

        [Test]
        public void ReadStringSetting_SettingExists_ReturnsValue()
        {
            var result = _configurationReader.ReadStringSetting("TestStringSetting");

            result.Should().Be("TestValue");
        }

        [Test]
        public void ReadStringSetting_SettingDoesNotExist_Throws()
        {
            Assert.That(() => _configurationReader.ReadStringSetting("DoesNotExist"),
                Throws.Exception.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void ReadIntSetting_ValidSettingExists_ReturnsValue()
        {
            var result = _configurationReader.ReadIntSetting("TestIntSetting");

            result.Should().Be(5);
        }

        [Test]
        public void ReadIntSetting_SettingDoesNotExist_Throws()
        {
            Assert.That(() => _configurationReader.ReadIntSetting("DoesNotExist"),
                Throws.Exception.TypeOf<KeyNotFoundException>());
        }

        [Test]
        public void ReadIntSetting_ValueIsNotInt_Throws()
        {
            Assert.That(() => _configurationReader.ReadIntSetting("TestStringSetting"),
                Throws.Exception.TypeOf<FormatException>());
        }
    }
}
