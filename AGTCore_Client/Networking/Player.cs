using OpenTK.Mathematics;
using System.Collections.Generic;
using Riptide;

namespace AGTCore
{
    public abstract class Player : Updatable
    {
        public static Dictionary<ushort, Player> players = new Dictionary<ushort, Player>();
        public ushort id { get; set; }
        public string username { get; set; }
        public Vector3 playerPos;
        public Vector3 playerRot;
        protected Vector3 lastPos; // used for interpolation

        public Player(ushort _id, string _username, Vector3 _spawnPos)
        {
            playerPos = _spawnPos;
            username = _username;
            id = _id;
        }

        public override void OnDispose() // removes player from list if this object is destroyed (due to disconnection)
        {
            players.Remove(id);
            base.OnDispose();
        }

        static void SpawnPlayer(ushort _id, string _username, Vector3 _spawnPos)
        {
            Player player;
            if (_id == NetworkManager.client.Id)
            {
                player = new LocalPlayer(_id, _username, _spawnPos); // sets local to true if the recieved id matches the local id
            }
            else
            {
                player = new NetworkPlayer(_id, _username, _spawnPos);
            }
            players.Add(_id, player); // adds recieved player to player dict

        }

        [MessageHandler((ushort)ServerMessageID.spawnPlayer)]
        private static void SpawnRecieved(Message message)
        {
            // System.Console.WriteLine("Connection Accepted");
            SpawnPlayer(message.GetUShort(), message.GetString(), message.GetVector3());
        }
        public float rotY; public float headRotY; public float headRotZ;

        [MessageHandler((ushort)ServerMessageID.position)]
        private static void PositionRecieved(Message message) // updates appropriate player's position
        {
            if (players.TryGetValue(message.GetUShort(), out Player _player)) // this tries to get a player using the id from the message
            {
                _player.lastPos = _player.playerPos;
                _player.playerPos = message.GetVector3(); // if successful, it updates the player's position
                _player.rotY = message.GetFloat(); _player.headRotY = message.GetFloat(); _player.headRotZ = message.GetFloat();
            }
        }

        [MessageHandler((ushort)ServerMessageID.block)]
        private static void BlockUpdateRecieved(Message message) // updates appropriate player's position
        {   // message order = vec3i pos, byte block, int tick
            Game.terrainManager.setBlock(message.GetVector3i(), message.GetByte(), message.GetInt());
        }

    }
}