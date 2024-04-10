using NAudio.Wave;

namespace MorseCodeTool;

/*
 Please read README.md
*/

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("\n\n\nMorse Code generator\n");

        if (DetermineIfParameterPresent(args, "-writefile"))
        {
            var tempFilePath = Path.GetTempFileName();
            tempFilePath = Path.ChangeExtension(tempFilePath, ".wav");
            Console.WriteLine($"Temporary output audio file: {tempFilePath}");

            // create a wave file with the SOS signal for 10 seconds
            await CreateWaveFile("sos", 10 * 1000, tempFilePath);
        }

        using var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel(); // Indicates that cancellation has been requested
            Console.WriteLine("\nTerminating the application...");
        };

        Console.WriteLine("Morse Code generator started. Press CTRL+C to stop.");

        // Passing a CancellationToken to a running task/loop
        await PlayAudioInfinitely(cancellationTokenSource.Token);

        Console.WriteLine("Morse Code generator stopped.");
    }

    private static Task PlayAudioInfinitely(CancellationToken cancellationToken)
    {
        var morseProvider = MorseWaveProviderFactory.FromText("sos");
        var waveOut = new WaveOutEvent();
        waveOut.Init(morseProvider);
        waveOut.Play();

        // A loop that runs until Ctrl+C is caught
        // This also makes it possible to play the generated audio.
        while (!cancellationToken.IsCancellationRequested) Thread.Sleep(100); // A short pause to make the loop cycle more CPU friendly

        return Task.CompletedTask;
    }

    private static bool DetermineIfParameterPresent(string[]? args, string parameterName)
    {
        if (args is null || args.Length == 0)
        {
            return false;
        }

        return args
            .Where(i => !string.IsNullOrWhiteSpace(i) && i.StartsWith("-"))
            .Any(i => string.Equals(i, parameterName, StringComparison.OrdinalIgnoreCase));
    }

    private static Task CreateWaveFile(string text, int milliseconds, string fileName)
    {
        var morseProvider = MorseWaveProviderFactory.FromText(text);
        using var waveOut = new WaveFileWriter(fileName, morseProvider.WaveFormat);
        morseProvider.WriteWaveData(waveOut, milliseconds);

        return Task.CompletedTask;
    }
}
