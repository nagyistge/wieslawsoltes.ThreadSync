#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

#endregion

namespace ThreadSync
{
    #region ViewService

    public class ViewService<T>
    {
        #region Fields

        private DrawService<T> draw = null;
        private Thread thread = null;

        #endregion

        #region Lifecycle

        public void Start(Action<T> action)
        {
            if (thread != null)
                return;

            draw = new DrawService<T>(action, true);

            thread = new Thread(new ThreadStart(draw.Loop));
            thread.Start();
        }

        public void Stop()
        {
            if (thread == null)
                return;

            // stop thread
            draw.SetRunning(false);
            lock (draw.Sync)
                Monitor.Pulse(draw.Sync);

            thread.Join();

            // reset fields
            thread = null;
            draw = null;
        } 

        #endregion

        #region Event Handler

        public void HandleEvent(T data, int timeout)
        {
            // try to notify ro Draw next frame
            var entered = Monitor.TryEnter(draw.Sync, timeout);
            if (entered == false)
            {
                Console.WriteLine("Skip: " + data);
                return;
            }

            // update Draw data
            draw.Data = data;

            // notify to Draw next frame
            Monitor.Pulse(draw.Sync);

            // enable Draw next frame
            Monitor.Exit(draw.Sync);  
        } 

        #endregion
    }

    #endregion
}
