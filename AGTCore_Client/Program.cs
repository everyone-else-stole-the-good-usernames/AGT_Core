using System.Threading;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AGTCore
{
    // this is the entry point into the program
    // it starts the game thread, and the network thread
    // IF YOU ARE READING THIS: hello, hope you are doing well and got more sleep yesterday than I got this past week
    static class Program
    {
        public static List<Updatable> updatables = new List<Updatable>(); // list of objects to be updated in the Network Thread
        static bool NetworkThreadRunning = false;
        public static bool StartGameWindow = false;
        public static NetworkManager networkManager;
        public static Menu menu;
        static void Main(string[] args)
        {
            NetworkThreadRunning = true;
            Thread networkThread = new Thread(new ThreadStart(Program.NetworkThread));
            networkThread.Start();
            networkManager = new NetworkManager();
            menu = new Menu();
            Application.Run(menu);

            if (StartGameWindow)
                // This line creates a game new instance, and wraps the instance in a using statement so it's automatically disposed once we've exited the block.
                using (Game game = new Game(1920, 1080, "AGT Core 2.3"))
                {
                    game.Run();
                }
            // System.Console.WriteLine("Render threads terminated");
            NetworkThreadRunning = false;
        }

        // used to communicate with server, and update updatable classes
        public static void NetworkThread()
        {
            // System.Console.WriteLine("Network thread started");
            DateTime nextTickTime = DateTime.Now;

            // this loop works by comparing the next loop's time to the current time
            // a builtin method such as Timers.Timer could work, but it doesnt wait for each game tick
            // forcing the timer to wait has the same overhead as implementing it myself, using DateTime.Now
            while (NetworkThreadRunning)
            {
                while (nextTickTime < DateTime.Now) // a while loop is used, so the server catches up in the event of a slowdown
                {
                    Updatable.tick++;
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
            // System.Console.WriteLine("Network threads terminated");
        }
    }
}