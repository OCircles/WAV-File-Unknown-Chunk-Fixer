# WAV File Unknown Chunk Fixer
Removes chunks from .wav files that makes certain (especially older) programs unable to read them

This will by default only keep the format and data chunks ("fmt "/"data"), but can also be set to keep "fact" chunks

I created this specifically for one of the many programs that fail to properly read .wav files whenever there is not exactly 44 bytes of header followed by only data chunks

## Usage
Place WAVChunkFix.exe in a directory containing your broken .wav files and simply run it to fix all the contained .wav files in one go

If it detects a .wav file that has chunks other than "fmt " and "data", then it will move the old .wav file into another directory that it will create at the same location (called "old"), and then replace the old one with one only containing said chunks

## Parameters
- /path, /p "{some path here}"    Specify another directory to work in
- /fact, /f                     Keep "fact" chunks for non-PCM
- /help, /h, /?                 Show this help

---

![WAVChunkFix in use](screenshot.png?raw=true "WAVChunkFix in use")
