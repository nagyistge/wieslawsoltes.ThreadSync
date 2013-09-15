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
            var service = CreateService();
            string line;

            while (true)
            {
                // get event from user
                line = Console.ReadLine();
                count++;
                Console.Title = count.ToString();

                // check for quit command
                if (line.ToLower() == "q")
                {
                    service.Stop();
                    Console.WriteLine("Quit");
                    break;
                }

                // try to handle next frame
                var result = service.HandleEvent(count, 16);
                if (result == false)
                    Console.WriteLine("Skip: " + count);
            }
        }

        private static BackgroundService<int> CreateService()
        {
            var service = new BackgroundService<int>();

            service.Start((data) =>
            {
                // draw frame
                Thread.Sleep(160);

                Console.WriteLine("Count: " + data);
            });

            return service;
        }
    } 

    #endregion
}
