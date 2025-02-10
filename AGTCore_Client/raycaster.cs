using System;
using OpenTK.Mathematics;

namespace AGTCore
{
    public static class Raycaster
    {
        public static (Vector3i, Vector3i) raycast(Vector3 origin, Vector3 direction, float radius)
        {
            // Cube containing origin point.
            var x = MathF.Floor(origin.X);
            var y = MathF.Floor(origin.Y);
            var z = MathF.Floor(origin.Z);
            // Break out direction vector.
            var dx = direction.X;
            var dy = direction.Y;
            var dz = direction.Z;
            // Direction to increment x,y,z when stepping.
            var stepX = signum(dx);
            var stepY = signum(dy);
            var stepZ = signum(dz);
            // See description above. The initial values depend on the fractional
            // part of the origin.
            var tMaxX = intbound(origin.X, dx);
            var tMaxY = intbound(origin.Y, dy);
            var tMaxZ = intbound(origin.Z, dz);
            // The change in t when taking a step (always positive).
            var tDeltaX = stepX / dx;
            var tDeltaY = stepY / dy;
            var tDeltaZ = stepZ / dz;
            // Buffer for reporting faces to the callback.
            Vector3i face = new Vector3i(0, 0, 0);

            // Avoids an infinite loop.
            if (dx == 0 && dy == 0 && dz == 0)
                throw new ArgumentOutOfRangeException("Ray cant be cast in zero direction");

            // Rescale from units of 1 cube-edge to units of 'direction' so we can
            // compare with 't'.
            radius /= MathF.Sqrt(dx * dx + dy * dy + dz * dz);

            while (/* ray has not gone past bounds of world */
                    (stepX > 0 ? true : x >= 0) &&
                    (stepY > 0 ? y < 256 : y >= 0) &&
                    (stepZ > 0 ? true : z >= 0))
            {


                // Invoke the callback, unless we are not *yet* within the bounds of the
                // world.
                // if (!(x < 0 || y < 0 || z < 0 || x >= wx || y >= wy || z >= wz))
                //     if (callback(x, y, z, blocks[x * wy * wz + y * wz + z], face))
                //         break;
                if (Game.terrainManager.getBlock(((int)x, (int)y, (int)z)) != 0)
                {
                    break;
                }


                // tMaxX stores the t-value at which we cross a cube boundary along the
                // X axis, and similarly for Y and Z. Therefore, choosing the least tMax
                // chooses the closest cube boundary. Only the first case of the four
                // has been commented in detail.
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        if (tMaxX > radius) break;
                        // Update which cube we are now in.
                        x += stepX;
                        // Adjust tMaxX to the next X-oriented boundary crossing.
                        tMaxX += tDeltaX;
                        // Record the normal vector of the cube face we entered.
                        face.X = -stepX;
                        face.Y = 0;
                        face.Z = 0;
                    }
                    else
                    {
                        if (tMaxZ > radius) break;
                        z += stepZ;
                        tMaxZ += tDeltaZ;
                        face.X = 0;
                        face.Y = 0;
                        face.Z = -stepZ;
                    }
                }
                else
                {
                    if (tMaxY < tMaxZ)
                    {
                        if (tMaxY > radius) break;
                        y += stepY;
                        tMaxY += tDeltaY;
                        face.X = 0;
                        face.Y = -stepY;
                        face.Z = 0;
                    }
                    else
                    {
                        // Identical to the second case, repeated for simplicity in
                        // the conditionals.
                        if (tMaxZ > radius) break;
                        z += stepZ;
                        tMaxZ += tDeltaZ;
                        face.X = 0;
                        face.Y = 0;
                        face.Z = -stepZ;
                    }
                }
            }
            return ((Vector3i)new Vector3(x, y, z), face);
        }

        static float intbound(float s, float ds)
        {
            // Find the smallest positive t such that s+t*ds is an integer.
            if (ds < 0)
            {
                return intbound(-s, -ds);
            }
            else
            {
                s = mod(s, 1);
                // problem is now s+t*ds = 1
                return (1 - s) / ds;
            }
        }

        static int signum(float x)
        {
            return x > 0 ? 1 : x < 0 ? -1 : 0;
        }

        static float mod(float value, float modulus)
        {
            return (value % modulus + modulus) % modulus;
        }
    }
}