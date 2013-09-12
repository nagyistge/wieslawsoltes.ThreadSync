using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadSync
{
    public class DrawService<T>
    {
        public readonly object Sync = new object();
        public T Data { get; set; }
        public Action<T> Action { get; set; }

        public void Loop()
        {
            while (true)
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

                thread = new Thread(new ThreadStart(draw.Loop));
                thread.Start();
            }
        }

        public void Stop()
        {
            if (thread != null)
            {
                thread.Join();
                thread = null;
                draw = null;
            }
        }

        public void HandleEvent(T data)
        {
            // try to notify Draw next frame
            entered = Monitor.TryEnter(draw.Sync, 16);
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

    class Program
    {
        static void Main(string[] args)
        {
            int count = 0;

            var view = new ViewService<int>();
            view.Start((c) =>
            {
                Thread.Sleep(160);
                Console.WriteLine("Count: " + c);
            });

            while(true)
            {
                // handle event
                var s = Console.ReadLine();
                count++;
                Console.Title = count.ToString();

                // check for quit command
                if (s == "q")
                {
                    view.Stop();
                    Console.WriteLine("Quit");
                    break;
                }

                // notify Draw next frame
                view.HandleEvent(count);
            }
        }
    }
}
