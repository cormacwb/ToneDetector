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
        public void ReadToneFrequency1_ReturnsValue()
        {
            var result = _configurationReader.ReadToneFrequency1();

            result.Should().Be(1);
        }

        [Test]
        public void ReadToneFrequency2_ReturnsValue()
        {
            var result = _configurationReader.ReadToneFrequency2();

            result.Should().Be(2);
        }

        [Test]
        public void ReadStreamUrl_ReturnsValue()
        {
            var result = _configurationReader.ReadStreamUrl();

            result.Should().Be("www.fakeurl.com");
        }
    }
}
