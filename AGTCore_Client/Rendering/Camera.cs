using OpenTK.Mathematics;
using System;

namespace AGTCore
{
    public class Camera
    {
        public Vector3 front = -Vector3.UnitZ;
        private Vector3 right = Vector3.UnitX;
        private Vector3 up = Vector3.UnitY;
        private Vector3 worldFront = -Vector3.UnitZ;
        private Vector3 worldRight = Vector3.UnitX;
        // private Vector3 _worldUp = Vector3.UnitY;

        // values are in radians
        public float yaw = -MathHelper.PiOver2; // starts camera facing forward
        private float fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            LocalPlayer.localPos = position;
            AspectRatio = aspectRatio;
        }
        public Vector3 Position { get; set; } // automatically generated getter and setter methods
        public float AspectRatio { private get; set; }

        public Vector3 Front => front;
        public Vector3 Up => up;
        public Vector3 Right => right;
        public Vector3 WorldFront => worldFront;
        public Vector3 WorldRight => worldRight;
        public Vector3 WorldUp = Vector3.UnitY;
        public float pitch;

        // the getter and setter methods have been overriden for the Pitch field
        // reading the value of Pitch returns the private field "pitch" in radians
        // setting the value in degrees modifies "pitch" in radians and updates the camera's vectors
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(pitch);
            set
            {
                // clamping value between +- 89 prevents gimbal lock
                var angle = MathHelper.Clamp(value, -89f, 89f);
                pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(yaw);
            set
            {
                yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(LocalPlayer.localPos, LocalPlayer.localPos + front, up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(fov, AspectRatio, 0.01f, 100f);
        }

        private void UpdateVectors()
        {
            front.X = MathF.Cos(pitch) * MathF.Cos(yaw);
            front.Y = MathF.Sin(pitch);
            front.Z = MathF.Cos(pitch) * MathF.Sin(yaw);

            worldFront.X = front.X;
            worldFront.Z = front.Z;

            front = Vector3.Normalize(front);
            worldFront = Vector3.Normalize(worldFront);

            right = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
            worldRight = Vector3.Normalize(Vector3.Cross(front, Vector3.UnitY));
        }

    }
}
