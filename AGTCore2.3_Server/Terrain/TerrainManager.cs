using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Riptide;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace AGTCoreServer
{
    public class TerrainManager : Updatable
    {
        public int renderDistance;
        public ConcurrentDictionary<Vector2i, Chunk> loadedChunks; // chunks the server has loaded
        public ConcurrentDictionary<Vector2i, HashSet<ushort>> playerChunks; // chunks the players want loaded
        public HashSet<Vector2i> chunksToUnload;
        HashSet<Vector2i> chunksToLoad;
        public TerrainManager(int _renderDistance = 2)
        {
            chunksToLoad = new HashSet<Vector2i>();
            chunksToUnload = new HashSet<Vector2i>();
            loadedChunks = new ConcurrentDictionary<Vector2i, Chunk>();
            playerChunks = new ConcurrentDictionary<Vector2i, HashSet<ushort>>();
            renderDistance = _renderDistance;
            renderDistance = _renderDistance;
        }

        public byte getBlock(int x, int y, int z)
        {
            // if requested block outside of chunk's y limit, return 0
            if (y < 0 || y >= 256) { return 0; }
            // divides by 16 to get the chunk position from the player position
            Vector2i chunkKey = (x >> 4, z >> 4);
            // tries to get chunk from Chunks list using the key calculate above
            if (loadedChunks.TryGetValue(chunkKey, out var chunk))
            {
                // % 16 converts the player's world position to the position in the chunk's array
                // returns the value stored in the chunk's block array
                return chunk.blocks[((x % 16), y, (z % 16))];
            }
            // return zero if the chunk at the requested position is not in the Chunks array
            else { return 0; }
        }
        public void setBlock(Vector3i pos, byte block, int tick)
        {
            System.Console.WriteLine($"{pos}");
            // if requested block outside of chunk's y limit, return
            if (pos.Y < 0 || pos.Y >= 256) { return; }
            // divides by 16 to get the chunk position from the player position
            Vector2i chunkKey = (pos.X >> 4, pos.Z >> 4);
            if (loadedChunks.TryGetValue(chunkKey, out var chunk)) // true if chunkKey exists in Chunks
            {
                // this ensures updates are only executed if they are newer than the previous update
                // out of order block updates can lead to desynching
                if (chunk.updateTimes.TryGetValue(pos, out int _tick)) // true if pos is already in chunkUpdates
                {
                    System.Console.WriteLine($"{tick}, _{_tick}");
                    if (tick > _tick) // checks if this update is more recent than the last
                    {

                        chunk.updateTimes[pos] = tick; // records this update
                        chunk.blocks[((pos.X % 16), pos.Y, (pos.Z % 16))] = block; // executes the update
                    }
                    else return; // if this update is older, return
                }
                else
                {
                    chunk.updateTimes.Add(pos, tick); // records this update
                    chunk.blocks[((pos.X % 16), pos.Y, (pos.Z % 16))] = block; // executes the update
                }
                // creates a message with the block pos, the block type and the update tick
                Message message = Message.Create(MessageSendMode.Reliable, ServerMessageID.block);
                message.AddVector3i(pos);
                message.Add(block);
                message.Add(tick);
                NetworkManager.server.SendToAll(message);
            }
        }

        void SendBlock(Vector3i pos, byte block, int tick)
        {
            Message message = Message.Create(MessageSendMode.Reliable, ServerMessageID.block);
            message.AddVector3i(pos);
            message.Add(block);
            message.Add(tick);
            NetworkManager.server.SendToAll(message);
        }

        public override void Update() // loads and unloads chunks based on player proximity
        {
            chunksToUnload.Clear();
            foreach (var chunk in loadedChunks.Values)
            {
                if (chunk.players.Count == 0) // if chunk isnt used by any players
                {
                    Console.WriteLine($"no players in chunk {chunk.chunkPos}");
                    chunksToUnload.Add(chunk.chunkPos); // mark chunk for unloading
                }
                //TODO: chunk update
            }
            foreach (var chunkKey in chunksToUnload)
            {
                // if remove was successful, unload and save chunk
                if (loadedChunks.TryRemove(chunkKey, out var chunk))
                {
                    chunk.saveChunk();
                }
            }
        }

        public override void Destroy()
        {
            Parallel.ForEach(loadedChunks.Values, chunk => chunk.saveChunk());
        }

        [MessageHandler((ushort)ClientMessageID.block)]
        private static void BlockUpdateRecieved(ushort _clientId, Message message) // sets block
        {
            Program.terrainManager.setBlock(message.GetVector3i(), message.GetByte(), message.GetInt());
        }
    }
}