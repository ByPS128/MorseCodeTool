using System.Text;

namespace MorseCodeTool.Translators;

public static class TextToMorseTranslator
{
    private const string SYMBOL_SPACE = " "; // 1 space - Between individual symbols (dots and dashes) of one character (letter or number), there should be a pause that is equivalent in length to one dot.

    private static readonly Dictionary<char, string> _morseCodeMap = new ()
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
        {' ', " "}, // ASCII 32 - soft space
        {' ', " "} // ASCII 160 - hard space
    };

    public static string Translate(string text)
    {
        var sequence = new StringBuilder();

        foreach (var letter in text)
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
}
