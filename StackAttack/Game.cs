using OpenTK;
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

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        List<Shader.ShaderDefinition> shaderDefinitions = new();
        List<Texture.TextureDefinition> textureDefinitions = new();
        List<Sprite.SpriteDefinition> spriteDefinitions = new();
        List<Tile.TileDefinition> tileDefinitions = new();
        List<GameObject> gameObjects = new();
        LevelData level;

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

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            IEnumerable<GameObject> gameObjectTypes = typeof(GameObject).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(GameObject)) && !t.IsAbstract)
                .Select(t => (GameObject)Activator.CreateInstance(t));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            foreach (GameObject gameObjectType in gameObjectTypes)
            {
                ContentManager.Add<GameObject>(gameObjectType.GetType().Name, gameObjectType);
            }

        }

        private void LoadDefinitionData<T>(string path, ref T data)
        {
            string input = File.ReadAllText(path);
            T? output = System.Text.Json.JsonSerializer.Deserialize<T>(input);
            if (output == null)
                return;
            data = output;
        }

        private void SaveDefintionData<T>(string path, T data)
        {
            string result = System.Text.Json.JsonSerializer.Serialize(data, typeof(T), new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
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
            LoadDefinitionData("Shaders/shaderDefinitions.json", ref shaderDefinitions);
            LoadDefinitionData("Textures/textureDefinitions.json", ref textureDefinitions);
            LoadDefinitionData("Textures/spriteDefinitions.json", ref spriteDefinitions);
            LoadDefinitionData("Textures/tileDefinitions.json", ref tileDefinitions);
            LoadDefinitionData("Levels/AlphaLevel.json", ref level);
            LoadDefinitions();
            
            foreach (GameObjectStartData objectData in level.GameObjectStartDatas)
            {
                (bool returnResult, GameObject? returnObject) = ContentManager.Get<GameObject>(objectData.GameObjectTypeID);
                if (returnResult == true && returnObject is not null)
                {
                    gameObjects.Add(returnObject.CreateNew(objectData.ObjectX, objectData.ObjectY, 0, objectData.Heading));
                }
            }
            gameObjects.Add(new Objects.Player(level.PlayerStartData.PlayerX, level.PlayerStartData.PlayerY, 0, level.PlayerStartData.Heading, level.PlayerStartData.SpriteID));

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            renderTexture = new RenderTexture(64, 64, "BaseShader", "");
            
            Logger.Log(Logger.Levels.Info, "Saturation Demo Keyboard info:\nD & A - Move Desat Shader Preview\nW - Increase Saturation\nS - Decrease Saturation\nNumpad Plus - Increase Brightness\nNumpad Minus - Decrease Brightness\nDefault Values: " + $"TexturePosition: {texturePosition}, Desaturation: {desaturation:0.##} / Saturation: {(1 - desaturation):0.##}, Brightness: {brightness:0.##}");

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
            string info = $"TexturePosition: {texturePosition}, Desaturation: {desaturation:0.##} / Saturation: {(1 - desaturation):0.##}, Brightness: {brightness:0.##}";
            if (KeyboardState.IsKeyReleased(Keys.W) || KeyboardState.IsKeyReleased(Keys.S) || KeyboardState.IsKeyReleased(Keys.A) || KeyboardState.IsKeyReleased(Keys.D) || KeyboardState.IsKeyReleased(Keys.KeyPadAdd) || KeyboardState.IsKeyReleased(Keys.KeyPadSubtract))
            {
                Logger.Log(Logger.Levels.Info, info);
            }

            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Update(args);
            }

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
            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            
            foreach (TileData tile in level.Background._tiles)
            {
                Tile.Draw(tile.TileID, new Vector2i(tile.TileX*4, tile.TileY*4), tile.GetTileRotationRad());
            }
            
            foreach (TileData tile in level.Foreground._tiles)
            {
                Tile.Draw(tile.TileID, new Vector2i(tile.TileX * 4, tile.TileY * 4), tile.GetTileRotationRad());
            }
            
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Draw(args);
            }

            renderTexture.End();
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            (bool returnResult, Shader? returnShader) = ContentManager.Get<Shader>("Desaturated");
            if (returnResult == true && returnShader is not null)
            { 
                returnShader.SetFloat("brightness", brightness);
                returnShader.SetFloat("desaturation", desaturation);
            }

            if (renderTexture.Sprite.returnResult && renderTexture.Sprite.spriteResult is not null)
            {
                Sprite.DrawTexture(renderTexture.Sprite.spriteResult.TextureID, new Vector2i(0, 0), new Vector2i(texturePosition, 64), "Desaturated", new Vector2i(0, 0), new Vector2i(texturePosition, 64), 0, false, true);
                Sprite.DrawTexture(renderTexture.Sprite.spriteResult.TextureID, new Vector2i(texturePosition, 0), new Vector2i(64 - texturePosition, 64), "BaseShader", new Vector2i(texturePosition, 0), new Vector2i(64 - texturePosition, 64), 0, false, true);
            }

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
