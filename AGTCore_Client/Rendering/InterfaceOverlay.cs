
using OpenTK.Mathematics;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using System;

namespace AGTCore
{
    public class Overlay
    {
        int OverlayVBO;
        int selectionPlaneVAO;
        Shader shader;
        Texture texture;
        public Matrix4 model { get; set; }

        static float[] cube = {
            0, 0, 0,  0.0f, 1,
            1, 0, 0,  1.0f, 1,
            1, 1, 0,  1.0f, 0,
            1, 1, 0,  1.0f, 0,
            0, 1, 0,  0.0f, 0,
            0, 0, 0,  0.0f, 1,// side
            0, 0, 1,  0.0f, 1,
            1, 0, 1,  1.0f, 1,
            1, 1, 1,  1.0f, 0,
            0, 1, 1,  0.0f, 0,
            1, 1, 1,  1.0f, 0,
            0, 0, 1,  0.0f, 1,//side
            0, 1, 1,  0.0f, 0,
            0, 1, 0,  1.0f, 0,
            0, 0, 0,  1.0f, 1,
            0, 0, 0,  1.0f, 1,
            0, 0, 1,  0.0f, 1,
            0, 1, 1,  0.0f, 0,//front
            1, 1, 1,  0.0f, 0,
            1, 1, 0,  1.0f, 0,
            1, 0, 0,  1.0f, 1,
            1, 0, 0,  1.0f, 1,
            1, 0, 1,  0.0f, 1,
            1, 1, 1,  0.0f, 0,//back
            0, 0, 0,  0.0f, 1.0f,
            1, 0, 0,  1.0f, 1.0f,
            1, 0, 1,  1.0f, 0,
            1, 0, 1,  1.0f, 0,
            0, 0, 1,  0.0f, 0,
            0, 0, 0,  0.0f, 1.0f,//bottom
            0, 1, 0,  0.0f, 1,
            1, 1, 0,  1.0f, 1,
            1, 1, 1,  1.0f, 0.0f,
            1, 1, 1,  1.0f, 0.0f,
            0, 1, 1,  0.0f, 0.0f,
            0, 1, 0,  0.0f, 1//top
        };
        Dictionary<Vector3i, int> vecToRot = new Dictionary<Vector3i, int>{
            {Vector3i.UnitX, 18},
            {-Vector3i.UnitX, 12},
            {Vector3i.UnitY, 30},
            {-Vector3i.UnitY, 24},
            {Vector3i.UnitZ, 6},
            {-Vector3i.UnitZ, 0},
            {-Vector3i.Zero, 6}
        };
        public Overlay(Shader _shader)
        {
            shader = _shader;
        }

        public void InitMesh()
        {

            texture = Texture.LoadTexFromFile("textures/selection.png");
            texture.Use(TextureUnit.Texture0);

            OverlayVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, OverlayVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, cube.Length * sizeof(float), cube, BufferUsageHint.StaticDraw);

            selectionPlaneVAO = GL.GenVertexArray();
            GL.BindVertexArray(selectionPlaneVAO);

            var vertexLocation = shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            var texCoordLocation = shader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        }

        public void RenderMesh()
        {
            GL.Disable(EnableCap.DepthTest);
            if (LocalPlayer.blockSelected)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, OverlayVBO);
                GL.BindVertexArray(selectionPlaneVAO);

                texture.Use(TextureUnit.Texture0);
                shader.Use();

                model = Matrix4.Identity * Matrix4.CreateTranslation(LocalPlayer.selectedBlockPos);
                shader.SetMatrix4("model", model);
                GL.DrawArrays(PrimitiveType.Triangles, vecToRot[LocalPlayer.selectedFace], 6);
            }
            GL.Enable(EnableCap.DepthTest);
        }
        public void Unload()
        {
            GL.DeleteBuffer(OverlayVBO);
        }
    }
}