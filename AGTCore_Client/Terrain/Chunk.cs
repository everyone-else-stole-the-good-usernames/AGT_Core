using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System;
namespace AGTCore
{
    public class Chunk
    {
        public byte[,,] blocks;
        float[] vertices;
        public Vector2i chunkPos;
        int vertexCount;
        int chunkVBO;
        int chunkVAO;
        int vertexLocation;
        int texCoordLocation;
        public Dictionary<Vector3i, int> chunkUpdates = new Dictionary<Vector3i, int>();
        public bool meshOutdated = true;
        public bool unInitialised = true;
        bool buffersNotCreated = true;


        public Chunk(Vector2i _chunkPos)
        {
            chunkPos = _chunkPos;
            GenerateChunk();
        }


        public void GenerateChunk()
        {
            blocks = ChunkGenerator.GenerateChunk(chunkPos.X, chunkPos.Y);
            vertices = GenerateMesh();
            vertexCount = vertices.Length;
        }
        public void RecieveBlockStream(byte[] blockStream)
        {
            // Console.WriteLine($"recieving {chunkPos}...");
            for (int i = 0; i < blockStream.Length; i += 4)
            {
                // Console.WriteLine($"{i}, {i + 1}, {i + 2}");
                blocks[blockStream[i], blockStream[i + 1], blockStream[i + 2]] = blockStream[i + 3];

            }
        }

        public void UpdateChunk()
        {
            if (meshOutdated)
            {
                vertices = GenerateMesh();
                vertexCount = vertices.Length;
                // mesh can only be reinitialised in the rendering thread, not the update thread
                meshOutdated = false;
                unInitialised = true;
            }
        }

        // sends vertex data to the GPU
        public void InitMesh()
        {
            // buffers need to be created on the first mesh init only
            if (buffersNotCreated)
            {
                // tells OpenGl to create VBO and VAO objects and return their ids
                // OpenGL is written in C, a non-OOP language.
                // OpenGL uses a state machine system, where objects can be referenced with ids
                chunkVBO = GL.GenBuffer();
                chunkVAO = GL.GenVertexArray();
                // gets references to the vertex and texture location attributes
                vertexLocation = Game.shader.GetAttribLocation("aPosition");
                texCoordLocation = Game.shader.GetAttribLocation("aTexCoord");
                buffersNotCreated = false;
            }
            // GL.Bind...(int id) tells the OpenGL statemachine to load an object using an id
            GL.BindVertexArray(chunkVAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, chunkVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexCount * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // attribute arrays tell OpenGL what shape the data in the buffers is
            // Vec3 positions and Vec2 tex coords are sent: equivolent of 5 floats
            // we need to tell the OpenGl that every 5 floats in the buffer contain the vertex and texture vectors
            GL.EnableVertexAttribArray(vertexLocation); // enables and sends vertexLocation attribute array
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            GL.EnableVertexAttribArray(texCoordLocation); // enables and sends texCoordLocation attribute array
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            unInitialised = false; // tells the RenderMesh() method that this mesh is read to render
        }

        public void RenderMesh(Texture texture)
        {
            if (unInitialised) InitMesh();
            GL.BindVertexArray(chunkVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, chunkVBO);

            var transform1 = Matrix4.Identity;

            texture.Use(TextureUnit.Texture0);
            Game.shader.Use();

            var model = Matrix4.Identity;
            Game.shader.SetMatrix4("model", model);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);
        }
        public void Unload()
        { }

        static float[][] cubePlanes = {
            new float[]{
            0, 0, 0,  0.0f, 0.666f,
            1, 0, 0,  1.0f, 0.666f,
            1, 1, 0,  1.0f, 0.333f,
            1, 1, 0,  1.0f, 0.333f,
            0, 1, 0,  0.0f, 0.333f,
            0, 0, 0,  0.0f, 0.666f},// side
            new float[]{
            0, 0, 1,  0.0f, 0.666f,
            1, 0, 1,  1.0f, 0.666f,
            1, 1, 1,  1.0f, 0.333f,
            0, 1, 1,  0.0f, 0.333f,
            1, 1, 1,  1.0f, 0.333f,
            0, 0, 1,  0.0f, 0.666f},//side
            new float[]{
            0, 1, 1,  0.0f, 0.333f,
            0, 1, 0,  1.0f, 0.333f,
            0, 0, 0,  1.0f, 0.666f,
            0, 0, 0,  1.0f, 0.666f,
            0, 0, 1,  0.0f, 0.666f,
            0, 1, 1,  0.0f, 0.333f},//front
            new float[]{
            1, 1, 1,  0.0f, 0.333f,
            1, 1, 0,  1.0f, 0.333f,
            1, 0, 0,  1.0f, 0.666f,
            1, 0, 0,  1.0f, 0.666f,
            1, 0, 1,  0.0f, 0.666f,
            1, 1, 1,  0.0f, 0.333f},//back
            new float[]{
            0, 0, 0,  0.0f, 1.0f,
            1, 0, 0,  1.0f, 1.0f,
            1, 0, 1,  1.0f, 0.666f,
            1, 0, 1,  1.0f, 0.666f,
            0, 0, 1,  0.0f, 0.666f,
            0, 0, 0,  0.0f, 1.0f},//bottom
            new float[]{
            0, 1, 0,  0.0f, 0.333f,
            1, 1, 0,  1.0f, 0.333f,
            1, 1, 1,  1.0f, 0.0f,
            1, 1, 1,  1.0f, 0.0f,
            0, 1, 1,  0.0f, 0.0f,
            0, 1, 0,  0.0f, 0.333f}//top
        };


        #region mesh generation
        public static int chunkVertices = 6 * 4096;

        // this function creates a mesh 
        public List<(Vector4i, int)> skinChunk()
        {
            // this list contains plane type (left, right, etc) and block type for each visible plane
            List<(Vector4i, int)> visiblePlanes = new List<(Vector4i, int)>();
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 128; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        int block = blocks[x, y, z];
                        // if block is not air, check its adjacent blocks
                        if (blocks[x, y, z] != 0)
                        {
                            // checks if air block is left, right, up etc relative to the current block
                            if ((x == 15) || blocks[x + 1, y, z] == 0)
                            {
                                visiblePlanes.Add(((x, y, z, 3), block));
                            }
                            if ((y == 128) || blocks[x, y + 1, z] == 0)
                            {
                                visiblePlanes.Add(((x, y, z, 5), block));
                            }
                            if ((z == 15) || blocks[x, y, z + 1] == 0)
                            {
                                visiblePlanes.Add(((x, y, z, 1), block));
                            }
                            if ((x == 0) || blocks[x - 1, y, z] == 0)
                            {
                                visiblePlanes.Add(((x, y, z, 2), block));
                            }
                            if ((y == 0) || blocks[x, y - 1, z] == 0)
                            {
                                visiblePlanes.Add(((x, y, z, 4), block));
                            }
                            if ((z == 0) || blocks[x, y, z - 1] == 0)
                            {
                                visiblePlanes.Add(((x, y, z, 0), block));
                            }
                        }

                    }
                }
            }
            chunkVertices = 6 * visiblePlanes.Count; // this value is needed by OpenGL to size vertex buffers
            return visiblePlanes;
        }

        public float[] GenerateMesh()
        {
            // gets list of visible planes (x,y,z,plane direction), block type
            List<(Vector4i, int)> visiblePlanes = skinChunk();
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
                    tempPlane[vert * 5] += visiblePlanes[i].Item1.X + chunkPos.X * 16;
                    tempPlane[vert * 5 + 1] += visiblePlanes[i].Item1.Y;
                    tempPlane[vert * 5 + 2] += visiblePlanes[i].Item1.Z + chunkPos.Y * 16;
                    // sets the x texture coordinates based on the block type
                    // textures for all blocks are merged into a single image
                    // each texture is shifted in the x axis
                    tempPlane[vert * 5 + 3] += (visiblePlanes[i].Item2 - 1);
                    tempPlane[vert * 5 + 3] /= TextureGenerator.blockCount;
                }
                Array.Copy(tempPlane, 0, mesh, counter * 30, 30);
                counter += 1;
            }
            return mesh;
        }
    }
    #endregion
}