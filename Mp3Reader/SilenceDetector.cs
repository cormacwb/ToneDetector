using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Mp3Reader.Interface;

namespace Mp3Reader
{
    public class SilenceDetector : ISilenceDetector
    {
        private const double SilenceThreshold = 0.01;
        private int _lengthOfSilenceInSamples;
        private readonly int _secondsOfSilenceEndingEachRecording;

        public SilenceDetector()
        {
            _lengthOfSilenceInSamples = 0;
            _secondsOfSilenceEndingEachRecording =
                Convert.ToInt32(ConfigurationManager.AppSettings["SecondsOfSilenceEndingEachRecording"]);
        }

        public bool IsRecordingComplete(IReadOnlyCollection<float> buffer, int sampleRate)
        {
            var containsOnlySilence = buffer.All(n => Math.Abs(n) < SilenceThreshold);

            if (containsOnlySilence)
            {
                _lengthOfSilenceInSamples += buffer.Count;
                var secondsOfSilence = _lengthOfSilenceInSamples/sampleRate;
                return secondsOfSilence > _secondsOfSilenceEndingEachRecording;
            }

            _lengthOfSilenceInSamples = 0;
            return false;
        }
    }
}
