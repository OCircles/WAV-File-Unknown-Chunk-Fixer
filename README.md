# WAV-File-Unknown-Chunk-Fixer
Removes chunks from .wav files that makes certain (especially older) programs unable to read them.

This will only keep the format chunk and the data chunks. 

## Usage:
Place WAVChunkFix.exe in a directory containing your broken .wav files and simply run it.

If it detects a .wav file that needs fixing, then it will move the old .wav file into a directory that it will create at the same location (called "old"), and then replace the broken one with the fixed one.

![WAVChunkFix in use](screenshot.png?raw=true "WAVChunkFix in use")
