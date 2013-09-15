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
        private bool entered = false; 

        #endregion

        #region Lifecycle

        public void Start(Action<T> action)
        {
            if (thread == null)
            {
                draw = new DrawService<T>();
                draw.Action = action;
                draw.SetRunning(true);

                thread = new Thread(new ThreadStart(draw.Loop));
                thread.Start();
            }
        }

        public void Stop()
        {
            if (thread != null)
            {
                // stop thread
                draw.SetRunning(false);
                lock (draw.Sync)
                    Monitor.Pulse(draw.Sync);

                // join thread
                thread.Join();
                thread = null;

                // reset draw
                draw.Action = null;
                draw = null;
            }
        } 

        #endregion

        #region Event Handler

        public void HandleEvent(T data, int timeout)
        {
            // try to notify Draw next frame
            entered = Monitor.TryEnter(draw.Sync, timeout);
            if (entered)
            {
                // update Draw state using new event data
                draw.Data = data;

                // notify Draw next frame
                Monitor.Pulse(draw.Sync);

                // enable Draw next frame
                Monitor.Exit(draw.Sync);
            }
            else
                Console.WriteLine("Skip: " + data);
        } 

        #endregion
    }

    #endregion
}
