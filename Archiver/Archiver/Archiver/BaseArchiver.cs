using Archiver.Consumers;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Archiver.Archiver
{
    public abstract class BaseArchiver
    {
        protected string InputFilePath { get; set; }

        protected string OutputFilePath { get; set; }

        protected ReaderConsumer InputQueue { get; set; }

        protected WriterConsumer OutputDictionary { get; set; }

        protected AutoResetEvent[] processEvents = new AutoResetEvent[Environment.ProcessorCount];

        protected volatile bool hasError = false;

        public BaseArchiver(string inputFilePath, string outputFilePath)
        {
            InputFilePath = inputFilePath;
            OutputFilePath = outputFilePath;
            InputQueue = new ReaderConsumer();
            OutputDictionary = new WriterConsumer();
        }

        public void Start()
        {
            var readingThread = new Thread(ReadInputFile);
            var compressingThreads = new List<Thread>();
            for (var i = 0; i < Environment.ProcessorCount; i++)
            {
                var j = i;
                processEvents[j] = new AutoResetEvent(false);
                compressingThreads.Add(new Thread(() => Process(j)));
            }
            var writingThread = new Thread(WriteOutputFile);

            readingThread.Start();

            foreach (var compressThread in compressingThreads)
            {
                compressThread.Start();
            }

            writingThread.Start();

            WaitHandle.WaitAll(processEvents);
            OutputDictionary.SetCompleted();

            writingThread.Join();
            Console.WriteLine(!hasError ? "Successfully competed" : "Error");
        }

        protected abstract void ReadInputFile();

        protected abstract void Process(int threadId);

        protected abstract void WriteOutputFile();
    }
}
