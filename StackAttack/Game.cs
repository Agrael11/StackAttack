using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using StackAttack.Engine;
using StackAttack.Engine.Helpers;

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
        public static bool Fullscreen { get; set; } = false;
        public static Sound? BackgroundMusic { get; set; } = null;
        public static int Ammo { get; set; } = 0;
        public static int Score { get; set; } = 0;

        public Scenes.Scene CurrentScene { get; private set; }
        private Scenes.Scene? NextScene { get; set; }

        private RenderTexture mainTexture;
        public string currentLevel = "AlphaLevel";

        public void SwitchScene(Scenes.Scene scene)
        {
            if (BackgroundMusic is not null)
            {
                BackgroundMusic.StopUseSound();
                BackgroundMusic.Dispose();
                BackgroundMusic = null;
            }
            NextScene = scene;
        }

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            //CurrentScene = new Scenes.CreditsScene(this);
            CurrentScene = new Scenes.StartScene(this);
            CurrentScene.Init();
            mainTexture = new(64, 64, "BaseShader");
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

            if (NextScene is not null)
            {
                CurrentScene.Dispose();
                CurrentScene = NextScene;
                CurrentScene.Init();
                mainTexture.Reregister();
                NextScene = null;
            }

            if (BackgroundMusic is not null)
            {
                BackgroundMusic.Update();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {

            if (IsExiting)
            {
                return;
            }

            CurrentScene.Draw(args, ref mainTexture);

            if (mainTexture.Sprite.returnResult && mainTexture.Sprite.spriteResult is not null)
            {
                mainTexture.Sprite.spriteResult.Draw(new Rectanglei(0,0,64,64), 0, false, true);
            }

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
            if (BackgroundMusic is not null)
            {
                BackgroundMusic.StopUseSound();
                BackgroundMusic.Dispose();
            }
            CurrentScene.Dispose();
            ContentManager.RemoveAll();
            base.OnUnload();
        }
    }
}
