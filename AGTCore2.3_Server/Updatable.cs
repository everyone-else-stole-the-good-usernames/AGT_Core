namespace AGTCoreServer
{

    public abstract class Updatable
    {
        public static int tick;
        public Updatable()
        {
            Program.updatables.Add(this); // adds updatable to Update queue
        }
        public virtual void Update() { }

        public virtual void Destroy()
        {
            Console.WriteLine("disposing");
        }


        // this section tells the garbage collector to unregister the updatable before discarding it
        private bool disposed = false;

        protected virtual void Dispose(bool _disposing)
        {
            if (!disposed)
            {
                if (_disposing) // disposes of the below variables
                {
                    Program.updatables.Remove(this);
                    Destroy();
                }

                disposed = true;
            }
        }

        public void Dispose() // handles disposing of the object instead of the C# garbage collector
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}