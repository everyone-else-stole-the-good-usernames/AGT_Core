using OpenTK.Mathematics;
using System.Collections.Generic;
using Riptide;

namespace AGTCore
{
    public class NetworkPlayer : Player
    {
        Matrix4 headPosM4; // used as a starting point for head rotation
        PlayerEntity playerEntity;
        public override void Update()
        {
            // applied transformations recieved from the server
            // these matrixes are used by the entity rendering class and OpenGL to translate its vertices to place
            playerEntity.modelTransform = Matrix4.Identity * Matrix4.CreateRotationY(-rotY) * Matrix4.CreateTranslation(playerPos);
            playerEntity.headTransform = headPosM4 * Matrix4.CreateRotationZ(headRotZ) * Matrix4.CreateRotationY(-headRotY) * Matrix4.CreateTranslation(playerPos + (0, 1.333f, 0));
        }
        public NetworkPlayer(ushort _id, string _username, Vector3 _spawnPos) : base(_id, _username, _spawnPos)
        {
            headPosM4 = Matrix4.Identity * Matrix4.CreateTranslation((0, -1.333f, 0)); // calculated once, not every time
            // System.Console.WriteLine($"spawned player {_id}");
            playerEntity = new PlayerEntity(_spawnPos); // creates a new player entity (3d model), so network players can be seen
            LocalPlayer.test = _id;
            beginUpdating();
        }
    }
}
