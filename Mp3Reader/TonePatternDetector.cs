using System;
using System.Collections.Generic;
using System.Linq;
using Mp3Reader.Interface;
using NAudio.Dsp;

namespace Mp3Reader
{
    public class TonePatternDetector : ITonePatternDetector
    {
        private readonly int _targetFrequency1;
        private readonly int _targetFrequency2;
        private readonly int _sampleRate;

        private ToneDetectorState _state;
        private readonly List<int> _dominantFrequencies; 

        public TonePatternDetector(int targetFrequency1, int targetFrequency2, int sampleRate)
        {
            _targetFrequency1 = targetFrequency1;
            _targetFrequency2 = targetFrequency2;
            _sampleRate = sampleRate;
            _state = ToneDetectorState.NoTargetFrequencyDetected;

            _dominantFrequencies = new List<int>();
        }

        public void Reset()
        {
            _dominantFrequencies.Clear();
            _state = ToneDetectorState.NoTargetFrequencyDetected;
        }

        public bool Detected(float[] samples)
        {
            Validate(samples);

            var fft = CreateFftBuffer(samples);
            FastFourierTransform.FFT(true, GetLog(samples.Length), fft);

            UpdateDominantFrequencyList(fft);
            UpdateState();

            return _state == ToneDetectorState.ToneDetected;
        }

        private static void Validate(float[] samples)
        {
            if (samples == null) throw new ArgumentNullException(nameof(samples));
            if (!samples.Any()) throw new ArgumentException("samples cannot be empty");
        }

        private void UpdateDominantFrequencyList(Complex[] fft)
        {
            var currentDominantFrequency = GetDominantFrequency(fft);
            _dominantFrequencies.Add(currentDominantFrequency);
        }

        private static Complex[] CreateFftBuffer(float[] samples)
        {
            var fft = new Complex[samples.Length];
            for (var i = 0; i < samples.Length; i++)
            {
                var fftComplexInput = CreateComplexInput(samples, i);
                fft[i] = fftComplexInput;
            }
            return fft;
        }

        private static Complex CreateComplexInput(float[] buffer, int index)
        {
            var real = CreateRealComponent(buffer, index);
            const int imaginary = 0;

            return new Complex { X = real, Y = imaginary };
        }

        private static float CreateRealComponent(IReadOnlyList<float> buffer, int index)
        {
            return (float)(buffer[index] * FastFourierTransform.HammingWindow(index, buffer.Count));
        }

        private static int GetLog(int bufferSize)
        {
            return (int) Math.Log(bufferSize, 2);
        }

        private int GetDominantFrequency(Complex[] fft)
        {
            var indexOfMaxMagnitude = GetIndexOfMaxMagnitude(fft);
            return GetFrequency(indexOfMaxMagnitude, _sampleRate, fft.Length);
        }

        private static int GetIndexOfMaxMagnitude(Complex[] fft)
        {
            double maxMagnitude = 0;
            int indexOfMaxMagnitude = 0;
            for (var i = 0; i < fft.Length; i++)
            {
                var magnitude = CalculateMagnitude(fft[i]);
                if (magnitude > maxMagnitude)
                {
                    indexOfMaxMagnitude = i;
                    maxMagnitude = magnitude;
                }
            }
            return indexOfMaxMagnitude;
        }

        private static int GetFrequency(int indexOfMaxMagnitude, int sampleRate, int bufferSize)
        {
            return indexOfMaxMagnitude * sampleRate / bufferSize;
        }

        private static double CalculateMagnitude(Complex complex)
        {
            return Math.Sqrt((complex.X * complex.X) + (complex.Y * complex.Y));
        }

        private void UpdateState()
        {
            var currentDominantFrequency = _dominantFrequencies.Last();
            var previousDominantFrequency = GetPreviousDominantFrequency();

            if (NoChangeInDominantFrequency(previousDominantFrequency, currentDominantFrequency)) return;

            var currentTargetFrequency = GetCurrentTargetFrequency();
            if (currentDominantFrequency == currentTargetFrequency)
            {
                _state = StateTransitionMap[_state];
            }
        }

        private bool NoChangeInDominantFrequency(int previousDominantFrequency, int currentDominantFrequency)
        {
            return previousDominantFrequency == currentDominantFrequency && _state != ToneDetectorState.NoTargetFrequencyDetected;
        }

        private int GetPreviousDominantFrequency()
        {
            return _dominantFrequencies.Count > 1
                ? _dominantFrequencies[_dominantFrequencies.Count - 2]
                : _dominantFrequencies.Single();
        }

        private static readonly Dictionary<ToneDetectorState, ToneDetectorState> StateTransitionMap = new Dictionary<ToneDetectorState, ToneDetectorState>
        {
            {ToneDetectorState.NoTargetFrequencyDetected, ToneDetectorState.Repetion1Frequency1},
            {ToneDetectorState.Repetion1Frequency1, ToneDetectorState.Repetion1Frequency2},
            {ToneDetectorState.Repetion1Frequency2, ToneDetectorState.Repetion2Frequency1},
            {ToneDetectorState.Repetion2Frequency1, ToneDetectorState.Repetion2Frequency2},
            {ToneDetectorState.Repetion2Frequency2, ToneDetectorState.Repetion3Frequency1},
            {ToneDetectorState.Repetion3Frequency1, ToneDetectorState.ToneDetected}
        };

        private int GetCurrentTargetFrequency()
        {
            switch (_state)
            {
                case ToneDetectorState.Repetion1Frequency1:
                case ToneDetectorState.Repetion2Frequency1: 
                case ToneDetectorState.Repetion3Frequency1:
                    return _targetFrequency2;
                case ToneDetectorState.NoTargetFrequencyDetected:
                case ToneDetectorState.Repetion1Frequency2:
                case ToneDetectorState.Repetion2Frequency2:
                    return _targetFrequency1;
                case ToneDetectorState.ToneDetected:
                    throw new InvalidOperationException("Tone has already been detected");
                default:
                    throw new Exception("Component has entered an unexpected state");
            }
        }
    }
}
