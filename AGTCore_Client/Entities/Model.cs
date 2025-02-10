using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using System.Collections;


namespace AGTCore
{
    public abstract class Entity : Updatable
    {
        protected string modelPath;
        List<int> EntityVAO = new List<int>();
        List<int> EntityVBO = new List<int>();
        List<int> EntityEBO = new List<int>();
        Shader shader;
        Texture texture;
        List<float[]> vertices;
        List<uint[]> indices;
        Vector3 pos;

        public Matrix4 modelTransform { get; set; }

        public Matrix4 headTransform { get; set; }

        public void InitMesh()
        {
            shader = Game.shader;
            ModelLoader.LoadModel(modelPath, out vertices, out indices);
            texture = Texture.LoadTexFromFile("textures/player.png");

            for (int i = 0; i < vertices.Count; i++)
            {
                int VAO = GL.GenVertexArray();
                GL.BindVertexArray(VAO);
                EntityVAO.Add(VAO);

                int VBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices[i].Length * sizeof(float), vertices[i], BufferUsageHint.StaticDraw);
                EntityVBO.Add(VBO);

                int EBO = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices[i].Length * sizeof(int), indices[i], BufferUsageHint.StaticDraw);
                EntityEBO.Add(EBO);

                var vertexLocation = shader.GetAttribLocation("aPosition");
                GL.EnableVertexAttribArray(vertexLocation);
                GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);


                var texCoordLocation = shader.GetAttribLocation("aTexCoord");
                GL.EnableVertexAttribArray(texCoordLocation);
                GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            }
        }

        public void RenderMesh()
        {
            GL.BindVertexArray(EntityVAO[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, EntityVBO[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EntityEBO[0]);

            texture.Use(TextureUnit.Texture0);
            shader.Use();

            shader.SetMatrix4("model", headTransform);
            GL.DrawElements(PrimitiveType.Triangles, indices[0].Length, DrawElementsType.UnsignedInt, 0);
            for (int i = 1; i < vertices.Count; i++)
            {
                GL.BindVertexArray(EntityVAO[i]);
                GL.BindBuffer(BufferTarget.ArrayBuffer, EntityVBO[i]);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, EntityEBO[i]);

                texture.Use(TextureUnit.Texture0);
                shader.Use();

                shader.SetMatrix4("model", modelTransform);
                GL.DrawElements(PrimitiveType.Triangles, indices[i].Length, DrawElementsType.UnsignedInt, 0);
            }
        }
    }

}