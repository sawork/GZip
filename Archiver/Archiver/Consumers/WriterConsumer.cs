using System.Collections.Generic;
using System.Threading;

namespace Archiver.Consumers
{
    public class WriterConsumer
    {
        private readonly object _writingLocker = new object ();
        private Dictionary<int, byte[]> _writingDictionary = new Dictionary<int, byte[]>();
        private bool _completed = false;
        private int _index = 0;

        public void Add(int chunkId, byte[] bytes)
        {
            lock (_writingLocker)
            {
                _writingDictionary.Add(chunkId, bytes);
                Monitor.PulseAll(_writingLocker);
            }
        }

        public bool TryGetValueByKey(out byte[] data)
        {
            lock (_writingLocker)
            {
                while (!_writingDictionary.ContainsKey(_index))
                {
                    if (_completed)
                    {
                        data = new byte[0];
                        return false;
                    }
                    Monitor.Wait(_writingLocker);                                        
                }
                data = _writingDictionary[_index++];
                Monitor.PulseAll(_writingLocker);
                return true;
            }
        }

        public void SetCompleted()
        {
            lock (_writingLocker)
            {
                _completed = true;
                Monitor.PulseAll(_writingLocker);
            }
        }
    }
}
