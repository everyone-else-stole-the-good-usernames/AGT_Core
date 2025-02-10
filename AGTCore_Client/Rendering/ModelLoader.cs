using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using System.Collections;
using Assimp;

namespace AGTCore
{
    public static class ModelLoader
    {
        // this method loads an obj model at a specified path 
        public static void LoadModel(string path, out List<float[]> vertices, out List<uint[]> indices)
        {
            AssimpContext importer = new AssimpContext();
            Scene scene = importer.ImportFile(path, PostProcessSteps.Triangulate);

            vertices = new List<float[]>();
            indices = new List<uint[]>();
            for (int i = 0; i < scene.MeshCount; i++)
            {
                Mesh mesh = scene.Meshes[i];
                indices.Add(mesh.GetUnsignedIndices());

                List<Vector3D> vertices3d = mesh.Vertices;
                List<Vector3D> texCoords3d = mesh.TextureCoordinateChannels[0];

                float[] subVertices = new float[mesh.VertexCount * 5];
                for (int j = 0; j < mesh.VertexCount; j++)
                {
                    subVertices[j * 5] = (vertices3d[j].X);
                    subVertices[j * 5 + 1] = (vertices3d[j].Y);
                    subVertices[j * 5 + 2] = (vertices3d[j].Z);
                    subVertices[j * 5 + 3] = texCoords3d[j].X;
                    subVertices[j * 5 + 4] = texCoords3d[j].Y;
                }
                vertices.Add(subVertices);
            }
        }

        static ModelLoader()
        {
        }
    }
}