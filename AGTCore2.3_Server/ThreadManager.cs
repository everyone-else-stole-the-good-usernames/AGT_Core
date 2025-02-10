using System;
using System.Collections.Generic;
using System.Text;

namespace AGTCoreServer
{
    class ThreadManager
    {
        private static readonly List<Action> MainThreadQueue = new List<Action>(); // queue of tasks to execute on main thread
        private static readonly List<Action> MainThreadQueueCopy = new List<Action>();
        private static bool actionToExecuteOnMainThread = false; // true if there is something to execute

        // adds a new task to be executed on the main thread
        public static void MainThreadEnQueue(Action _task)
        {
            lock (MainThreadQueue) // lock wrestles control of a variable
            {
                MainThreadQueue.Add(_task);
                actionToExecuteOnMainThread = true;
            }
        }

        // runs the enqueued tasks
        public static void Update() // should only be called from the main thread
        {
            if (actionToExecuteOnMainThread)
                // copies MainThreadQueue so that it can be accessed by other threads
                MainThreadQueueCopy.Clear();
            lock (MainThreadQueue) // MainThreadQueue is only locked during copying, not during execution of its tasks
            {
                MainThreadQueueCopy.AddRange(MainThreadQueue);
                MainThreadQueue.Clear();
                actionToExecuteOnMainThread = false;
            }

            for (int i = 0; i < MainThreadQueueCopy.Count; i++) // executes tasks
            {
                MainThreadQueueCopy[i]();
            }
        }
    }
}