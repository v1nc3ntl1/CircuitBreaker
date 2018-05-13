using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CircuitBreaker
{
    public class CircuitController
    {
        private string currentState = State.Closed;
        private readonly int closedFailureThreshold = 10;
        private readonly int FailTimerInterval = 10;
        private readonly int HalfOpenedCounterThreshold = 10;
        private readonly int halfOpenedHitsWindows = 5;
        private Timer openedTimer;
        private Timer closedTimer;
        private int closedFailureCounter = 0;
        private int halfOpenedHits = 0;
        private object syncLock = new object();
        public IWatcher Watcher { get; }
        public Action FailedOperation { get; }

        public CircuitController(string initialState, IWatcher watcher, Action failedOperation)
        {
            openedTimer = new Timer(FailTimerInterval * 1000);
            openedTimer.Elapsed += ResetOpenedTimer;

            closedTimer = new Timer(FailTimerInterval * 1000);
            closedTimer.Elapsed += ResetClosedTimer;

            currentState = initialState;
            this.Watcher = watcher;
            this.FailedOperation = failedOperation;
        }

        private void ResetClosedTimer(object sender, ElapsedEventArgs e)
        {
            lock(this.syncLock)
            {
                closedFailureCounter = 0;
            }
        }

        private void ResetOpenedTimer(object sender, ElapsedEventArgs e)
        {
            lock (this.syncLock)
            {
                this.currentState = State.HalfOpened;
            }
        }

        public T Pass<T>(Func<T> operation)
        {
            if (currentState == State.Opened)
            {
                // todo: set a default value for fail circuit.
                return default(T);
            }

            PipelineData<T> data = null;

            if (currentState == State.HalfOpened)
            {
                halfOpenedHits++;
                if (halfOpenedHits % halfOpenedHitsWindows != 0)
                {
                    // todo: set a default value for fail circuit.
                    return default(T);
                }

                // Allow pass through for every halfOpenedHitsWindows times of operation.
                data = Watcher.Evaluate<T>(operation);

                if (data.Success)
                {
                    // If half opened succeed at least HalfOpenedCounterThreshold of times, change circuit to Closed.
                    if ((halfOpenedHits / halfOpenedHitsWindows) >= HalfOpenedCounterThreshold)
                    {
                        currentState = State.Closed;
                    }

                    return data.Value;
                }

                return default(T);
            }

            data = Watcher.Evaluate<T>(operation);

            if (data.Success)
            {
                if (currentState != State.Closed)
                {
                    lock (this.syncLock)
                    {
                        currentState = State.Closed;
                    }
                }

                return data.Value;
            }

            closedFailureCounter++;
            if (closedFailureCounter >= closedFailureThreshold)
            {
                lock (this.syncLock)
                {
                    currentState = State.Opened;
                }

                this.openedTimer.Start();
            }

            // todo: set a default value for fail circuit.
            return default(T);
        }
    }
}
