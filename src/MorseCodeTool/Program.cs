using MorseCodeTool.Translators;
using NAudio.Wave;

namespace MorseCodeTool;

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

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel(); // Indicates that cancellation has been requested
            Console.WriteLine("\nTerminating the application...");
        };

        Console.WriteLine("Morse Code generator started. Press CTRL+C to stop.");

        // Passing a CancellationToken to a running task/loop
        await DoWork(cancellationTokenSource.Token);

        Console.WriteLine("Morse Code generator stopped.");
    }

    private static Task DoWork(CancellationToken cancellationToken)
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
}
