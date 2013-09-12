using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadSync
{
    public class DrawService
    {
        public readonly object Sync = new object();
        public int Count { get; set; }

        public void Loop()
        {
            while (true)
            {
                lock (Sync)
                {
                    // simulate drawing canvas
                    Thread.Sleep(160);
                    Console.WriteLine("Count: " + Count);

                    // wait for next frame notifications
                    Monitor.Wait(Sync);
                }
            }
        }
    }

    public class ViewService
    {
        private DrawService draw;
        private Thread thread;
        private bool entered;

        public void Start()
        {
            if (thread == null)
            {
                draw = new DrawService();
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

        public void HandleEvent(int count)
        {
            // try to notify Draw next frame
            entered = Monitor.TryEnter(draw.Sync, 16);
            if (entered)
            {
                draw.Count = count;

                Monitor.Pulse(draw.Sync);
                Monitor.Exit(draw.Sync);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int count = 0;

            var view = new ViewService();
            view.Start();

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
