using System.Buffers;
using NAudio.Wave;

namespace MorseCodeTool;

public class MorseWaveProvider : IWaveProvider
{
    private readonly float _amplitude;
    private readonly float _frequency;
    private readonly string _instrumentalSequence;
    private readonly int _sampleRate;
    private readonly int _samplesPerInstrument;
    private int _samplePosition;
    private int _sequencePosition;

    public MorseWaveProvider(string instrumentalSequence, int sampleRate = 44100, float frequency = 440f, float amplitude = 0.5f)
    {
        _sampleRate = sampleRate;
        _frequency = frequency;
        _amplitude = amplitude;
        _sequencePosition = 0;
        _samplesPerInstrument = sampleRate / 10; // tenth of a second per character

        _instrumentalSequence = instrumentalSequence;

        WaveFormat = new WaveFormat(sampleRate, 16, 1); // 16 bit mono
        Console.Write("Getting new buffer..");
    }

    public WaveFormat WaveFormat { get; }

    public int Read(byte[] buffer, int offset, int count)
    {
        Console.Write(".");

        void UpdatePositions()
        {
            _samplePosition++;
            if (_samplePosition <= _samplesPerInstrument)
            {
                return;
            }

            _samplePosition = 0;
            _sequencePosition++;
            if (_sequencePosition >= _instrumentalSequence.Length)
            {
                _sequencePosition = 0;
            }
        }

        // I will create a temporary array of floats which I will then convert to flats
        // For 16 bit, 2 bytes per sample
        var halfCount = count / 2;
        var pool = ArrayPool<float>.Shared;
        var rentedArray = pool.Rent(halfCount); // Rent() may return an array larger than I ordered.
        var floatBuffer = rentedArray.AsSpan(0, halfCount); // This is how I make sure I'm working with a correctly sized rented array.
        try
        {
            try
            {
                for (var n = 0; n < floatBuffer.Length; n++)
                {
                    UpdatePositions();
                    if (_instrumentalSequence[_sequencePosition] is (char) 32)
                    {
                        // Space between characters or words, no sound
                        floatBuffer[n] = 0;
                        continue;
                    }

                    // Making a sound
                    floatBuffer[n] = (float) (_amplitude * Math.Sin(2 * Math.PI * _frequency * _samplePosition / _sampleRate));
                }

                WriteSamplesToBuffer(floatBuffer, buffer.AsSpan(offset));
            }
            finally
            {
                pool.Return(rentedArray);
            }

            return count; // We will return the number of processed bytes
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
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
