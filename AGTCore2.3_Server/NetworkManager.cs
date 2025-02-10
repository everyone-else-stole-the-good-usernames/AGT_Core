using Riptide;
using Riptide.Utils;

namespace AGTCoreServer
{

    public enum ServerMessageID : ushort
    {
        spawnPlayer = 1,
        position,
        block,
        chunkSend
    }

    public enum ClientMessageID : ushort
    {
        username = 1,
        position,
        block,
        chunkLoad,
        chunkUnload,
    }

    class NetworkManager : Updatable
    {

        public static Server server { get; set; }
        static ushort port = Constants.port;
        static ushort maxPlayers = Constants.maxPlayers;

        public NetworkManager() : base()
        {
            RiptideLogger.Initialize(Console.WriteLine, true);
            server = new Server();
            server.Start(port, maxPlayers);
            server.ClientDisconnected += PlayerDisconnected;
        }

        public override void Update()
        {
            server.Update();
        }

        static void Shutdown()
        {
            server.Stop();
        }

        private void PlayerDisconnected(object sender, ServerDisconnectedEventArgs e)
        {
            if (Player.players.ContainsKey(e.Client.Id))
            {
                Console.WriteLine($"{Player.players[e.Client.Id].username} left");
                Player.players[e.Client.Id].Dispose();
            }
        }
    }
}