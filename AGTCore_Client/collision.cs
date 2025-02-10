using OpenTK.Mathematics;
using System;
namespace AGTCore
{
    class Test
    {

        Vector3 position = new Vector3(10, 10, 10);
        Vector3 velocity = new Vector3(10, 10, 10);
        Vector3i boundingBox = new Vector3i(2, 2, 2);
        bool onGround = false;
        void collide(Vector3 vel, float dt)
        {
            for (int x = (int)(position.X - boundingBox.X);
                 x < position.X + boundingBox.X; x++)
                for (int y = (int)(position.Y - boundingBox.Y); y < position.Y + 0.7; y++)
                    for (int z = (int)(position.Z - boundingBox.Z);
                         z < position.Z + boundingBox.Z; z++)
                    {

                        if (((Game.terrainManager.getBlock((x, y, z))) != 0))
                        {
                            if (vel.Y > 0)
                            {
                                position.Y = y - boundingBox.Y;
                                velocity.Y = 0;
                            }
                            else if (vel.Y < 0)
                            {
                                onGround = true;
                                position.Y = y + boundingBox.Y + 1;
                                velocity.Y = 0;
                            }

                            if (vel.X > 0)
                            {
                                position.X = x - boundingBox.X;
                            }
                            else if (vel.X < 0)
                            {
                                position.X = x + boundingBox.X + 1;
                            }

                            if (vel.Z > 0)
                            {
                                position.Z = z - boundingBox.Z;
                            }
                            else if (vel.Z < 0)
                            {
                                position.Z = z + boundingBox.Z + 1;
                            }
                        }
                    }
        }
    }
}