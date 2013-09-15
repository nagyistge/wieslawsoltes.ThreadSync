#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

#endregion

namespace ThreadSync
{
    #region BackgroundService

    public class BackgroundService<T>
    {
        #region Fields

        private DataHolder<T> holder = null;
        private Thread thread = null;

        #endregion

        #region Lifecycle

        public void Start(Action<T> action)
        {
            if (thread != null)
                return;

            holder = new DataHolder<T>(action, true);

            thread = new Thread(new ThreadStart(holder.Loop));
            thread.Start();
        }

        public void Stop()
        {
            if (thread == null)
                return;

            holder.SetRunning(false);
            lock (holder.Sync)
                Monitor.Pulse(holder.Sync);

            thread.Join();

            thread = null;
            holder = null;
        } 

        #endregion

        #region Event Handler

        public bool HandleEvent(T data, int timeout)
        {
            return holder != null ? holder.SetData(data, timeout) : false;
        } 

        #endregion
    }

    #endregion
}
