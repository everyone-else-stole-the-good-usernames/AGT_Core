using System;

namespace AGTCore
{

    public abstract class Updatable
    {
        public static int tick;
        // adds an instance of this class to the list of updatables on instantiation 
        // public Updatable() // constructors of abstract classes are implicitly called in c# 
        // {

        // }
        public void beginUpdating()
        {
            Program.updatables.Add(this);
        }
        public virtual void Update() { } // this method is to be overrided 

        public virtual void OnDispose() { } // this method is called before disposal by the garbage collector


        // this section tells the garbage collector to unregister the updatable before discarding it
        private bool disposed = false;

        protected virtual void Dispose(bool _disposing)
        {
            if (!disposed)
            {
                if (_disposing) // disposes of the below variables
                {
                    Program.updatables.Remove(this); // removes instance from updatables list
                    OnDispose();
                }

                disposed = true;
            }
        }

        public void Dispose() // handles disposing of the object instead of the C# garbage collector
        {
            Dispose(true); // calls the above method
            GC.SuppressFinalize(this);
        }
    }
}
