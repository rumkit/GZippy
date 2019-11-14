using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZippy
{
    class Worker : IDisposable
    {
        public Worker()
        {
            _workerThread = new Thread(WorkerRoutine) { IsBackground = true };
            _readyToWork = new ManualResetEvent(false);
            _results = new ConcurrentQueue<byte[]>();
            _workerThread.Start();            
        }

        private void WorkerRoutine()
        {
            while(true)
            {                
                _readyToWork.WaitOne();                
                // Try to aquire new payload, if no payload then go to sleep
                var payload = _payloadSource(this);
                if(payload == null)
                {
                    State = WorkerState.Idle;
                    _readyToWork.Reset();
                    _payloadSource = null;
                    _job = null;
                    _jobCompleted = null;
                    continue;
                }
                var result = _job(payload);
                _results.Enqueue(result);
                _jobCompleted?.Invoke();                
            }
        }
      
        private readonly Thread _workerThread;
        private readonly ManualResetEvent _readyToWork;        
        private readonly ConcurrentQueue<byte[]> _results;        

        
        private Func<Worker,byte[]> _payloadSource;
        private Func<byte[],byte[]> _job;
        private Action _jobCompleted;
        

        
        public WorkerState State { get; private set; }

        /// <summary>
        /// Worker will query bytes from payloadSource and pass to job untill payloadSource returns null
        /// </summary>
        /// <param name="payloadSource">source of payload to work with</param>
        /// <param name="job">data processing routine</param>
        public void QueueJob(Func<Worker, byte[]> payloadSource, Func<byte[],byte[]> job, Action onComplete)
        {
            if(State != WorkerState.Idle)
                throw new ConcurrencyException("You have started new job without finishing the previos one.");
            _payloadSource = payloadSource;
            _job = job;
            _jobCompleted = onComplete;
            _readyToWork.Set();
        }        

        public bool HasResult => _results.Count > 0;
        public byte[] GetResult()
        {
            if(_results.TryDequeue(out byte[] result))
            {
                return result;
            }
            throw new ConcurrencyException("Result was requested before it is ready");
        }

        public void Dispose()
        {
            
        }       
    }

    enum WorkerState
    {
        Idle,
        Busy
    }
}
