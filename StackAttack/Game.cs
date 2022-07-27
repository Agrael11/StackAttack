using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace StackAttack
{
    class Game : GameWindow
    {
        public static float rotato = 0;
        public static int ViewportX { get; set; } = 0;
        public static int ViewportY { get; set; } = 0;
        public static int ViewportWidth { get; set; } = 64;
        public static int ViewportHeight { get; set; } = 64;
        public static int WindowWidth { get; set; } = 512;
        public static int WindowHeight { get; set; } = 512;
        public static bool Fullscreen = false;

        public string BGMap =
            "0000000000000000\n" +
            "0111101111111111\n" +
            "0111101111111111\n" +
            "0111111111111111\n" +
            "0111101110000000\n" +
            "0111101110333330\n" +
            "0000001110333330\n" +
            "2222221110333330\n" +
            "2222221113333330\n" +
            "0002001110333330\n" +
            "0222201110333330\n" +
            "0222201110000000\n" +
            "0222201110333330\n" +
            "0222201113333330\n" +
            "0222201110333330\n" +
            "0000000000000000";

        public string FGMap =
            "1111122222222222\n" +
            "1000020000000002\n" +
            "1000020000000002\n" +
            "4009050000000002\n" +
            "1000020003333333\n" +
            "1000020003000003\n" +
            "1222220003000003\n" +
            "000000A003000003\n" +
            "0000000005000003\n" +
            "1115110003000003\n" +
            "1000010003000003\n" +
            "1000010003333333\n" +
            "1000010003000003\n" +
            "1000010006000003\n" +
            "1080017003000003\n" +
            "1111112223333333";
        public string[,] Background = new string[16, 16];
        public string[,] Walls = new string[16, 16];

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        List<Shader.ShaderDefinition> shaderDefinitions = new();
        List<Texture.TextureDefinition> textureDefinitions = new();
        List<Sprite.SpriteDefinition> spriteDefinitions = new();
        List<Tile.TileDefinition> tileDefinitions = new();


        int texturePosition = 32; 
        float desaturation = 0.85f;
        float brightness = 0.6f;

        private void LoadDefinitions()
        {
            foreach (var shaderDefinition in shaderDefinitions)
            {
                ContentManager.Load<Shader>(shaderDefinition.ShaderID, shaderDefinition.FileName);
            }

            foreach (var textureDefinition in textureDefinitions)
            {
                ContentManager.Load<Texture>(textureDefinition.TextureID, textureDefinition.FileName);
            }

            foreach (var tileDefinition in tileDefinitions)
            {
                ContentManager.Add(tileDefinition.TileID, new Tile(tileDefinition.TextureID, tileDefinition.ShaderID, new Vector2i(tileDefinition.X, tileDefinition.Y), new Vector2i(tileDefinition.Width, tileDefinition.Height)));
            }

            foreach (var spriteDefinition in spriteDefinitions)
            {
                ContentManager.Add(spriteDefinition.SpriteID, new Sprite(spriteDefinition.TextureID, spriteDefinition.ShaderID, new Vector2i(spriteDefinition.X, spriteDefinition.Y), new Vector2i(spriteDefinition.Width, spriteDefinition.Height)));
            }
        }

        private void LoadDefintionData<T>(string path, ref List<T> data)
        {
            string input = File.ReadAllText(path);
            List<T>? output = System.Text.Json.JsonSerializer.Deserialize<List<T>>(input);
            if (output == null)
                return;
            data = output;
        }

        private void SaveDefintionData<T>(string path, List<T> data)
        {
            string result = System.Text.Json.JsonSerializer.Serialize(data, typeof(List<T>), new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, result);
        }

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

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    switch (BGMap.Split('\n')[y][x])
                    {
                        case '1':
                            Background[x, y] = "BlueFloor";
                            break;
                        case '2':
                            Background[x, y] = "GreenFloor";
                            break;
                        case '3':
                            Background[x, y] = "WoodFloor";
                            break;
                    }
                    switch (FGMap.Split('\n')[y][x])
                    {
                        case '1':
                            Walls[x, y] = "WoodWall";
                            break;
                        case '2':
                            Walls[x, y] = "BrickWall";
                            break;
                        case '3':
                            Walls[x, y] = "StoneWall";
                            break;
                        case '4':
                            Walls[x, y] = "Exit";
                            break;
                        case '5':
                            Walls[x, y] = "BlueDoor";
                            break;
                        case '6':
                            Walls[x, y] = "GoldDoor";
                            break;
                        case '7':
                            Walls[x, y] = "Key";
                            break;
                        case '8':
                            Walls[x, y] = "Chest";
                            break;
                        case '9':
                            Walls[x, y] = "Enemy";
                            break;
                        case 'A':
                            Walls[x, y] = "Player";
                            break;
                    }

                }
            }

            LoadDefintionData("Shaders/shaderDefinitions.json", ref shaderDefinitions);
            LoadDefintionData("Textures/textureDefinitions.json", ref textureDefinitions);
            LoadDefintionData("Textures/spriteDefinitions.json", ref spriteDefinitions);
            LoadDefintionData("Textures/tileDefinitions.json", ref tileDefinitions);
            LoadDefinitions();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            renderTexture = new RenderTexture(64, 64, "BaseShader", "");
            renderTexture2 = new RenderTexture(64, 64, "Desaturated", "");
            
            Logger.Log(Logger.Levels.Info, "Saturation Demo Keyboard info:\nD & A - Move Desat Shader Preview\nW - Increase Saturation\nS - Decrease Saturation\nNumpad Plus - Increase Brightness\nNumpad Minus - Decrease Brightness\nDefault Values: " + $"TexturePosition: {texturePosition}, Desaturation: {desaturation:0.##} / Saturation: {(1 - desaturation):0.##}, Brightness: {brightness:0.##}");

            base.OnLoad();
        }

        RenderTexture renderTexture;
        RenderTexture renderTexture2;

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            KeyboardState input = KeyboardState.GetSnapshot();
            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            };

            base.OnUpdateFrame(args);
        }

        public static void SetViewport(int x, int y, int width, int height, int renderWidth, int renderHeight)
        {
            GL.Viewport(x, y, width, height);
            ViewportWidth = renderWidth;
            ViewportHeight = renderHeight;
            ViewportX = x;
            ViewportY = y;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            if (KeyboardState.IsKeyDown(Keys.D))
            {
                texturePosition++;
                if (texturePosition > 64) texturePosition = 64;
            }
            else if (KeyboardState.IsKeyDown(Keys.A))
            {
                texturePosition--;
                if (texturePosition < 0) texturePosition = 0;
            }
            else if (KeyboardState.IsKeyDown(Keys.S))
            {
                desaturation += 0.01f;
                if (desaturation > 1) desaturation = 1;
            }
            else if (KeyboardState.IsKeyDown(Keys.W))
            {
                desaturation -= 0.01f;
                if (desaturation < 0) desaturation = 0;
            }
            else if (KeyboardState.IsKeyDown(Keys.KeyPadAdd))
            {
                brightness += 0.01f;
                if (brightness > 10) brightness = 10;
            }
            else if (KeyboardState.IsKeyDown(Keys.KeyPadSubtract))
            {
                brightness -= 0.01f;
                if (brightness < 0) brightness = 0;
            }
            rotato = (rotato + 0.1f) % (float)Math.Tau;
            string info = $"TexturePosition: {texturePosition}, Desaturation: {desaturation:0.##} / Saturation: {(1-desaturation):0.##}, Brightness: {brightness:0.##}";
            this.Title = info;
            if (KeyboardState.IsKeyReleased(Keys.W) || KeyboardState.IsKeyReleased(Keys.S) || KeyboardState.IsKeyReleased(Keys.A) || KeyboardState.IsKeyReleased(Keys.D) || KeyboardState.IsKeyReleased(Keys.KeyPadAdd) || KeyboardState.IsKeyReleased(Keys.KeyPadSubtract))
            {
                Logger.Log(Logger.Levels.Info, info);
            }
            renderTexture.Begin();
            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            for (int y = 0; y <16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    string tileID = Background[x, y];
                    if (!string.IsNullOrWhiteSpace(tileID) && !tileID.Contains("Dark"))
                    {
                        Tile.Draw(tileID, new Vector2i(x * 4, y * 4), 0, false, false);
                    }

                    tileID = Walls[x, y];
                    if (!string.IsNullOrWhiteSpace(tileID) && !tileID.Contains("Dark"))
                    {
                        if (tileID == "Player")
                        {
                            Tile.Draw(tileID, new Vector2i(x * 4, y * 4), rotato, false, false);
                        }
                        else
                        {
                            Tile.Draw(tileID, new Vector2i(x * 4, y * 4), (y == 9&&x==3)?(float)(Math.PI/2):0, false, false);
                        }
                    }
                }
            }

            renderTexture.End();
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            ContentManager.Get<Shader>("Desaturated").SetFloat("brightness", brightness);
            ContentManager.Get<Shader>("Desaturated").SetFloat("desaturation", desaturation);

            Sprite.DrawTexture(renderTexture.Sprite.TextureID, new Vector2i(0, 0), new Vector2i(texturePosition, 64), "Desaturated", new Vector2i(0, 0), new Vector2i(texturePosition, 64));
            Sprite.DrawTexture(renderTexture.Sprite.TextureID, new Vector2i(texturePosition, 0), new Vector2i(64 - texturePosition, 64), "BaseShader", new Vector2i(texturePosition, 0), new Vector2i(64 - texturePosition, 64));

            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUnload()
        {
            ContentManager.RemoveAll();
            base.OnUnload();
        }
    }
}
