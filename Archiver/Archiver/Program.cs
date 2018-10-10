using Archiver.Archiver;
using System;
using System.IO;

namespace Archiver
{
    class Program
    {
        static void Main(string[] args)
        {
            if (ValidateInputArgs(args))
            {
                BaseArchiver archiver;
                if (args[0].ToLower() == "compress")
                {
                    archiver = new Compressor(args[1], args[2]);
                }
                else
                {
                    archiver = new Decompressor(args[1], args[2]);
                }
                archiver.Start();                
            }
            Console.ReadKey();
        }

        private static bool ValidateInputArgs(string[] args)
        {
            if (args?.Length != 3)
            {
                Console.WriteLine("Incorrect number of parameters");
                return false;
            }

            if (args[0].ToLower() != "compress" && args[0].ToLower() != "decompress")
            {
                Console.WriteLine("Invalid first argument");
            }

            var inputFileInfo = new FileInfo(args[1]);
            if (!inputFileInfo.Exists || inputFileInfo.Length == 0)
            {
                Console.WriteLine("Invalid second argument");
                return false;
            }

            var outputFileInfo = new FileInfo(args[2]);
            if (outputFileInfo.Exists)
            {
                Console.WriteLine("Invalid third argument");
                return false;
            }

            return true;
        }
    }
}
