﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StackAttack.Engine;
using StackAttack.Engine.Helpers;
using StackAttack.Engine.Map;

namespace StackAttack
{
    public class Game : GameWindow
    {
        public static int ViewportX { get; set; } = 0;
        public static int ViewportY { get; set; } = 0;
        public static int ViewportWidth { get; set; } = 64;
        public static int ViewportHeight { get; set; } = 64;
        public static int WindowWidth { get; set; } = 512;
        public static int WindowHeight { get; set; } = 512;
        public static bool Fullscreen = false;

        public Scenes.Scene CurrentScene { get; private set; }

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            CurrentScene = new Scenes.GameScene(this);
        }

        public static void LoadDefinitionData<T>(string path, ref T data)
        {
            string input = File.ReadAllText(path);
            T? output = System.Text.Json.JsonSerializer.Deserialize<T>(input);
            if (output == null)
                return;
            data = output;
        }

#if DEBUG
        public static void SaveDefintionData<T>(string path, T data)
        {
            string result = System.Text.Json.JsonSerializer.Serialize(data, typeof(T), new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, result);
        }
#endif

        protected override void OnResize(ResizeEventArgs e)
        {
            if (!Fullscreen)
            {
                Size = new Vector2i(WindowWidth, WindowHeight);
                base.OnResize(e);
            }
        }

        protected override void OnLoad()
        {
            base.OnLoad();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            CurrentScene.Update(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {

            if (IsExiting)
            {
                return;
            }

            CurrentScene.Draw(args);

            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        public static void SetViewport(int x, int y, int width, int height, int renderWidth, int renderHeight)
        {
            GL.Viewport(x, y, width, height);
            ViewportWidth = renderWidth;
            ViewportHeight = renderHeight;
            ViewportX = x;
            ViewportY = y;
        }

        protected override void OnUnload()
        {
            CurrentScene.Dispose();
            ContentManager.RemoveAll();
            base.OnUnload();
        }
    }
}
