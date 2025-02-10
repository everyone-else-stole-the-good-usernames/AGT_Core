using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace AGTCoreServer
{
    class Program
    {
        public static List<Updatable> updatables = new List<Updatable>();
        static bool isRunning = false;
        public static TerrainManager terrainManager = new TerrainManager();
        static void Main(string[] args)
        {
            isRunning = true;
            NetworkManager networkManager = new NetworkManager();
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
            Console.ReadKey();
            isRunning = false;
        }
        private static void MainThread()
        {
            Console.WriteLine("Main thread started");
            DateTime nextTickTime = DateTime.Now;

            // this loop works by comparing the next loop's time to the current time
            // a builtin method such as Timers.Timer could work, but it doesnt wait for each game tick
            // forcing the timer to wait has the same overhead as implementing it myself, using DateTime.Now
            while (isRunning)
            {
                while (nextTickTime < DateTime.Now) // a while loop is used, so the server catches up in the event of a slowdown
                {
                    for (int x = 0; x < updatables.Count; x++) // calls all classes with Update() methods
                    {
                        updatables[x].Update();
                    }

                    nextTickTime = nextTickTime.AddMilliseconds(50);

                    if (nextTickTime > DateTime.Now)
                    {
                        Thread.Sleep(nextTickTime - DateTime.Now); // waits for next game tick
                    }
                }
            }
            for (int x = 0; x < updatables.Count; x++) // calls all classes with Update() methods
            {
                updatables[x].Dispose();
            }
            Console.WriteLine("Main thread terminated");
        }
    }

}