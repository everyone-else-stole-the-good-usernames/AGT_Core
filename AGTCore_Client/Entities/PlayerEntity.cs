using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using System;


namespace AGTCore
{
    public class PlayerEntity : Entity
    {
        public PlayerEntity(Vector3 _pos)
        {
            // Console.WriteLine($"entitiy created at {_pos.X}, {_pos.Y}, {_pos.Z}");
            modelPath = "Entities/player.obj";
            modelTransform = Matrix4.Identity * Matrix4.CreateTranslation(_pos);
            EntityManager.entitiesToInit.Enqueue(this);
        }
    }
}