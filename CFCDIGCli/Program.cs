using CFCDIGCli.CFCDIGUtilities;
using System;
using System.IO;
using System.Linq;

namespace CFCDIGCli
{
    class Program
    {
        static void Main(string[] args)
        {
            switch (args.Count())
            {
                case 0:
                    {
                        args = new string[7] { "-p", "-i", "data", "-o", "CFC.DIG", "-l", "filelist.txt" };
                        break;
                    }

                case 1:
                    {
                        if (File.Exists(args[0]))
                        {
                            args = new string[5] { "-u", "-i", args[0], "-o", $"{args[0]}_output" };
                        }
                        else if (Directory.Exists(args[0]))
                        {
                            args = new string[7] { "-p", "-i", args[0], "-o", $"{Path.GetDirectoryName(args[0])}\\CFC.DIG", "-l", $"{Path.GetDirectoryName(args[0])}\\filelist.txt" };
                        }

                        break;
                    }
            }

            switch (args[0])
            {
                default:
                    {
                        Program.PrintHelp(true);
                        break;
                    }

                case "-h":
                case "-help":
                    {
                        Program.PrintHelp();
                        break;
                    }

                case "-u":
                case "-unpack":
                    {
                        string inputPath = null;
                        string outputDirectory = null;
                        bool useDecompression = true;

                        for (uint i = 1; i < args.Count(); i++)
                        {
                            string option = args[i].ToLower();

                            switch (option)
                            {
                                default:
                                    {
                                        Program.PrintHelp(true);
                                        return;
                                    }

                                case "-i":
                                case "-in":
                                case "-input":
                                    {
                                        i++;
                                        inputPath = args[i];
                                        break;
                                    }

                                case "-o":
                                case "-out":
                                case "-output":
                                    {
                                        i++;
                                        outputDirectory = args[i];
                                        break;
                                    }

                                case "-ndec":
                                case "-nodecompression":
                                    {
                                        useDecompression = false;
                                        break;
                                    }
                            }
                        }

                        if (string.IsNullOrEmpty(inputPath))
                            throw new ArgumentNullException("inputPath", "Input path for CFC.DIG not valid.");

                        if (string.IsNullOrEmpty(outputDirectory))
                            throw new ArgumentNullException("outputDirectory", "Output directory for files not valid.");

                        CFCDIG.Unpack(inputPath, outputDirectory, useDecompression);
                        break;
                    }

                case "-p":
                    {
                        string inputDirectory = null;
                        string outputPath = null;
                        string[] filePathArray = null;

                        for (uint i = 1; i < args.Count(); i++)
                        {
                            string option = args[i].ToLower();

                            switch (option)
                            {
                                default:
                                    {
                                        Program.PrintHelp(true);
                                        return;
                                    }

                                case "-i":
                                case "-in":
                                case "-input":
                                    {
                                        i++;
                                        inputDirectory = args[i];
                                        break;
                                    }

                                case "-o":
                                case "-out":
                                case "-output":
                                    {
                                        i++;
                                        outputPath = args[i];
                                        break;
                                    }

                                case "-l":
                                case "-list":
                                case "-filelist":
                                    {
                                        i++;
                                        filePathArray = File.Exists(args[i]) ? File.ReadAllLines(args[i]) : null; // If filelist doesn't exist in FS, it's set to null
                                        break;
                                    }
                            }
                        }

                        if (string.IsNullOrEmpty(outputPath))
                            throw new ArgumentNullException("outputPath", "Output path for CFC.DIG not valid.");

                        if (string.IsNullOrEmpty(inputDirectory))
                            throw new ArgumentNullException("inputDirectory", "Input directory for files not valid.");

                        if (!Directory.Exists(inputDirectory))
                            throw new ArgumentException("inputDirectory", $"Input directory \"{inputDirectory}\" does not exist.");

                        CFCDIG.Write(inputDirectory, outputPath, filePathArray);
                        break;
                    }
            }
        }

        private static void PrintHelp(bool isError = false)
        {
            if (isError)
            {
                Console.WriteLine("\nInvalid usage");
            }

            Console.WriteLine("\nCFCDIGCli\nCopyright (c) 2020  SockNastre\nVersion: 1.0.0.0\n\n" +
                "Racjin (de)compression\nLink: https://github.com/Raw-man/Racjin-de-compression \nLicense (GPL-3.0): https://github.com/Raw-man/Racjin-de-compression/blob/master/LICENSE \n\n" +
                new string('-', 50) + "\n\nUsage: CFCDIGCli.exe <Command> <Options>\n\nCommands:\n-help (h)\n-unpack (-u)\n-pack (-p)\n\n" +
                "Unpack Options:\n-input (-i)\n-output (-o)\n-nodecompression (-ndec)\n\n" +
                "Pack Options:\n-input (-i)\n-output (-o)\n-filelist (-l)\n\n" +
                "Examples: \n\nCFCDIGCli.exe -u -i \"CFC.DIG\" -o \"output_folder\"\n" +
                new string(' ', 14) + "-p -i \"input_files\" -o \"CFC.DIG\"");
        }
    }
}
