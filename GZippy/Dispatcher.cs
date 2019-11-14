using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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


        public void Compress(Stream source, Stream destination)
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

        public void Decompress(Stream source, Stream destination)
        {
            _sourceLength = source.Length;
            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i].QueueJob(
                    (worker) => EnqueueWorkItem(worker, source),
                    (data) => Decompress(data),
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
            using(var ms = new MemoryStream())
            using (var zipStream = new GZipStream(ms, CompressionLevel.Optimal))
            {
                zipStream.Write(data,0,data.Length);
                zipStream.Flush();
                return ms.ToArray();
            }
        }

        private byte[] Decompress(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var zipStream = new GZipStream(ms, CompressionMode.Decompress))
            {
                var decompressed = new byte[ChunkLength];
                var count = zipStream.Read(decompressed,0,decompressed.Length);
                var ret = new byte[count];
                Buffer.BlockCopy(decompressed,0,ret,0,count);
                return ret;
            }
        }

        private byte[] EnqueueWorkItem(Worker worker, Stream source)
        {
            lock (_activeJobs)
            {
                if (_sourcePosition >= _sourceLength)
                    return null;
                var currentChunkLength = _sourcePosition + ChunkLength > source.Length ?
                    _sourceLength - source.Position :
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
