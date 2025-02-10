using Riptide;
using OpenTK.Mathematics;

namespace AGTCoreServer
{
    public class Player : Updatable
    {
        public static Dictionary<ushort, Player> players = new Dictionary<ushort, Player>();
        public ushort id { get; set; }
        public string username { get; set; }
        public Queue<Vector2i> chunksToLoad;
        public HashSet<Vector2i> occupiedChunks;
        // public Queue<Vector2i> chunksToUnload;
        Vector3 position; Vector3 rotation;

        Player(ushort _id, string _username, Vector3 _spawnPos)
        {
            position = _spawnPos;
            username = _username;
            id = _id;
            chunksToLoad = new Queue<Vector2i>();
            occupiedChunks = new HashSet<Vector2i>();
            // chunksToUnload = new Queue<Vector2i>();
        }

        public override void Update()
        {
            SendPosToAll(position, rotation);
            // removes requested chunks from chunksToDelete set
            while (chunksToLoad.TryDequeue(out var chunkKey))
            {
                Console.WriteLine($"processing request {chunkKey}");
                if (Program.terrainManager.loadedChunks.TryGetValue(chunkKey, out var chunk))
                {
                    chunk.SendBlockStream(id);
                }
                else
                {
                    chunk = new Chunk(chunkKey);
                    if (Program.terrainManager.loadedChunks.TryAdd(chunkKey, chunk))
                        chunk.SendBlockStream(id);
                }
                occupiedChunks.Add(chunkKey);
            }
            // while (chunksToUnload.TryDequeue(out var chunkKey))
            // {
            //     Console.WriteLine($"processing request {chunkKey}");
            //     if (Program.terrainManager.loadedChunks.TryGetValue(chunkKey, out var chunk))
            //     {
            //         chunk.SendBlockStream(id);
            //     }
            //     else
            //     {
            //         chunk = new Chunk(chunkKey);
            //         if (Program.terrainManager.loadedChunks.TryAdd(chunkKey, chunk))
            //             chunk.SendBlockStream(id);
            //     }
            // }
        }

        // lets all other players know this player's position
        void SendPosToAll(Vector3 _pos, Vector3 _rot)
        {
            Message message = Message.Create(MessageSendMode.Unreliable, ServerMessageID.position);
            message.AddUShort(id);
            message.AddVector3(_pos);
            message.AddVector3(_rot);
            NetworkManager.server.SendToAll(message, id);
        }


        public override void Destroy()
        {
            Console.WriteLine($"{username} disconnected");
            players.Remove(id);
            base.Destroy();
        }

        public static void SpawnPlayer(ushort _id, string _username)
        {
            // sends info about the existing players to the new player
            foreach (Player existingPlayer in players.Values)
            {
                existingPlayer.SendSpawn(_id);
            }
            // // tries to get player from saved players dict by recieved username
            // if (!FileHandler.savedPlayers.TryGetValue(_username, out var player)) // if not successful
            // {
            // }
            // creates a new player object with the recieved id and username at the default spawn point
            var player = new Player(_id, _username, Constants.defaultSpawn);
            // sends data about the newly connected player to all existing players
            player.SendSpawn();
            // adds player to current player list
            players.Add(_id, player);
        }

        void SendSpawn() // sends data needed to spawn this player to all players
        {
            Console.WriteLine($"Spawning {username}");
            // sends info relating to the new player to all players
            Message message = Message.Create(MessageSendMode.Reliable, (ushort)ServerMessageID.spawnPlayer);
            message.AddUShort(id);
            message.AddString(username);
            message.AddVector3(position);

            NetworkManager.server.SendToAll(message);
        }

        void SendSpawn(ushort _id) // sends data needed to spawn this player to a player with a specific id
        {
            {
                Console.WriteLine($"Spawning {username}");
                // sends info relating to the new player to all players
                Message message = Message.Create(MessageSendMode.Reliable, ServerMessageID.spawnPlayer);
                message.AddUShort(id);
                message.AddString(username);
                message.AddVector3(position);

                NetworkManager.server.Send(message, _id);
            }
        }

        // tells riptide to call the method below after a username message is recieved
        [MessageHandler((ushort)ClientMessageID.username)]
        private static void UsernameRecieved(ushort _clientId, Message message) // after recieving the username, player data is exchanged
        {
            SpawnPlayer(_clientId, message.GetString());
        }

        [MessageHandler((ushort)ClientMessageID.position)]
        private static void UpdatePosition(ushort _clientId, Message message) // updates appropriate player's position
        {
            players[_clientId].position = message.GetVector3();
            players[_clientId].rotation = message.GetVector3();
        }

        [MessageHandler((ushort)ClientMessageID.chunkLoad)]
        private static void ChunkRequestRecieved(ushort _clientId, Message message) // updates appropriate player's position
        {
            Player.players[_clientId].chunksToLoad.Enqueue(message.GetVector2i());
        }

        [MessageHandler((ushort)ClientMessageID.chunkUnload)]
        private static void ChunkUnloadRecieved(ushort _clientId, Message message) // updates appropriate player's position
        {
            if (Program.terrainManager.loadedChunks.TryGetValue(message.GetVector2i(), out var chunk))
                chunk.players.Remove(_clientId);
        }
    }
}