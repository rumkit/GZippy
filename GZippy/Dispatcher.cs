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
        // 1 Mb
        private const long ChunkLength = 1_048_576;

        private readonly ICompressionStrategy _compressionStrategy;
        private readonly IFileFormatter _fileFormatter;
        private readonly Worker[] _workers;
        private readonly ConcurrentQueue<Worker> _activeJobs = new ConcurrentQueue<Worker>();
        private readonly AutoResetEvent _chunkCompleted = new AutoResetEvent(false);
        private readonly object _enqueueLockRoot = new object();
        private bool _endOfStream = false;        

        public Dispatcher(ICompressionStrategy compressionStrategy, IFileFormatter fileFormatter)
        {
            _compressionStrategy = compressionStrategy;
            _fileFormatter = fileFormatter;
            _workers = new Worker[Environment.ProcessorCount];
            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new Worker();
            }
        }

        /// <summary>
        /// Compresses data from <see cref="source"/> stream and writes result to <see cref="destination"/> stream
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
            var offsets = WaitAndWriteResult(destination);
            _fileFormatter.WriteMetadata(destination, offsets);
        }

        /// <summary>
        /// Decompresses data from <see cref="source"/> stream and writes result to <see cref="destination"/> stream
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public void Decompress(Stream source, Stream destination)
        {
            _fileFormatter.ReadMetadata(source);
            foreach (var worker in _workers)
            {
                worker.QueueJob(
                    (w) => EnqueueWorkItem(w, () => _fileFormatter.ParseCompressedStream(source)),
                    (data) => _compressionStrategy.Decompress(data),
                    () => _chunkCompleted.Set()
                    );
            }
            WaitAndWriteResult(destination);
        }

        private IEnumerable<long> WaitAndWriteResult(Stream destination)
        {
            var chunkOffsets = new List<long>();
            while (!_endOfStream || _activeJobs.Count > 0)
            {
                _chunkCompleted.WaitOne(100);
                while (IsNextChunkReady())
                {
                    if (_activeJobs.TryDequeue(out Worker worker))
                    {
                        var result = worker.GetResult();
                        chunkOffsets.Add(destination.Position);
                        destination.Write(result, 0, result.Length);                        
                    }
                }
            }
            return chunkOffsets;
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
            lock(_enqueueLockRoot)
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
