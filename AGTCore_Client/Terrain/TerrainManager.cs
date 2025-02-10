using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Riptide;

namespace AGTCore
{
    public class TerrainManager : Updatable
    {
        public int renderDistance { get; set; }
        Texture texture;
        public static int chunkVAO;
        public ConcurrentDictionary<Vector2i, Chunk> loadedChunks { get; set; }
        public ConcurrentDictionary<Vector2i, Chunk> partialChunks { get; set; }
        Queue<Vector2i> chunksToLoad;
        HashSet<Vector2i> chunksToUnload;
        bool consEnd = false;
        public TerrainManager(int _renderDistance = 2)
        {
            loadedChunks = new ConcurrentDictionary<Vector2i, Chunk>();
            renderDistance = _renderDistance;
            partialChunks = new ConcurrentDictionary<Vector2i, Chunk>();
            System.Console.WriteLine(Constants.blockNames[0]);
            TextureGenerator.StitchTextures(Constants.blockNames);
            chunksToLoad = new Queue<Vector2i>();
            base.beginUpdating();
        }


        public byte getBlock(Vector3i pos)
        {
            // if requested block outside of chunk's y limit, return 0
            if (pos.Y < 0 || pos.Y >= 256) { return 0; }
            // divides by 16 to get the chunk position from the player position
            Vector2i chunkKey = (pos.X >> 4, pos.Z >> 4);
            // tries to get chunk from Chunks list using the key calculate above
            if (loadedChunks.TryGetValue(chunkKey, out var chunk))
            {
                // % 16 converts the player's world position to the position in the chunk's array
                // returns the value stored in the chunk's block array
                return chunk.blocks[pos.X % 16, pos.Y, pos.Z % 16];
            }
            // return zero if the chunk at the requested position is not in the Chunks array
            else { return 0; }
        }
        public void setBlock(Vector3i pos, byte block, int _tick)
        {
            // System.Console.WriteLine($"{pos}, {_tick}");
            // if requested block outside of chunk's y limit, return
            if (pos.Y < 0 || pos.Y >= 256) { return; }
            // divides by 16 to get the chunk position from the player position
            Vector2i chunkKey = (pos.X >> 4, pos.Z >> 4);
            if (loadedChunks.TryGetValue(chunkKey, out var chunk)) // true if chunkKey exists in Chunks
            {
                // this ensures updates are only executed if they are newer than the previous update
                // out of order block updates can lead to desynching
                if (chunk.chunkUpdates.TryGetValue(pos, out int lastTick)) // true if pos is already in chunkUpdates
                {
                    // System.Console.WriteLine($"{lastTick}, {_tick}");
                    if (_tick > lastTick) // checks if this update is more recent than the last
                    {
                        chunk.chunkUpdates[pos] = _tick; // records this update
                        chunk.blocks[pos.X % 16, pos.Y, pos.Z % 16] = block; // executes the update
                        chunk.meshOutdated = true; // tells the chunk a new mesh is needed
                    }
                }
                else
                {
                    chunk.chunkUpdates.Add(pos, _tick); // records this update
                    chunk.blocks[pos.X % 16, pos.Y, pos.Z % 16] = block; // executes the update
                    chunk.meshOutdated = true;
                }
            }
        }

        public void initTerrain()
        {
            texture = Texture.LoadTexFromFile("textureAtlas.png");
            texture.Use(TextureUnit.Texture0);
            chunkVAO = GL.GenVertexArray();
        }

        // loops over chunks and renders them
        public void renderTerrain(int playerX, int playerZ)
        {
            texture.Use(TextureUnit.Texture0);
            GL.BindVertexArray(TerrainManager.chunkVAO);

            foreach (Chunk chunk in loadedChunks.Values)
            {
                chunk.RenderMesh(texture);
            }
        }

        public override void Update() // loads and unloads chunks based on player proximity
        {
            // converts player coords into chunk coords
            int playerX = (int)LocalPlayer.localPos.X >> 4;
            int playerZ = (int)LocalPlayer.localPos.Z >> 4;
            // all chunk positions in the chunk dict are added to the chunksToDelete list
            chunksToUnload = loadedChunks.Keys.ToHashSet();
            // the range of chunk position that should be loaded is looped over
            for (int x = playerX - renderDistance; x < playerX + renderDistance; x++)
            {
                for (int z = playerZ - renderDistance; z < playerZ + renderDistance; z++)
                {
                    // tries to get the chunk at position x, z from the Chunks list
                    if (loadedChunks.TryGetValue((x, z), out var chunk))
                    {
                        // if it exists, the chunk is updated and removed from the chunksToDelete list
                        chunk.UpdateChunk();
                        chunksToUnload.Remove((x, z));
                    }
                    else
                    {
                        // if it doesnt exist in the list, it is added to the chunkToLoad list
                        chunksToLoad.Enqueue((x, z));
                    }
                }
            }
            // requests the chunks that are to be loaded
            while (chunksToLoad.TryDequeue(out var chunkKey))
            {
                if (partialChunks.TryAdd(chunkKey, new Chunk(chunkKey)))
                {
                    // System.Console.WriteLine($"requesting {chunkKey}...");
                    Message message = Message.Create(MessageSendMode.Reliable, ClientMessageID.chunkLoad);
                    message.AddVector2i(chunkKey);
                    NetworkManager.client.Send(message);
                }
            }
            // any chunks still in the chunksToDelete list were not in the range of values looped over above
            // they can therefore be removed, as they are outside the player's range
            foreach (var chunkKey in chunksToUnload)
            {
                // System.Console.WriteLine($"unloading {chunkKey}...");
                // if remove was successful, unload chunk
                if (loadedChunks.TryRemove(chunkKey, out var chunk))
                {
                    Message message = Message.Create(MessageSendMode.Reliable, ClientMessageID.chunkUnload);
                    message.AddVector2i(chunkKey);
                    NetworkManager.client.Send(message);
                    chunk.Unload();
                }
                // if unsuccessful, the chunk can be unloaded next gametick
            }

        }

        public void unloadTerrain()
        {
            foreach (var chunk in loadedChunks.Values)
            {
                chunk.Unload();
            }
        }

        void SendChunkRequest(Vector2i chunkKey)
        {
            Message message = Message.Create(MessageSendMode.Reliable, ClientMessageID.chunkLoad);
            message.AddVector2i(chunkKey);
            NetworkManager.client.Send(message);
        }

        [MessageHandler((ushort)ServerMessageID.chunkSend)]
        private static void BlockStreamRecieved(Message message) // recieves multiple blocks, packed into in a byte array
        {   // message order = vec2i chunkPos, ushort streamLength, byte[] stream
            var chunkKey = message.GetVector2i();
            ushort streamLength = message.GetUShort();
            // System.Console.WriteLine($"recieving {chunkKey}... len({streamLength})");
            Game.terrainManager.partialChunks[chunkKey].RecieveBlockStream(message.GetBytes(streamLength));
            // if chunk is completed, move from partial chunks to loaded chunks
            if (streamLength < 801) // max length is 800
            {
                // System.Console.WriteLine($"recieved {chunkKey}");
                if (Game.terrainManager.partialChunks.TryRemove(chunkKey, out var chunk))
                    Game.terrainManager.loadedChunks.TryAdd(chunkKey, chunk);
            }
        }
    }
}