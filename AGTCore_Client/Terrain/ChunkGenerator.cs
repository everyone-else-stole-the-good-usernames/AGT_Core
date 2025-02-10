using System;
using System.Threading.Tasks;
using System.Collections;
namespace AGTCore
{
    public class ChunkGenerator
    {

        public const int chunkSize = 16;

        public static byte[,,] GenerateChunk(int chunkX, int chunkY)
        {
            int height;
            float[,] heightMap = Noise2d.GenerateNoiseMap(16, 16, chunkX * 16, chunkY * 16, 3, out float min, out float range);
            byte[,,] chunk = new byte[16, 256, 16];
            Array.Clear(chunk);
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    height = (int)(16 * heightMap[x, z] + 10); //normalised perlin noise
                    // height = 2;
                    for (int y = 0; y < height; y++)
                    {
                        chunk[x, y, z] = 1;
                    }
                    if (height > 0)
                        chunk[x, height, z] = 2;
                }
            }
            return chunk;
        }


    }
}