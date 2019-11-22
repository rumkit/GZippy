using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZippy
{
    [DebuggerDisplay("State = {State} WorkerId = {_workerThread.ManagedThreadId}")]
    class Worker
    {
        private readonly Thread _workerThread;
        private readonly ManualResetEvent _readyToWork;
        private readonly ConcurrentQueue<byte[]> _results;
        private readonly int _maxResults;

        private Func<Worker, byte[]> _payloadSource;
        private Func<byte[], byte[]> _job;
        private Action _jobCompleted;

        /// <summary>
        /// Current worker state.
        /// </summary>
        public WorkerState State { get; private set; }
        /// <summary>
        /// Returns true if there are any queued results.
        /// </summary>
        public bool HasResult => _results.Count > 0;

        public Worker(int maxResults = 10)
        {
            _maxResults = maxResults;
            _workerThread = new Thread(WorkerRoutine) { IsBackground = true };
            _readyToWork = new ManualResetEvent(false);
            _results = new ConcurrentQueue<byte[]>();
            _workerThread.Start();
        }

        private void WorkerRoutine()
        {
            var sw = new SpinWait();
            while (true)
            {
                _readyToWork.WaitOne();
                // Try to aquire new payload, if no payload then go to sleep
                byte[] payload = _payloadSource(this);
                if (payload == null)
                {
                    _readyToWork.Reset();
                    _payloadSource = null;
                    _job = null;
                    _jobCompleted = null;
                    State = WorkerState.Idle;
                    continue;
                }
                byte[] result = _job(payload);
                while (_results.Count > _maxResults)
                    sw.SpinOnce();
                _results.Enqueue(result);
                _jobCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Worker will query bytes from payloadSource and pass to job untill payloadSource returns null
        /// </summary>
        /// <param name="payloadSource">source of payload to work with</param>
        /// <param name="job">data processing routine</param>
        public void QueueJob(Func<Worker, byte[]> payloadSource, Func<byte[], byte[]> job, Action onComplete)
        {
            if (State != WorkerState.Idle)
                throw new ConcurrencyException("You have started new job without finishing the previous one.");
            _payloadSource = payloadSource;
            _job = job;
            _jobCompleted = onComplete;
            State = WorkerState.Busy;
            _readyToWork.Set();
        }

        /// <summary>
        /// Return queued result. You should always check <see cref="HasResult"/> before requesting result.
        /// </summary>
        /// <returns>byte array of proccessed data</returns>
        public byte[] GetResult()
        {
            if (_results.TryDequeue(out byte[] result))
            {
                return result;
            }
            throw new ConcurrencyException("Result was requested before it is ready");
        }
    }
}
