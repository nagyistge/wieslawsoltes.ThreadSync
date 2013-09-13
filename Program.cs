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
        public readonly object Sync = new object();
        private bool isRunning = false;

        public T Data { get; set; }
        public Action<T> Action { get; set; }

        public void SetRunning(bool run)
        {
            isRunning = run;
        }

        public void Loop()
        {
            while (isRunning)
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
    } 

    #endregion

    #region ViewService

    public class ViewService<T>
    {
        private DrawService<T> draw;
        private Thread thread;
        private bool entered;

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
    } 

    #endregion

    #region Program

    class Program
    {
        static void Main(string[] args)
        {
            int count = 0;

            var view = new ViewService<int>();
            view.Start((data) =>
            {
                // draw frame
                Thread.Sleep(160);

                Console.WriteLine("Count: " + data);
            });

            while (true)
            {
                // handle event
                var s = Console.ReadLine();
                count++;
                Console.Title = count.ToString();

                // check for quit command
                if (s.ToLower() == "q")
                {
                    view.Stop();
                    Console.WriteLine("Quit");
                    break;
                }

                // notify Draw next frame
                view.HandleEvent(count, 16);
            }
        }
    } 

    #endregion
}
