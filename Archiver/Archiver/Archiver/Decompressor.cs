using Archiver.GZipArchiver;
using Archiver.Models;
using System;
using System.IO;

namespace Archiver.Archiver
{
    public class Decompressor : BaseArchiver
    {
        public Decompressor(string inputFilePath, string outputFilePath)
            : base(inputFilePath, outputFilePath)
        {
        }

        protected override void ReadInputFile()
        {
            try
            {
                using (var sourceStream = new FileStream(InputFilePath, FileMode.Open, FileAccess.Read))
                using (var binaryReader = new BinaryReader(sourceStream))
                {
                    var fileInfo = new FileInfo(InputFilePath);
                    var fileSize = fileInfo.Length;

                    const int intSize = 4;
                    var chunkId = 0;                    

                    while (fileSize > 0 && !hasError)
                    {
                        var chunkSize = binaryReader.ReadInt32();
                        var bytes = binaryReader.ReadBytes(chunkSize);
                        InputQueue.Enqueue(new Chunk(chunkId++, bytes));
                        fileSize -= (chunkSize + intSize);
                        if (fileSize == 0)
                        {
                            InputQueue.ReadComplete();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                hasError = true;
            }
        }

        protected override void WriteOutputFile()
        {
            try
            {
                using (var destinationStream = new FileStream(OutputFilePath, FileMode.Create, FileAccess.Write))
                using (var binaryWriter = new BinaryWriter(destinationStream))
                {
                    while (OutputDictionary.TryGetValueByKey(out var data) && !hasError)
                    {
                        binaryWriter.Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception e)
            {
                hasError = true;
            }
        }

        protected override void Process(int threadId)
        {
            try
            {
                while (InputQueue.TryDequeue(out Chunk chunk) && !hasError)
                {
                    var decompressedChunkData = GZip.DecompressBlock(chunk.Bytes);
                    OutputDictionary.Add(chunk.Id, decompressedChunkData);
                }
                processEvents[threadId].Set();
            }
            catch (Exception e)
            {
                hasError = true;
            }            
        }  
    }
}
