﻿using System;
using Larx.Terrain;
using Larx.Object;
using Larx.Text;
using Larx.UserInterFace;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using GL4 = OpenTK.Graphics.OpenGL4;

namespace Larx
{
    class Program : GameWindow
    {
        private int FPS;
        private double lastFPSUpdate;
        private Multisampling multisampling;
        private PolygonMode polygonMode;

        private Camera camera;
        private Light light;
        private ObjectRenderer debug;
        private TerrainRenderer terrain;
        private MousePicker mousePicker;
        private Ui ui;

        private KeyboardState keyboard;

        private float scaleFactor;
        private float radius;
        private float hardness;
        private int lastScroll;
        private Point lastMouse;

        public Program() : base(
            1280, 720,
            new GraphicsMode(32, 24, 0, 0), "Larx", 0,
            DisplayDevice.Default, 4, 0,
            GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
        {
            lastMouse = new Point();
            scaleFactor = Width / 1280f;
            lastFPSUpdate = 0;
            polygonMode = PolygonMode.Fill;
            radius = 3f;
            hardness = 0.5f;
        }

        protected override void OnLoad(EventArgs e)
        {
            multisampling = new Multisampling(ClientSize.Width, ClientSize.Width, 8);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL4.GL.BlendFuncSeparate(GL4.BlendingFactorSrc.SrcAlpha, GL4.BlendingFactorDest.OneMinusSrcAlpha, GL4.BlendingFactorSrc.One, GL4.BlendingFactorDest.One);

            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color.FromArgb(255, 24, 24, 24));
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            ui = new Ui();
            terrain = new TerrainRenderer();
            debug = new ObjectRenderer();
            camera = new Camera();
            light = new Light();
            mousePicker = new MousePicker(camera);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            var mouse = Mouse.GetCursorState();
            var mousePos = this.PointToClient(new Point(mouse.X, mouse.Y));
            var uiIntersect = ui.Update(mousePos, mouse.LeftButton);

            if (mouse.ScrollWheelValue > lastScroll) camera.Zoom(-0.2f);
            if (mouse.ScrollWheelValue < lastScroll) camera.Zoom( 0.2f);
            if (mouse.MiddleButton == ButtonState.Pressed) camera.Rotate(new Vector2((float)(mousePos.X - lastMouse.X), (float)(mousePos.Y - lastMouse.Y)));

            if (keyboard[Key.W]) camera.Move(new Vector3( 0.0f, 0.0f, 1.0f));
            if (keyboard[Key.S]) camera.Move(new Vector3( 0.0f, 0.0f,-1.0f));
            if (keyboard[Key.A]) camera.Move(new Vector3( 1.0f, 0.0f, 0.0f));
            if (keyboard[Key.D]) camera.Move(new Vector3(-1.0f, 0.0f, 0.0f));

            if (keyboard[Key.Up]) light.Position += new Vector3( 0.0f, 0.0f, 1.0f);
            if (keyboard[Key.Down]) light.Position += new Vector3( 0.0f, 0.0f,-1.0f);
            if (keyboard[Key.Left]) light.Position += new Vector3( 1.0f, 0.0f, 0.0f);
            if (keyboard[Key.Right]) light.Position += new Vector3(-1.0f, 0.0f, 0.0f);

            camera.Update((float)e.Time);
            mousePicker.Update(mousePos.X, mousePos.Y, Width, Height);

            if (!uiIntersect) {
                if (mouse.LeftButton == ButtonState.Pressed) terrain.ChangeElevation(0.1f, radius, hardness, mousePicker);
                if (mouse.RightButton == ButtonState.Pressed) terrain.ChangeElevation(-0.1f, radius, hardness, mousePicker);
            }

            lastFPSUpdate += e.Time;
            if (lastFPSUpdate > 1)
            {
                Title = $"Larx (Vsync: {VSync}) - FPS: {FPS}";
                FPS = 0;
                lastFPSUpdate %= 1;
            }

            lastScroll = mouse.ScrollWheelValue;
            lastMouse = new Point(mousePos.X, mousePos.Y);

            var pos = terrain.GetPosition(mousePicker);
            ui.UpdateText("position", $"Position: {pos.X:0.##} {pos.Z:0.##}");
            ui.UpdateText("size", $"Tool Size: {radius}");
            ui.UpdateText("hardness", $"Hardness: {MathF.Round(hardness, 1)}");
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            multisampling.Bind();
            FPS++;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.PolygonMode(MaterialFace.FrontAndBack, polygonMode);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);

            terrain.Render(camera, light);
            debug.Render(camera, light.Position / 10.0f);
            multisampling.Draw();

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);

            ui.Render();

            SwapBuffers();
        }

        protected override void OnResize(EventArgs e)
        {
            camera.AspectRatio = (float)Width / (float)Height;
            ui.Resize(new SizeF(Width / scaleFactor, Height / scaleFactor));

            GL.Viewport(0, 0, Width, Height);
            multisampling.RefreshBuffers(Width, Height);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            var mouse = Mouse.GetCursorState();
            var mousePos = this.PointToClient(new Point(mouse.X, mouse.Y));
            var uiIntersect = ui.Click(mousePos, mouse.LeftButton);

            if (uiIntersect == Keys.Terrain.SizeIncrease) {
                radius ++;
                if (radius > 12.0f) radius = 12.0f;
            }

            if (uiIntersect == Keys.Terrain.SizeDecrease) {
                radius --;
                if (radius < 0.0f) radius = 0.0f;
            }

            if (uiIntersect == Keys.Terrain.HardnessIncrease) {
                hardness += 0.1f;
                if (hardness > 1.0f) hardness = 1.0f;
            }

            if (uiIntersect == Keys.Terrain.HardnessDecrease) {
                hardness -= 0.1f;
                if (hardness < 0.0f) hardness = 0.0f;
            }
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (!e.IsRepeat)
            {
                if (e.Keyboard[Key.Escape])
                    Exit();

                if (e.Control && e.Keyboard[Key.F])
                    WindowState = WindowState == WindowState.Fullscreen ? WindowState.Normal : WindowState.Fullscreen;

                if (e.Control && e.Keyboard[Key.W])
                    polygonMode = polygonMode == PolygonMode.Fill ? PolygonMode.Line : PolygonMode.Fill;

                if (e.Control && e.Keyboard[Key.G])
                    terrain.ShowGridLines = !terrain.ShowGridLines;
            }

            if (!e.Control)
                keyboard = e.Keyboard;
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            keyboard = e.Keyboard;
        }

        public static void Main(string[] args)
        {
            using (var program = new Program())
            {
                program.VSync = VSyncMode.Off;
                program.Run(60);
            }
        }
    }
}
