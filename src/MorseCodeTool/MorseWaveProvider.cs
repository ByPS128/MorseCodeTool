using System.Text;
using NAudio.Wave;

namespace MorseCodeTool;

public class MorseWaveProvider : IWaveProvider
{
    /*
     How the app works:

     The input text is translated into Morse code.
     Morse code is translated into a field of spaces and the character X, where X is an audio signal.
     Space plays no sound and X plays sound.
     At the same time, the rules of pauses between letters and words are respected.
     For example, the text SOS is translates:
     to morse code: "... --- ... "
     to instrumental sequence: "x x x   xxx xxx xxx   x x x       "

     I need to widen the space between letters and the space between words.  
     Letter spacing is ASCII 32 (soft space), word spacing is ASCII 160 (hard space).

     I declare that one instrument lasts a tenth of a second.
     Subsequently, the number of samples per instrument is calculated.
     Instruments array is traversed and sound samples are generated according to them.
     When the end of the instrument list is reached, it starts again from the beginning.

     To generate the audio signal, I used an audio stream, which is implemented using the IWaveProvider interface.
     The provider tells itself about the filling of the buffer for playing the sound and starts playing it.
     As soon as it approaches the end of the buffer during playback, it will ask for another buffer filling.
     So the audio data is generated piece by piece in advance. 
     The application is therefore a state machine that remembers where the generation of audio data ended and as soon as 
     the application provider requests additional data, the application continues generating from the last position 
     where the previous generation ended.
    */
     
    const string SYMBOL_SPACE = " "; // 1 space - Between individual symbols (dots and dashes) of one character (letter or number), there should be a pause that is equivalent in length to one dot.
    const string LETTER_SPACE = "  "; // 3-1 spaces - Between the letters of a word there should be a pause that is equivalent in length to three periods. This allows distinguishing where one letter ends and another begins.
    const string DOT = "x" + SYMBOL_SPACE;
    const string DASH = "xxx" + SYMBOL_SPACE;
    const string WORD_SPACE = "      "; // 7-1 spaces - Between words there should be a pause that is equivalent in length to seven periods. This clearly indicates the separation of one word from another.

    private readonly float _frequency;
    private readonly Dictionary<char, string> _morseCodeMap = new()
    {
        {'A', ".-"},
        {'B', "-..."},
        {'C', "-.-."},
        {'D', "-.."},
        {'E', "."},
        {'F', "..-."},
        {'G', "--."},
        {'H', "...."},
        {'I', ".."},
        {'J', ".---"},
        {'K', "-.-"},
        {'L', ".-.."},
        {'M', "--"},
        {'N', "-."},
        {'O', "---"},
        {'P', ".--."},
        {'Q', "--.-"},
        {'R', ".-."},
        {'S', "..."},
        {'T', "-"},
        {'U', "..-"},
        {'V', "...-"},
        {'W', ".--"},
        {'X', "-..-"},
        {'Y', "-.--"},
        {'Z', "--.."},
        {'1', ".----"},
        {'2', "..---"},
        {'3', "...--"},
        {'4', "....-"},
        {'5', "....."},
        {'6', "-...."},
        {'7', "--..."},
        {'8', "---.."},
        {'9', "----."},
        {'0', "-----"},
        {'.', ".-.-.-"},
        {',', "--..--"},
        {'?', "..--.."},
        {'!', "-.-.--"},
        {'-', "-....-"},
        {'/', "-..-."},
        {'@', ".--.-."},
        {'(', "-.--.-"},
        {')', "-.--.-"},
        {'"', ".-..-."},
        {'=', "-...-"},
        {'+', ".-.-."},
        {';', "-.-.-."},
        {':', "---..."},
        {'$', "...-..-"},
        {'&', ".-..."},
        {'_', "..--.-"},
        {' ', " "},
        {' ', " "}
    };

    private readonly string _morseSequence;
    private readonly string _instrumentalSequence;
    private readonly int _sampleRate;
    private readonly int _samplesPerDot;
    private int _samplePosition;
    private int _sequencePosition;
    private readonly float _amplitude;

    public MorseWaveProvider(string text, int sampleRate = 44100, float frequency = 440f, float amplitude = 0.5f)
    {
        text = text.Trim().ToUpper().Replace(" ", " ") + " ";
        Console.WriteLine($"Text to convert: {text}");
        _morseSequence = ConvertTextToMorseSequence(text);
        Console.WriteLine($"Morse sequence: '{_morseSequence}'");
        _instrumentalSequence = ConvertMorseSequenceToInstrumentalSequence(_morseSequence);
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
            char morseSignal = _instrumentalSequence[_sequencePosition];

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

    private void WriteSamplesToBuffer(Span<float> samples, Span<byte> buffer)
    {
        // I will convert the float samples to 16-bit integers and then to bytes
        for (int i = 0; i < samples.Length; i++)
        {
            var sample = (short)(samples[i] * short.MaxValue);
            buffer[2 * i] = (byte)(sample & 0xFF);
            buffer[2 * i + 1] = (byte)((sample >> 8) & 0xFF);
        }
    }

    private string ConvertTextToMorseSequence(string text)
    {
        var sequence = new StringBuilder();

        foreach (char letter in text)
        {
            if (_morseCodeMap.TryGetValue(letter, out var morseCode))
            {
                sequence.Append(morseCode);
                sequence.Append(SYMBOL_SPACE);
            }
            else
            {
                throw new ArgumentException($"Unknown character '{letter}', cannot be translated into Morse code.");
            }
        }

        return sequence.ToString();
    }

    private string ConvertMorseSequenceToInstrumentalSequence(string morseSequence)
    {
        var sequence = new StringBuilder();

        foreach (char symbol in morseSequence)
        {
            switch (symbol)
            {
                case '.':
                    sequence.Append(DOT);
                    break;
                case '-':
                    sequence.Append(DASH);
                    break;
                case ' ':
                    sequence.Append(LETTER_SPACE);
                    break;
                case ' ':
                    sequence.Append(WORD_SPACE);
                    break;
            }
        }

        return sequence.ToString();
    }
}
