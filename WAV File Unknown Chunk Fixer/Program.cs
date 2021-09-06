using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WAV_File_Unknown_Chunk_Fixer
{
    class Program
    {

        static string wavPath;
        static bool showHelp;

        static List<string> good_chunks;

        static void Main(string[] args)
        {

            // CLI stuff

            Console.WriteLine("");
            Console.WriteLine("WAVChunkFix v1.1 by O_Circles, 2021");
            Console.WriteLine("");

            good_chunks = new[] { "fmt ", "data" }.ToList();

            ParseParams(args);

            if (!string.IsNullOrEmpty(wavPath) && !Directory.Exists(wavPath))
            {
                Console.WriteLine("The path \"" + wavPath + "\" does not exist. Exiting application.");
                return;
            }

            if (string.IsNullOrEmpty(wavPath))
                wavPath = Directory.GetCurrentDirectory();

            if (showHelp)
            {
                ShowHelp();
                return;
            }


            // Actual *do stuff here*

            var wavFiles = Directory.GetFiles(wavPath, "*.wav");

            Console.WriteLine("Searching for .wav files in " + wavPath + Environment.NewLine);

            foreach (var file in wavFiles)
                FixWav(file);

            Console.WriteLine("");
        }

        static void ShowHelp()
        {
            string[] help = new[]
            {
                "Do note that argument prefixes like /, -, -- are all interchangable",
                "Meaning that /path will work just as well as --path",
                "",
                "OPTIONS:",
                "",
                "/path, /p [some directory path]\t\tSpecify what directory to use",
                "/fact, /f\t\t\t\tKeep \"fact\" chunks for non-PCM",
                "/help, /h, /?\t\t\t\tDisplay this text"
            };

            foreach (var s in help)
                Console.WriteLine(s);

            Console.WriteLine("");
        }

        static void ParseParams(string[] args)
        {
            int index = 0;

            foreach (var arg in args)
            {
                if (arg[0] == '/' || arg[0] == '-')
                {
                    string realArg = "";
                    realArg = arg.TrimStart('-');
                    realArg = realArg.TrimStart('/');

                    if (!string.IsNullOrEmpty(realArg))
                    {
                        switch (realArg)
                        {
                            case "path":
                            case "p":
                                wavPath = args[index+1];
                                break;
                            case "fact":
                            case "f":
                                good_chunks.Add("fact");
                                break;
                            case "help":
                            case "?":
                            case "h":
                                showHelp = true;
                                return;
                        }
                    }
                }
                index++;
            }
        }




        static void FixWav(string path)
        {
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            Console.Write(fileName + "...");

            var wave = ReadFile(path);
            int badChunks = 0;

            foreach (var v in wave.Chunks)
                if (!good_chunks.Contains(v.Name))
                    badChunks++;

            if (badChunks == 0)
            {
                Console.Write("OK" + Environment.NewLine);
                return;
            }

            int removed = 0;

            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(directory + "\\new_" + fileName)))
                {
                    byte[] rh_riff = Encoding.ASCII.GetBytes("RIFF");
                    byte[] rh_wave = Encoding.ASCII.GetBytes("WAVE");


                    uint actual_size = 4; // WAVE header
                    //actual_size += 12;  // RIFF header

                    foreach (var v in wave.Chunks)
                    {
                        if (!good_chunks.Contains(v.Name))
                        {
                            actual_size += 8; //ID and Size header
                            actual_size += v.Length;

                            if (v.Padded)
                                actual_size++;
                        }
                    }

                    writer.Write(rh_riff);
                    writer.Write(actual_size);
                    writer.Write(rh_wave);

                    foreach (var v in wave.Chunks)
                    {
                        if (v.Name == "fmt " || v.Name == "data")
                        {
                            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
                            {
                                int pad = 0;
                                if (v.Padded) pad = 1;

                                int size = (int)v.Length + pad;

                                reader.BaseStream.Position = v.Position + 8;
                                var bytes = reader.ReadBytes(size);

                                writer.Write(Encoding.ASCII.GetBytes(v.Name));
                                writer.Write(v.Length);
                                writer.Write(bytes);
                                
                            }
                        }
                        else
                            removed++;

                    }

                }
            }
            catch (Exception ex)
            {
                Console.Write("ERROR: " + ex.Message);
            }

            var oldPath = directory + "\\old\\" + fileName;

            Directory.CreateDirectory(directory + "\\old");

            if (File.Exists(oldPath))
                File.Delete(oldPath);
            
            File.Move(path, oldPath);
            File.Move(directory + "\\new_" + fileName, directory + "\\" + fileName);

            Console.Write("FIXED (Removed " + removed + " unknown chunks)");
            Console.WriteLine("");

        }

        static WAVFile ReadFile(string path)
        {

            WAVFile wave = new WAVFile();
            wave.Path = path;

            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
                {
                    string rh_riff = Encoding.ASCII.GetString(reader.ReadBytes(4));
                    wave.Length = reader.ReadUInt32();
                    string rh_wave = Encoding.ASCII.GetString(reader.ReadBytes(4));

                    if (rh_riff != "RIFF" || rh_wave != "WAVE")
                        throw new Exception("Missing or malformed RIFF WAVE header in file");


                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        WAVChunk chunk = new WAVChunk
                        {
                            Position = reader.BaseStream.Position,
                            Name = Encoding.ASCII.GetString(reader.ReadBytes(4)),
                            Length = reader.ReadUInt32()
                        };

                        if (chunk.Length % 2 != 0)
                            chunk.Padded = true;

                        reader.BaseStream.Position += chunk.Length;
                        if (chunk.Padded)
                            reader.BaseStream.Position++;

                        if (chunk.Name == "fmt ")
                            wave.FORMATCHUNK = chunk;

                        wave.Chunks.Add(chunk);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write("ERROR: " + ex.Message);
            }

            return wave;
        }

        struct WAVChunk
        {
            public string Name;
            public long Position;
            public UInt32 Length;
            public bool Padded;
        }

        class WAVFile
        {
            public string Path;
            public UInt32 Length;

            public WAVChunk FORMATCHUNK;
            public List<WAVChunk> Chunks;
        
            public WAVFile()
            {
                Chunks = new List<WAVChunk>();
            }
        }

    }
}
