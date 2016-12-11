using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kordon
{
    public sealed class Cord<T>
    {
        private readonly List<T> _executionArgs = new List<T>();
        private readonly List<Action<T>> _executionHandlers = new List<Action<T>>();
        private readonly AutoResetEvent _autoReset = new AutoResetEvent(false);
        private readonly object _syncObject = new object();
        private readonly Queue<T> _raiseQueue = new Queue<T>();
        private readonly List<Action<T>> _handlers = new List<Action<T>>();

        public Cord()
        {
            Task.Factory.StartNew(Executor, TaskCreationOptions.LongRunning);
        }

        public void Raise(T arg)
        {
            lock (_syncObject)
            {
                if (_handlers.Count == 0) { return; }

                _raiseQueue.Enqueue(arg);
                _autoReset.Set();
            }
        }

        public void Register(Action<T> handler)
        {
            lock (_syncObject)
            {
                if (handler == null) { return; }
                if (_handlers.Contains(handler)) { return; }
                else { _handlers.Add(handler); }
            }
        }

        public void Unregister(Action<T> handler)
        {
            lock (_syncObject)
            {
                if (handler == null) { return; }
                if (_handlers.Contains(handler)) { _handlers.Remove(handler); }
            }
        }

        private void Executor()
        {
            while (true)
            {
                _autoReset.WaitOne();
                _executionArgs.Clear();
                _executionHandlers.Clear();

                lock (_syncObject)
                {
                    while (_raiseQueue.Count > 0)
                    {
                        _executionArgs.Add(_raiseQueue.Dequeue());
                    }

                    _executionHandlers.AddRange(_handlers);
                }

                if (_executionArgs.Count == 0 || _executionHandlers.Count == 0) { return; }

                foreach (var handler in _executionHandlers)
                {
                    Task.Run(() =>
                    {
                        foreach (var arg in _executionArgs)
                        {
                            handler(arg);
                        }
                    });
                }
            }
        }
    }
}