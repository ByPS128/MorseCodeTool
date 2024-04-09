# MorseCodeTool

## How the app works:

The input text is translated into Morse code.  
Morse code is translated into a field of spaces and the character X, where X is an audio signal.  
Space plays no sound and X plays sound.  
At the same time, the rules of pauses between letters and words are respected.  
For example, the text SOS is translates:  
to morse code: `... --- ... `  
to instrumental sequence: `x x x   xxx xxx xxx   x x x       `  

I need to widen the space between letters and the space between words.  
Letter spacing is ASCII 32, word spacing is ASCII 160.

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
