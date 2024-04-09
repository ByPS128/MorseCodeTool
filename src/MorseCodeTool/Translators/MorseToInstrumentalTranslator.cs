using System.Text;

namespace MorseCodeTool.Translators;

public static class MorseToInstrumentalTranslator
{
    private const string SYMBOL_SPACE = " "; // 1 space - Between individual symbols (dots and dashes) of one character (letter or number), there should be a pause that is equivalent in length to one dot.

    private static readonly Dictionary<char, string> _instrumentCodeMap = new ()
    {
        {'.', "X"},
        {'-', "XXX"},
        {' ', "  "}, // 3-1 spaces
        {' ', "      "} // 7-1 spaces
    };

    public static string Translate(string morseCode)
    {
        var sequence = new StringBuilder();

        foreach (var letter in morseCode)
        {
            if (_instrumentCodeMap.TryGetValue(letter, out var instrumentalCode))
            {
                sequence.Append(instrumentalCode);
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
