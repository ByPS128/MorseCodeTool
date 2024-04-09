﻿using MorseCodeTool.Translators;
using NAudio.Wave;

namespace MorseCodeTool;

public static class MorseWaveProviderFactory
{
    public static IWaveProvider FromText(string text, int sampleRate = 44100, float frequency = 440f, float amplitude = 0.5f)
    {
        Console.WriteLine($"Text to convert: {text}");
        var morseSequence = TextToMorseTranslator.Translate(NormalizeText(text));

        return FromMorseSequence(morseSequence, sampleRate, frequency, amplitude);
    }

    public static IWaveProvider FromMorseSequence(string morseSequence, int sampleRate = 44100, float frequency = 440f, float amplitude = 0.5f)
    {
        Console.WriteLine($"Morse sequence: '{morseSequence}'");
        var instrumentalSequence = MorseToInstrumentalTranslator.Translate(morseSequence);

        return FromInstrumentalSequence(instrumentalSequence, sampleRate, frequency, amplitude);
    }

    public static IWaveProvider FromInstrumentalSequence(string instrumentalSequence, int sampleRate = 44100, float frequency = 440f, float amplitude = 0.5f)
    {
        Console.WriteLine($"Instrumental sequence: '{instrumentalSequence}'");

        return new MorseWaveProvider(instrumentalSequence, sampleRate, frequency, amplitude);
    }

    private static string NormalizeText(string text)
    {
        return text.Trim().ToUpper().Replace(" ", " ") + " ";
    }
}
