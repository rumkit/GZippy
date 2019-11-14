using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZippy
{
    class Dispatcher
    {
        public Dispatcher()
        {
            _workers = new Worker[Environment.ProcessorCount];
            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new Worker();
            }
        }


        private readonly Worker[] _workers;
        private readonly ConcurrentQueue<Worker> _activeJobs = new ConcurrentQueue<Worker>();        
        private readonly AutoResetEvent _resultReady = new AutoResetEvent(false);        


        private const long ChunkLength = 1024;
        private long _sourcePosition;
        private long _sourceLength;


        public void Process(Stream source, Stream destination)
        {
            _sourceLength = source.Length;
            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i].QueueJob(
                    (worker) => EnqueueWorkItem(worker, source),
                    (data) => Compress(data),
                    () => OnChunkCompleted(destination)
                    );
            }
            _resultReady.WaitOne();
        }

        private readonly object _completedLockRoot = new object();

        private void OnChunkCompleted(Stream destination)
        {
            lock(_completedLockRoot)
            { 
                while (IsNextChunkReady())
                {
                    if (_activeJobs.TryDequeue(out Worker worker))
                    {
                        var result = worker.GetResult();
                        destination.Write(result, 0, result.Length);
                        if (_sourcePosition >= _sourceLength && _activeJobs.Count == 0)
                        {
                            _resultReady.Set();
                        }
                    }
                }                
            }
        }

        private bool IsNextChunkReady()
        {
            if (_activeJobs.TryPeek(out Worker worker))
            {
                return worker.HasResult;
            }
            return false;
        }

        private byte[] Compress(byte[] data)
        {
            return data;
        }

        private byte[] EnqueueWorkItem(Worker worker, Stream source)
        {
            lock (_activeJobs)
            {
                if (_sourcePosition >= source.Length)
                    return null;
                var currentChunkLength = _sourcePosition + ChunkLength > source.Length ?
                    source.Length - source.Position :
                    ChunkLength;
                var payload = new byte[currentChunkLength];
                source.Read(payload, 0, payload.Length);
                _sourcePosition += payload.Length;
                _activeJobs.Enqueue(worker);
                return payload;
            }
        }
    }
}
