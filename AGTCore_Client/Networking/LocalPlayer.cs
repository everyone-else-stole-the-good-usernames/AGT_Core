using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using Riptide;

namespace AGTCore
{
    // the local player class inherits the networking functionality of the Player class
    // it also contains static methods that handle movement of the local camera
    public class LocalPlayer : Player
    {
        public LocalPlayer(ushort _id, string _username, Vector3 _spawnPos) : base(_id, _username, _spawnPos)
        {
            localPos = _spawnPos;
            Game.terrainManager = new TerrainManager(4);
            beginUpdating();
            Program.menu.Close();
            Program.StartGameWindow = true;
        }
        float bodyRot = 0f;

        #region networking
        // this section handles sending position data to the server from the local player
        public override void Update() // called every game tick
        {
            // sends position and rotation of the camera to the server
            if (Game.camera != null)
                SendPos();
        }

        void SendPos()
        {
            // this sends the head and body rotation
            // the head body is rotated if the head turns past a certain threshold for asthetic reasons
            if (walking || MathF.Abs(bodyRot - Game.camera.yaw) > 0.61f) bodyRot = Game.camera.yaw;
            // creates a message containing the rotation and position of the player
            Message message = Message.Create(MessageSendMode.Unreliable, ClientMessageID.position);
            // local pos is the position calculated in the OnUpdateFrame thread
            message.AddVector3(localPos - (0, 1.5f, 0));
            // vector containing the body rotation and head yaw and pitch
            message.AddVector3((bodyRot, Game.camera.yaw, Game.camera.pitch));
            NetworkManager.client.Send(message); // sends message constructed above
        }
        #endregion

        public static ushort test;

        #region local movement
        // these fields and methods are static, so that other classes may access them
        static bool onGround = false;
        static bool walking = false;
        static bool _firstMove = true; // wether mouse is moved for first time
        static Vector2 _lastPos; // last position of mouse
        static Vector3 velocity;
        // float upwardsVelocity = 0;
        const float gravity = -1f;
        static bool isFlying = false;
        static double lastJumpTime;
        // Raycaster raycaster;
        public static Vector3i selectedBlockPos;
        public static Vector3i selectedFace;
        public static Vector3 localPos;
        public static Vector3 localPosi;
        public static bool blockSelected = false;
        public static byte blockInHand = 1;

        public static void handleInputs(KeyboardState input, MouseState mouse, float dt)
        {
            // performs a raycast to figure which block the player is facing in
            (selectedBlockPos, selectedFace) = Raycaster.raycast(localPos, Game.camera.Front, 5);
            localPosi = (Vector3i)localPos;

            // increases the player speed if the player is on the ground and the shift key is pressed
            float playerSpeed = (onGround && input.IsKeyDown(Keys.LeftShift)) ? 4f : 2f;
            const float sensitivity = 0.15f;// camera sensitivity

            walking = false;
            if (input.IsKeyDown(Keys.W))
            {
                walking = true;
                velocity += Game.camera.WorldFront * playerSpeed * dt; // Forwards
            }
            if (input.IsKeyDown(Keys.S))
            {
                walking = true;
                velocity -= Game.camera.WorldFront * playerSpeed * dt; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                walking = true;
                velocity -= Game.camera.WorldRight * playerSpeed * dt; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                walking = true;
                velocity += Game.camera.WorldRight * playerSpeed * dt; // Right
            }
            // cancels player velocity in a certain direction if there is a block in said direction
            if (Game.terrainManager.getBlock((Vector3i)(localPos + (velocity.X, -1, 0))) != 0)
            {
                velocity.X = 0;
            }
            if (Game.terrainManager.getBlock((Vector3i)(localPos + (0, -1, velocity.Z))) != 0)
            {
                velocity.Z = 0;
            }

            if (!onGround)
            {
                velocity.Y += gravity * dt;
                if (Game.terrainManager.getBlock((Vector3i)(localPos + (0, -1.5f, velocity.Z))) != 0)
                {
                    velocity.Y = 0f;
                    onGround = true;
                }
            }
            else if (Game.terrainManager.getBlock((Vector3i)(localPos + (0, -1.5f, velocity.Z))) == 0)
            {
                onGround = false; // if there is nothing underneath the player, they should fall
            }
            else if (input.IsKeyDown(Keys.Space))
            {
                if (isFlying)
                {
                    localPos += Game.camera.WorldUp * playerSpeed * dt;
                }
                else
                {
                    velocity.Y = -gravity * 0.2f;
                    onGround = false;
                    lastJumpTime = 0;
                }
            }
            localPos += velocity;

            velocity.X *= 0.8f * dt; // decelerates the player, acts like friction and smoothes movement 
            velocity.Z *= 0.8f * dt;
            // TODO: reimpliment movement, with proper collision and non-framerate-dependant deceleration


            // if (input.IsKeyPressed(Keys.E)) // i just use this for random debugging stuff
            // {
            //     localPos.Deconstruct(out float fx, out float fy, out float fz);
            //     System.Console.WriteLine($"float pos: {fx} , {fy}, {fz}");
            //     int ix = Convert.ToInt32(fx); int iy = Convert.ToInt32(fy); int iz = Convert.ToInt32(fz);
            //     System.Console.WriteLine($"int pos: {ix} , {iy}, {iz}");
            //     System.Console.WriteLine($"{Game.camera.Front}");
            // }

            if (Game.terrainManager.getBlock(selectedBlockPos) != 0)
            {
                blockSelected = true;
            }
            else { blockSelected = false; }
            if (mouse.IsButtonReleased(MouseButton.Left))
            {
                SendBlock(selectedBlockPos, 0, tick);
                Game.terrainManager.setBlock(selectedBlockPos, 0, tick);
            }

            if (mouse.IsButtonPressed(MouseButton.Right) && blockSelected)
            {
                // (selectedBlockPos + selectedFace).Deconstruct(out int sx, out int sy, out int sz);
                var sf = (selectedBlockPos + selectedFace);
                System.Console.WriteLine(selectedFace);
                SendBlock(sf, blockInHand, tick);
                Game.terrainManager.setBlock(sf, blockInHand, tick);
            }

            void SendBlock(Vector3i pos, byte block, int tick)
            {
                Message message = Message.Create(MessageSendMode.Reliable, ClientMessageID.block);
                message.AddVector3i(pos);
                message.Add(block);
                message.Add(tick);
                NetworkManager.client.Send(message);
            }

            if (_firstMove) // This bool variable is initially set to true, used to set up _lastPos
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                Game.camera.Yaw += deltaX * sensitivity;
                Game.camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            }
            if (mouse.ScrollDelta.Y >= 1)
            {
                blockInHand++;
                blockInHand %= (byte)(TextureGenerator.blockCount + 1);

            }
            if (mouse.ScrollDelta.Y <= -1)
            {

                blockInHand--;
                blockInHand %= (byte)(TextureGenerator.blockCount + 1);
            }
        }
        #endregion
    }
}
