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
        public Dispatcher(ICompressionStrategy compressionStrategy)
        {
            _compressionStrategy = compressionStrategy;
            _workers = new Worker[Environment.ProcessorCount];
            for (int i = 0; i < _workers.Length; i++)
            {
                _workers[i] = new Worker();
            }
        }


        private readonly ICompressionStrategy _compressionStrategy;
        private readonly Worker[] _workers;
        private readonly ConcurrentQueue<Worker> _activeJobs = new ConcurrentQueue<Worker>();
        private readonly AutoResetEvent _chunkCompleted = new AutoResetEvent(false);

        private const long ChunkLength = 4096;
        private bool _endOfStream = false;


        public void Compress(Stream source, Stream destination)
        {
            foreach (var worker in _workers)
            {
                worker.QueueJob(
                    (w) => EnqueueWorkItem(w, source),
                    (data) => _compressionStrategy.Compress(data),
                    () => _chunkCompleted.Set()
                    );
            }
            WaitAndWriteResult(destination);
        }

        public void Decompress(Stream source, Stream destination)
        {
            foreach (var worker in _workers)
            {
                worker.QueueJob(
                    (w) => EnqueueWorkItem(w, source),
                    (data) => _compressionStrategy.Decompress(data),
                    () => _chunkCompleted.Set()
                    );
            }
            WaitAndWriteResult(destination);
        }

        //todo remove: private readonly object _completedLockRoot = new object();

        private void WaitAndWriteResult(Stream destination)
        {
            while (!_endOfStream || _activeJobs.Count > 0)
            {
                _chunkCompleted.WaitOne();

                while (IsNextChunkReady())
                {
                    if (_activeJobs.TryDequeue(out Worker worker))
                    {
                        chunksDequed++;
                        var result = worker.GetResult();
                        destination.Write(result, 0, result.Length);
                    }
                }
            }

            Console.WriteLine($"Queued {chunksQueued} Dequeued {chunksDequed}");

        }

        private bool IsNextChunkReady()
        {
            if (_activeJobs.TryPeek(out Worker worker))
            {
                return worker.HasResult;
            }
            return false;
        }

        private object enqueLockRoot = new object();
        private int chunksQueued;
        private int chunksDequed;
        private byte[] EnqueueWorkItem(Worker worker, Stream source)
        {
            lock (enqueLockRoot)
            {
                byte[] chunk = source.ReadChunk(ChunkLength);
                if (chunk == null)
                    _endOfStream = true;
                else
                {
                    _activeJobs.Enqueue(worker);
                    chunksQueued++;
                }

                return chunk;
            }
        }
    }
}
