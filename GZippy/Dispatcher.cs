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
        private readonly AutoResetEvent _chunkReady = new AutoResetEvent(false);

        private const long ChunkLength = 512;        
        private long sourcePosition;              


        public void Compress(Stream source, Stream destination)
        {   
            for(int i = 0; i < _workers.Length; i++)
            {
                _workers[i].QueueJob(
                    (worker) =>  EnqueueWorkItem(worker,source),
                    (data) => Compress(data),
                    OnChunkCompleted
                    );                    
            }
        }

        private void OnChunkCompleted()
        {
            
        }

        private bool IsNextChunkReady()
        {
            Worker worker;
            if(_activeJobs.TryPeek(out worker))
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
                if (sourcePosition >= source.Length)
                    return null;
                var currentChunkLength = sourcePosition + ChunkLength > source.Length ?
                    source.Length - source.Position :
                    ChunkLength;
                var payload = new byte[currentChunkLength];
                source.Read(payload,0,payload.Length);
                _activeJobs.Enqueue(worker);
                return payload;
            }
        }
    }
}
