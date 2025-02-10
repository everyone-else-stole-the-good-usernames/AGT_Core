using OpenTK.Mathematics;
using MemoryPack;
using System.Collections.Generic;
using Riptide;
namespace AGTCoreServer
{
    public class Chunk
    {
        public Dictionary<Vector3i, byte> blocks;
        public Vector2i chunkPos { get; set; }
        public Dictionary<Vector3i, int> updateTimes = new Dictionary<Vector3i, int>();
        public HashSet<ushort> players;


        public Chunk(Vector2i _chunkPos)
        {
            chunkPos = _chunkPos;
            if (File.Exists($"save/{chunkPos.X}, {chunkPos.Y}.chnk")) // if this chunk has already been saved
                blocks = loadChunk(); // loads from file
            else
                blocks = new Dictionary<Vector3i, byte>(); // else create empty blocks dict
            players = new HashSet<ushort>();
        }

        public void UpdateChunk()
        {

        }

        public void SendBlockStream(ushort _id)
        {
            players.Add(_id);
            Message message = Message.Create(MessageSendMode.Reliable, ServerMessageID.chunkSend);
            byte[] blockStream = new byte[blocks.Count * 4];
            message.AddVector2i(chunkPos);
            message.AddUShort((ushort)(blocks.Count * 4));
            Console.WriteLine($"sending chunk{chunkPos}");
            foreach (var block in blocks)
            {
                message.AddVector3b(block.Key);
                message.AddByte(block.Value);
            }
            NetworkManager.server.Send(message, _id);
        }

        public Dictionary<Vector3i, byte> loadChunk()
        {
            var cereal = File.ReadAllBytes($"save/{chunkPos.X}, {chunkPos.Y}.chnk");
            // gets deserialised block dictionary from file
            // ?? checks if a value is null, if yes: right value is used
            blocks = MemoryPackSerializer.Deserialize<Dictionary<Vector3i, byte>>(cereal) ?? new Dictionary<Vector3i, byte>();

            return blocks;
        }

        public void saveChunk()
        {
            if (blocks.Count < 1) return; // no point saving empty chunks
            var cereal = MemoryPackSerializer.Serialize(blocks); // serialises the blocks to a byte[] array
            // a file is created and used for each chunk. this allows multiple chunks to be saved and loaded concurrently
            using (var fileStream = new FileStream($"save/{chunkPos.X}, {chunkPos.Y}.chnk", FileMode.OpenOrCreate))
            {
                // writes the byte array to a file
                fileStream.Write(cereal);
            }
        }
    }
}