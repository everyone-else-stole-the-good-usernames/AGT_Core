using OpenTK.Mathematics;

namespace AGTCoreServer
{
    public static class Constants
    {
        public static ushort port { get; set; }
        public static ushort maxPlayers { get; set; }
        static public Vector3 defaultSpawn = new Vector3(10000, 10, 10000);

        static Constants()
        {
            port = 8001;
            maxPlayers = 4;
        }
    }
}
