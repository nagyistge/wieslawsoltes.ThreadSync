using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadSync
{
    public class Draw
    {
        public readonly object Sync = new object();
        public int Count { get; set; }

        public void Loop()
        {
            while (true)
            {
                lock (Sync)
                {
                    Thread.Sleep(16);
                    Console.WriteLine("Count: " + Count);

                    Monitor.Wait(Sync);
                }
            }
        }
    }

    public class View
    {
        public Draw draw;
        Thread thread;

        public View()
        {
            draw = new Draw();
            thread = new Thread(new ThreadStart(draw.Loop));

            thread.Start();
        }

        public void Stop()
        {
            thread.Join();
        }

        public void Notify()
        {
            lock (draw.Sync)
            {
                Monitor.Pulse(draw.Sync);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int count = 0;
            var view = new View();

            while(true)
            {
                var s = Console.ReadLine();
                count++;

                lock(view.draw.Sync)
                {
                    if (s == "q")
                    {
                        view.Stop();
                        Console.WriteLine("Done");
                        break;
                    }
                    else
                    {
                        view.draw.Count = count;
                        //view.Notify();
                        Monitor.Pulse(view.draw.Sync);
                    }
                }
            }
        }
    }
}
