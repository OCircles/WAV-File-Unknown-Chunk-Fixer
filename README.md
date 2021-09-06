# WAV File Unknown Chunk Fixer
Removes chunks from .wav files that makes certain (especially older) programs unable to read them

This will only keep the format chunk and the data chunks

I created this specifically for one of the many programs that fail to properly read .wav files whenever the header is not exactly 44 bytes long and assumes the rest is data chunks, so I decided not to include the "fact" chunk for Non-PCM. I'm too lazy to put a in CLI parameter for this, but you can very easily modify the program to keep that one as well if you'd like

## Usage:
Place WAVChunkFix.exe in a directory containing your broken .wav files and simply run it

If it detects a .wav file that has chunks other than "fmt " and "data", then it will move the old .wav file into another directory that it will create at the same location (called "old"), and then replace the old one with one only containing said chunks

---

![WAVChunkFix in use](screenshot.png?raw=true "WAVChunkFix in use")
