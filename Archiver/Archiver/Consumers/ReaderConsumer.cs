using Archiver.Models;
using System.Collections.Generic;
using System.Threading;

namespace Archiver.Consumers
{
    public class ReaderConsumer
    {
        private Queue<Chunk> _readerQueue = new Queue<Chunk>();
        private readonly object _readingLocker = new object();
        private bool _readingComplete = false;

        public void Enqueue(Chunk chunk)
        {
            lock (_readingLocker)
            {
                _readerQueue.Enqueue(chunk);
                Monitor.PulseAll(_readingLocker);
            }
        }

        public bool TryDequeue(out Chunk chunk)
        {
            lock (_readingLocker)
            {
                while (_readerQueue.Count == 0)
                {
                    if (_readingComplete)
                    {
                        chunk = new Chunk();
                        return false;
                    }
                    Monitor.Wait(_readingLocker);
                }
                chunk = _readerQueue.Dequeue();
                Monitor.PulseAll(_readingLocker);
                return true;
            }
        }

        public void ReadComplete()
        {
            lock (_readingLocker)
            {
                _readingComplete = true;
                Monitor.PulseAll(_readingLocker);
            }
        }
    }
}
