using Archiver.Constants;
using Archiver.GZipArchiver;
using Archiver.Models;
using System;
using System.IO;

namespace Archiver.Archiver
{
    public class Compressor : BaseArchiver
    {       
        public Compressor(string inputFilePath, string outputFilePath) 
            : base(inputFilePath, outputFilePath)
        {
        }       

        protected override void ReadInputFile()
        {
            try
            {
                var fileInfo = new FileInfo(InputFilePath);
                var fileSize = fileInfo.Length;

                using (var sourceStream = new FileStream(InputFilePath, FileMode.Open, FileAccess.Read))
                using (var binaryReader = new BinaryReader(sourceStream))
                {
                    var chunkId = 0;
                    while (fileSize > 0 && !hasError)
                    {
                        var currentChunkSize = fileSize > Const.ChunkSize ? Const.ChunkSize : fileSize;
                        var bytes = binaryReader.ReadBytes((int)currentChunkSize);
                        InputQueue.Enqueue(new Chunk(chunkId++, bytes));
                        fileSize -= currentChunkSize;
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

        protected override void Process(int threadId)
        {
            try
            {
                while (InputQueue.TryDequeue(out Chunk chunk) && !hasError)
                {
                    var compressedChunk = GZip.CompressBlock(chunk.Bytes);
                    OutputDictionary.Add(chunk.Id, compressedChunk);
                }
                processEvents[threadId].Set();
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
                        binaryWriter.Write(data.Length);
                        binaryWriter.Write(data, 0, data.Length);
                    }
                }
            }
            catch(Exception e)
            {
                hasError = true;
            }
        }         
    }
}
