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
        private const long ChunkLength = 2048;

        private readonly ICompressionStrategy _compressionStrategy;
        private readonly Worker[] _workers;
        private readonly ConcurrentQueue<Worker> _activeJobs = new ConcurrentQueue<Worker>();
        private readonly AutoResetEvent _chunkCompleted = new AutoResetEvent(false);
        private object _enqueLockRoot = new object();
        private bool _endOfStream = false;        

        public Dispatcher(ICompressionStrategy compressionStrategy)
        {
            _compressionStrategy = compressionStrategy;
            _workers = new Worker[Environment.ProcessorCount];
            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new Worker();
            }
        }

        /// <summary>
        /// Compresses data from <see cref="source"> stream and writes result to <see cref="destination"/> stream
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public void Compress(Stream source, Stream destination)
        {            
            foreach (var worker in _workers)
            {
                worker.QueueJob(
                    (w) => EnqueueWorkItem(w, ()=> source.ReadChunk(ChunkLength)),
                    (data) => _compressionStrategy.Compress(data),
                    () => _chunkCompleted.Set()
                    );
            }
            WaitAndWriteResult(destination);
        }

        /// <summary>
        /// Decompresses data from <see cref="source"> stream and writes result to <see cref="destination"/> stream
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public void Decompress(Stream source, Stream destination)
        {            
            foreach (var worker in _workers)
            {
                worker.QueueJob(
                    (w) => EnqueueWorkItem(w, () => _compressionStrategy.ParseCompressedStream(source)),
                    (data) => _compressionStrategy.Decompress(data),
                    () => _chunkCompleted.Set()
                    );
            }
            WaitAndWriteResult(destination);
        }

        private void WaitAndWriteResult(Stream destination)
        {
            while (!_endOfStream || _activeJobs.Count > 0)
            {
                _chunkCompleted.WaitOne(100);
                while (IsNextChunkReady())
                {
                    if (_activeJobs.TryDequeue(out Worker worker))
                    {
                        var result = worker.GetResult();
                        destination.Write(result, 0, result.Length);
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

        private byte[] EnqueueWorkItem(Worker worker, Func<byte[]> bytesSource)
        {
            lock (_enqueLockRoot)
            {
                byte[] chunk = bytesSource();
                if (chunk == null)
                    _endOfStream = true;
                else
                {
                    _activeJobs.Enqueue(worker);
                }
                return chunk;
            }
        }
    }
}
