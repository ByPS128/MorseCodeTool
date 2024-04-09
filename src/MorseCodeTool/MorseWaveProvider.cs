using MorseCodeTool.Translators;
using NAudio.Wave;

namespace MorseCodeTool;

public class MorseWaveProvider : IWaveProvider
{
    private readonly float _amplitude;
    private readonly float _frequency;
    private readonly string _instrumentalSequence;
    private readonly int _sampleRate;
    private readonly int _samplesPerDot;
    private int _samplePosition;
    private int _sequencePosition;

    public MorseWaveProvider(string text, int sampleRate = 44100, float frequency = 440f, float amplitude = 0.5f)
    {
        text = NormalizeText(text);
        Console.WriteLine($"Text to convert: {text}");
        var morseSequence = TextToMorseTranslator.Translate(text);
        Console.WriteLine($"Morse sequence: '{morseSequence}'");
        _instrumentalSequence = MorseToInstrumentalTranslator.Translate(morseSequence);
        Console.WriteLine($"Instrumental sequence: '{_instrumentalSequence}'");

        _sampleRate = sampleRate;
        _frequency = frequency;
        _amplitude = amplitude;
        _sequencePosition = 0;
        _samplesPerDot = sampleRate / 10; // tenth of a second per character

        WaveFormat = new WaveFormat(sampleRate, 16, 1); // 16 bit mono
    }

    public WaveFormat WaveFormat { get; }

    public int Read(byte[] buffer, int offset, int count)
    {
        void UpdatePositions()
        {
            _samplePosition++;
            if (_samplePosition > _samplesPerDot)
            {
                _samplePosition = 0;
                _sequencePosition++;
                if (_sequencePosition >= _instrumentalSequence.Length)
                {
                    _sequencePosition = 0;
                }
            }
        }

        // I will create a temporary array of floats which I will then convert to flats
        var floatBuffer = new float[count / 2]; // For 16 bit, 2 bytes per sample

        for (var n = 0; n < floatBuffer.Length; n++)
        {
            UpdatePositions();
            var morseSignal = _instrumentalSequence[_sequencePosition];

            if (morseSignal == ' ')
            {
                // Space between characters or words, no sound
                floatBuffer[n] = 0;
            }
            else
            {
                // Making a sound for a dot or dash
                floatBuffer[n] = (float) (_amplitude * Math.Sin(2 * Math.PI * _frequency * _samplePosition / _sampleRate));
            }
        }

        WriteSamplesToBuffer(floatBuffer, buffer.AsSpan(offset));

        return count; // We will return the number of processed bytes
    }

    private static string NormalizeText(string text)
    {
        return text.Trim().ToUpper().Replace(" ", " ") + " ";
    }

    private void WriteSamplesToBuffer(Span<float> samples, Span<byte> buffer)
    {
        // I will convert the float samples to 16-bit integers and then to bytes
        for (var i = 0; i < samples.Length; i++)
        {
            var sample = (short) (samples[i] * short.MaxValue);
            buffer[2 * i] = (byte) (sample & 0xFF);
            buffer[2 * i + 1] = (byte) ((sample >> 8) & 0xFF);
        }
    }
}
