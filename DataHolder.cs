#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

#endregion

namespace ThreadSync
{
    #region DataHolder

    public class DataHolder<T>
    {
        #region Properties

        public readonly object Sync = new object();
        public bool IsRunning { get; private set; }

        public T Data { get; private set; }
        public Action<T> Action { get; private set; } 

        #endregion

        #region Constructor

        public DataHolder(Action<T> action, bool isRunning)
        {
            Action = action;
            IsRunning = isRunning;
        } 

        #endregion

        #region Set

        public void SetAction(Action<T> action)
        {
            lock(Sync)
                Action = action;
        }

        public void SetRunning(bool isRunning)
        {
            IsRunning = isRunning;
        }

        public bool SetData(T data, int timeout)
        {
            if (Monitor.TryEnter(Sync, timeout) == false)
                return false;

            Data = data;

            Monitor.Pulse(Sync);
            Monitor.Exit(Sync);

            return true;
        }

        #endregion

        #region Run Loop

        public void Loop()
        {
            while (IsRunning)
            {
                lock (Sync)
                {
                    if (Action != null)
                        Action(Data);

                    Monitor.Wait(Sync);
                }
            }
        } 

        #endregion
    }

    #endregion
}
