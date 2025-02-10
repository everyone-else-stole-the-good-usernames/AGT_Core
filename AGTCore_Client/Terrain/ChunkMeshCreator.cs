using System.Collections.Generic;
using OpenTK.Mathematics;
using System;

namespace AGTCore
{
    public static class ChunkMeshGenerator
    {
        static float[][] cubePlanes2;
        static Dictionary<int, float[]> cubePlanes = new Dictionary<int, float[]>(){
            {0, new float[]{
            -0.5f, -0.5f, -0.5f,  0.0f, 0.666f,
             0.5f, -0.5f, -0.5f,  1.0f, 0.666f,
             0.5f,  0.5f,-0.5f,  1.0f, 0.333f,
             0.5f,  0.5f, -0.5f,  1.0f, 0.333f,
            -0.5f,  0.5f, -0.5f,  0.0f, 0.333f,
            -0.5f, -0.5f, -0.5f,  0.0f, 0.666f}},// side
            {1, new float[]{
            -0.5f, -0.5f,  0.5f,  0.0f, 0.666f,
             0.5f, -0.5f,  0.5f,  1.0f, 0.666f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.333f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.333f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.333f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.666f}},//side
            {2, new float[]{
            -0.5f,  0.5f,  0.5f,  0.0f, 0.333f,
            -0.5f,  0.5f, -0.5f,  1.0f, 0.333f,
            -0.5f, -0.5f, -0.5f,  1.0f, 0.666f,
            -0.5f, -0.5f, -0.5f,  1.0f, 0.666f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.666f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.333f}},//front
            {3, new float[]{
             0.5f,  0.5f,  0.5f,  0.0f, 0.333f,
             0.5f,  0.5f, -0.5f,  1.0f, 0.333f,
             0.5f, -0.5f, -0.5f,  1.0f, 0.666f,
             0.5f, -0.5f, -0.5f,  1.0f, 0.666f,
             0.5f, -0.5f,  0.5f,  0.0f, 0.666f,
             0.5f,  0.5f,  0.5f,  0.0f, 0.333f}},//back
            {4, new float[]{
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
             0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
             0.5f, -0.5f,  0.5f,  1.0f, 0.666f,
             0.5f, -0.5f,  0.5f,  1.0f, 0.666f,
            -0.5f, -0.5f,  0.5f,  0.0f, 0.666f,
            -0.5f, -0.5f, -0.5f,  0.0f, 1.0f}},//bottom
            {5, new float[]{
            -0.5f,  0.5f, -0.5f,  0.0f, 0.333f,
             0.5f,  0.5f, -0.5f,  1.0f, 0.333f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
             0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
            -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
            -0.5f,  0.5f, -0.5f,  0.0f, 0.333f}}//top
        };

        // static int planes = 16 * 16 * 16;
        // public static int chunkVertices = 6 * planes;
        public static int chunkVertices = 6 * 4096;

        public static List<(Vector4i, int)> cleanChunk(ref byte[,,] chunkArray)
        {
            List<(Vector4i, int)> visiblePlanes = new List<(Vector4i, int)>();
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 128; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        int block = chunkArray[x, y, z];
                        if (chunkArray[x, y, z] != 0)
                        {
                            if ((x == 15) || chunkArray[x + 1, y, z] == 0)
                            {
                                visiblePlanes.Add((new Vector4i(x, y, z, 3), block));
                            }
                            if ((y == 128) || chunkArray[x, y + 1, z] == 0)
                            {
                                visiblePlanes.Add((new Vector4i(x, y, z, 5), block));
                            }
                            if ((z == 15) || chunkArray[x, y, z + 1] == 0)
                            {
                                visiblePlanes.Add((new Vector4i(x, y, z, 1), block));
                            }
                            if ((x == 0) || chunkArray[x - 1, y, z] == 0)
                            {
                                visiblePlanes.Add((new Vector4i(x, y, z, 2), block));
                            }
                            if ((y == 0) || chunkArray[x, y - 1, z] == 0)
                            {
                                visiblePlanes.Add((new Vector4i(x, y, z, 4), block));
                            }
                            if ((z == 0) || chunkArray[x, y, z - 1] == 0)
                            {
                                visiblePlanes.Add((new Vector4i(x, y, z, 0), block));
                            }
                        }

                    }
                }
            }
            chunkVertices = 6 * visiblePlanes.Count;
            return visiblePlanes;
        }

        // this generates a mesh for the chunk
        public static float[] GenerateMesh(byte[,,] chunkArray, Vector2i ChunkPos)
        {

            List<(Vector4i, int)> visiblePlanes = cleanChunk(ref chunkArray);
            float[] mesh = new float[visiblePlanes.Count * 30];
            float[] tempPlane = new float[30];
            int counter = 0;
            // foreach (var visiblePlane in visiblePlanes)
            for (int i = 0; i < visiblePlanes.Count; i++)
            {
                cubePlanes[visiblePlanes[i].Item1.W].CopyTo(tempPlane, 0);
                for (int vert = 0; vert < 6; vert++)
                {
                    // moves the vertices to the correct position
                    tempPlane[vert * 5] += visiblePlanes[i].Item1.X + ChunkPos.X * 16;
                    tempPlane[vert * 5 + 1] += visiblePlanes[i].Item1.Y;
                    tempPlane[vert * 5 + 2] += visiblePlanes[i].Item1.Z + ChunkPos.Y * 16;
                    // sets the x texture coordinates
                    tempPlane[vert * 5 + 3] += (visiblePlanes[i].Item2 - 1);
                    tempPlane[vert * 5 + 3] /= TextureGenerator.blockCount;
                }
                Array.Copy(tempPlane, 0, mesh, counter * 30, 30);
                counter += 1;
            }
            return mesh;
        }
    }
}