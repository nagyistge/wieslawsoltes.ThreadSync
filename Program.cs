#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

#endregion

namespace ThreadSync
{
    #region Program

    class Program
    {
        private static void Main(string[] args)
        {
            Run();
        }

        private static void Run()
        {
            int count = 0;
            var view = CreateView();
            string line;

            while (true)
            {
                // handle event
                line = Console.ReadLine();
                count++;
                Console.Title = count.ToString();

                // check for quit command
                if (line.ToLower() == "q")
                {
                    view.Stop();
                    Console.WriteLine("Quit");
                    break;
                }

                // notify Draw next frame
                var result = view.HandleEvent(count, 16);
                if (result == false)
                    Console.WriteLine("Skip: " + count);
            }
        }

        private static BackgroundService<int> CreateView()
        {
            var view = new BackgroundService<int>();

            view.Start((data) =>
            {
                // draw frame
                Thread.Sleep(160);

                Console.WriteLine("Count: " + data);
            });

            return view;
        }
    } 

    #endregion
}
