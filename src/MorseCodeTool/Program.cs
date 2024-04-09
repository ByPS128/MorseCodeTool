using NAudio.Wave;

namespace MorseCodeTool;

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel(); // Indicates that cancellation has been requested
            Console.WriteLine("Terminating the application...");
        };

        Console.WriteLine("Morse Code generator started. Press CTRL+C to stop.");

        // Passing a CancellationToken to a running task/loop
        await DoWork(cancellationTokenSource.Token);

        Console.WriteLine("Morse Code generator stopped.");
    }

    private static Task DoWork(CancellationToken cancellationToken)
    {
        var morseProvider = new MorseWaveProvider("sos");
        var waveOut = new WaveOutEvent();
        waveOut.Init(morseProvider);
        waveOut.Play();

        // A loop that runs until Ctrl+C is caught
        // This also makes it possible to play the generated audio.
        while (!cancellationToken.IsCancellationRequested) Thread.Sleep(100); // A short pause to make the loop cycle more CPU friendly

        return Task.CompletedTask;
    }
}
