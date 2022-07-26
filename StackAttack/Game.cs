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

        public string BGMap =
            "0000000000000000\n" +
            "0111101111122000\n" +
            "0111101111220000\n" +
            "0111111112200000\n" +
            "0111101122000000\n" +
            "0111101220000000\n" +
            "0000002220000000\n" +
            "1111112220000000\n" +
            "1111112220000000\n" +
            "0001001220000000\n" +
            "0111101120000000\n" +
            "0111101110000000\n" +
            "0111101110000000\n" +
            "0111101110000000\n" +
            "0111101110000000\n" +
            "0000000000000000";

        public string FGMap =
            "1111111111112200\n" +
            "1000010000000000\n" +
            "1000010000000000\n" +
            "3009040000000000\n" +
            "1000010000000000\n" +
            "1000010002000000\n" +
            "1111110002000000\n" +
            "000000A002000000\n" +
            "0000000005000000\n" +
            "1114110002000000\n" +
            "1000010002000000\n" +
            "1000010002000000\n" +
            "1000010001000000\n" +
            "1000010006000000\n" +
            "1070018001000000\n" +
            "1111111111000000";
        public string[,] Background = new string[16, 16];
        public string[,] Walls = new string[16, 16];

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        List<Shader.ShaderDefinition> shaderDefinitions = new();
        List<Texture.TextureDefinition> textureDefinitions = new();
        List<Sprite.SpriteDefinition> spriteDefinitions = new();
        List<Tile.TileDefinition> tileDefinitions = new();

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

        protected override void OnLoad()
        {

            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    switch (BGMap.Split('\n')[y][x])
                    {
                        case '1':
                            Background[x, y] = "DarkGroundTile";
                            break;
                        case '2':
                            Background[x, y] = "GroundTile";
                            break;
                    }
                    switch (FGMap.Split('\n')[y][x])
                    {
                        case '1':
                            Walls[x, y] = "DarkWoodTile";
                            break;
                        case '2':
                            Walls[x, y] = "WoodTile";
                            break;
                        case '3':
                            Walls[x, y] = "DarkExit";
                            break;
                        case '4':
                            Walls[x, y] = "DarkDoor";
                            break;
                        case '5':
                            Walls[x, y] = "Door";
                            break;
                        case '6':
                            Walls[x, y] = "DarkGoldDoor";
                            break;
                        case '7':
                            Walls[x, y] = "DarkChest";
                            break;
                        case '8':
                            Walls[x, y] = "DarkKey";
                            break;
                        case '9':
                            Walls[x, y] = "DarkEnemy";
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

            base.OnLoad();
        }

        RenderTexture renderTexture;

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
            renderTexture.Begin();
            rotato = (rotato + 0.1f) % (float)Math.Tau;
            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            for (int y = 0; y <16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    string tileID = Background[x, y];
                    if (!string.IsNullOrWhiteSpace(tileID))
                    {
                        Tile.Draw(tileID, new Vector2i(x * 4, y * 4), 0, false, false);
                    }

                    tileID = Walls[x, y];
                    if (!string.IsNullOrWhiteSpace(tileID))
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

            renderTexture.Sprite.Draw(new Vector2i(0, 0), 0, false, true);

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
