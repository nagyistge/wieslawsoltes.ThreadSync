#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

#endregion

namespace ThreadSync
{
    #region DrawService

    public class DrawService<T>
    {
        #region Properties

        public volatile object Sync = new object();
        public bool IsRunning { get; private set; }
        public T Data { get; set; }
        public Action<T> Action { get; set; } 

        #endregion

        #region Running

        public void SetRunning(bool run)
        {
            IsRunning = run;
        }

        #endregion

        #region Draw Loop

        public void Loop()
        {
            while (IsRunning)
            {
                lock (Sync)
                {
                    // execute drawing action
                    if (Action != null)
                        Action(Data);

                    // wait for next frame notification
                    Monitor.Wait(Sync);
                }
            }
        } 

        #endregion
    }

    #endregion
}
