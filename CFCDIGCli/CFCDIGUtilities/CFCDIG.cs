using CFCDIGCli.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CFCDIGCli.CFCDIGUtilities
{
    /// <summary>
    /// Provides static classes for unpacking/writing CFC.DIG archives.
    /// </summary>
    public static class CFCDIG
    {
        /// <summary>
        /// Unpacks CFC.DIG archive.
        /// </summary>
        /// <param name="inputPath">Path of CFC.DIG archive.</param>
        /// <param name="outputDirectory">Directory to extract archive contents.</param>
        /// <param name="useDecompression">Decompress compressed files in CFC.DIG or not.</param>
        public static void Unpack(string inputPath, string outputDirectory, bool useDecompression = false)
        {
            using (var reader = new BinaryReader(File.Open(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                reader.BaseStream.Position = 16; // First 16 bytes are blank
                var rawArchiveList = new List<RawArchive>();

                Console.WriteLine("\nGrabbing file records...");
                while (true)
                {
                    var rawArchive = new RawArchive(reader);

                    if (rawArchive.Offset == 0)
                    {
                        Console.WriteLine("Done.");
                        break;
                    }

                    rawArchiveList.Add(rawArchive);
                }

                Directory.CreateDirectory(outputDirectory);
                Console.WriteLine("\nExtracting files...");

                uint count = 0;
                foreach (RawArchive archive in rawArchiveList)
                {
                    count++;
                    reader.BaseStream.Position = archive.Offset;
                    byte[] data;

                    /*if (archive.IsCompressed & useDecompression)
                    {
                        data = Compression.Decompress(reader.ReadBytes((int)archive.PackedSize), archive.UnpackedSize);
                    }
                    else
                    {*/
                        data = reader.ReadBytes((int)archive.PackedSize);
                    //}

                    File.WriteAllBytes($"{outputDirectory}\\{count}_{(archive.SectionCount == ushort.MaxValue ? "UNK" : archive.SectionCount.ToString())}_{(archive.IsCompressed ? '1' : '0')}.raw", data);
                    Console.Write($"\r{count} / {rawArchiveList.Count()}");
                }

                Console.WriteLine();
            }
        }

        /// <summary>
        /// Writes CFC.DIG archive.
        /// </summary>
        /// <param name="inputDirectory">Directory to pack files in.</param>
        /// <param name="outputPath">Path to save CFC.DIG once packed.</param>
        /// <param name="filePathArray">Custom file paths in order of pack order.</param>
        public static void Write(string inputDirectory, string outputPath, string[] filePathArray = null)
        {
            if (filePathArray == null)
            {
                Console.WriteLine("\nFile path array not detected, grabbing and organizing files...");
                string[] filePaths = Directory.GetFiles(inputDirectory, "*", SearchOption.AllDirectories);
                filePathArray = new string[filePaths.Count()];

                for (uint i = 0; i < filePaths.Count(); i++)
                {
                    filePathArray[i] = filePaths[i].Substring(inputDirectory.Length);
                }

                Array.Sort(filePathArray, new NaturalStringComparer());
                Console.WriteLine("Done.");
            }

            using (var writer = new BinaryWriter(File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read)))
            {
                Console.WriteLine("\nWriting File Records...");
                writer.Write(new byte[16]); // First 16 bytes are blank

                uint headerSize = BinaryUtils.GetPaddedSize((uint)(filePathArray.Count() * 16 + 16), 2048, 16);
                uint previousSizesSum = 0;

                uint count = 0;
                foreach (string filePath in filePathArray)
                {
                    count++;
                    string fullPath = inputDirectory + '\\' + filePath;
                    var file = new FileInfo(fullPath);

                    // Make sure to read in __DOCUMENTATION__ for info on the CFCDIG format
                    writer.Write((headerSize + previousSizesSum) / 2048);
                    writer.Write((uint)file.Length);
                    writer.Write(RawArchive.GetFileCount(fullPath));
                    writer.Write((short)0);
                    writer.Write((uint)file.Length);

                    Console.Write($"\r{count} / {filePathArray.Count()}");
                    previousSizesSum += BinaryUtils.GetPaddedSize((uint)file.Length, 2048);
                }

                writer.WritePadding(2048); // CFC.DIG archive records padded to 2048 (0x800)
                Console.WriteLine("\n\nPacking Files...");

                count = 0;
                foreach (string filePath in filePathArray)
                {
                    count++;
                    writer.Write(File.ReadAllBytes(inputDirectory + '\\' + filePath));
                    writer.WritePadding(2048); // All archives in CFC.DIG are padded to 2048

                    Console.Write($"\r{count} / {filePathArray.Count()}");
                }

                Console.WriteLine();
            }
        }
    }
}
