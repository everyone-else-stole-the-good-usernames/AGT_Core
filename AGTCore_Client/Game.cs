using System.Threading;
using System.Collections.Concurrent;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace AGTCore
{
    public class Game : GameWindow
    {

        public static Shader shader;
        public static Camera camera;


        Overlay overlay;

        public static TerrainManager terrainManager;
        double currentTime = 0;
        int nbFrames = 0;
        const int offset = 10000;
        public static ConcurrentQueue<int> buffersToDelete = new ConcurrentQueue<int>();

        public Game(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title }) { }

        public static bool startGame = false;

        protected override void OnLoad()
        {
            base.OnLoad();
            shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");

            GL.ClearColor(0.7f, 0.8f, 1f, 1.0f);
            GL.Enable(EnableCap.DepthTest);


            shader.Use();
            camera = new Camera(new Vector3(offset, 20, offset), Size.X / (float)Size.Y);

            // Thread networkThread = new Thread(new ThreadStart(Program.NetworkThread));
            // networkThread.Start();
            // Console.WriteLine("Connecting...");
            // Program.networkManager.Connect();
            overlay = new Overlay(shader);

            terrainManager.initTerrain();
            overlay.InitMesh();
            overlay.RenderMesh();


            CursorState = CursorState.Grabbed; // hides and captures mouse


        }


        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            nbFrames++;
            currentTime += e.Time;
            // if (currentTime >= 0.25)
            // {
            //     System.Console.WriteLine(nbFrames * 4);
            //     nbFrames = 0;
            //     currentTime = 0;
            // }

            // clears the screen using the colour set in Onload()
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // sends matrix view and projection matrixes to the GPU shader
            shader.SetMatrix4("view", camera.GetViewMatrix()); // view matrix rotates vertices to match camera rotation
            shader.SetMatrix4("projection", camera.GetProjectionMatrix()); // projection matrix projects 3d coords to 2d screen space coords
            terrainManager.renderTerrain(((int)LocalPlayer.localPos.X), ((int)LocalPlayer.localPos.Z));
            overlay.RenderMesh(); // renders the ui overlay
            EntityManager.RenderEntities(); // renders all entities

            // swaps between screen buffers (one is written to while the other is displayed to avoid screen tearing)
            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // used to resize screenspace coords (-1 to 1) to the size of the window
            GL.Viewport(0, 0, e.Width, e.Height);
            camera.AspectRatio = Size.X / (float)Size.Y;
        }
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (!IsFocused) // Check to see if the window is focused
            {
                return;
            }
            var input = KeyboardState;
            var mouse = MouseState;

            if ((camera.Position.Y < -5) || input.IsKeyDown(Keys.Escape))
            {
                Close();
            }
            LocalPlayer.handleInputs(input, mouse, (float)e.Time);
            while (buffersToDelete.TryDequeue(out int buffer))
            {
                GL.DeleteBuffer(buffer);
            }
        }
        protected override void OnUnload()
        {
            // Unbind all the resources by binding the targets to 0/null.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
            terrainManager.unloadTerrain();

            shader.Dispose();

            base.OnUnload();
        }

    }


}