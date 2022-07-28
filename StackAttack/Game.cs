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
    public class Game : GameWindow
    {
        public static int ViewportX { get; set; } = 0;
        public static int ViewportY { get; set; } = 0;
        public int CameraX { get; set; } = 0;
        public int CameraY { get; set; } = 0;
        public static int ViewportWidth { get; set; } = 64;
        public static int ViewportHeight { get; set; } = 64;
        public static int WindowWidth { get; set; } = 512;
        public static int WindowHeight { get; set; } = 512;
        public static bool Fullscreen = false;
        
        List<Shader.ShaderDefinition> shaderDefinitions = new();
        List<Texture.TextureDefinition> textureDefinitions = new();
        List<Sprite.SpriteDefinition> spriteDefinitions = new();
        List<Tile.TileDefinition> tileDefinitions = new();
        LevelData level = new();
        RenderTexture rayCastRenderTexture = new();
        RenderTexture gameRenderTexture = new();
        RenderTexture tempRenderTexture = new();
        RenderTexture memoryRenderTexture = new();

        public List<GameObject> gameObjects { get; set; } = new();
        public GameObject? player { get; set; }
        public TileMap Background { get; set; } = new();
        public TileMap Foreground { get; set; } = new();

        public int Score { get; set; } = 0;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        private IEnumerable<T> GetAllInherited<T>()
        {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            return typeof(T).Assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(T)) && !t.IsAbstract)
                .Select(t => (T)Activator.CreateInstance(t));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
        }

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

            IEnumerable<GameObject> gameObjectTypes = GetAllInherited<GameObject>();
            foreach (GameObject gameObjectType in gameObjectTypes)
            {
                ContentManager.Add<GameObject>(gameObjectType.GetType().Name, gameObjectType);
            }
            Background = level.Background.Clone();
            Foreground = level.Foreground.Clone();
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
                    gameObjects.Add(returnObject.CreateNew(objectData.ObjectX, objectData.ObjectY, 0, objectData.Heading, this, objectData.SpriteID));
                }
            }
            player = new Objects.Player(level.PlayerStartData.PlayerX, level.PlayerStartData.PlayerY, 0, level.PlayerStartData.Heading, this, level.PlayerStartData.SpriteID);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            gameRenderTexture = new RenderTexture(64, 64, "BaseShader", "");
            tempRenderTexture = new RenderTexture(64, 64, "BaseShader", "");
            memoryRenderTexture = new RenderTexture(level.LevelWidth*4, level.LevelHeight*4, "Desaturated", "");
            rayCastRenderTexture = new RenderTexture(64, 64, "BaseShader", "");

            base.OnLoad();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Update(args);
            }

            if (player is null)
                return;

            player.Update(args);

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

        public (bool intersects, Vector2 point) LinesCollisions(float x1, float x2, float y1, float y2, float x3, float x4, float y3, float y4)
        {
            //float D = ((x1 - x2) * (y3 - y4)) - ((y1 - y2) * (x3 - x4));
            //float Px = ((((x1 * y2) - (y1 * x2)) * (x3 - x4)) - ((x1 - x2) * ((x3 * y4) - (y3 * x4))))/D;
            //float Py = ((((x1 * y2) - (y1 * x2)) * (y3 - y4)) - ((y1 - y2) * ((x3 * y4) - (y3 * x4))))/D;
            float D = ((x1 - x2) * (y3 - y4)) - ((y1 - y2) * (x3 - x4));
            float t = (((x1 - x3) * (y3 - y4)) - ((y1 - y3) * (x3 - x4))) / D;
            float u = (((x1 - x3) * (y1 - y2)) - ((y1 - y3) * (x1 - x2))) / D;
            
            bool intersect = (t >= 0) && (t <= 1) && (u >= 0) && (u <= 1);
            if (!intersect)
                return (false, Vector2.Zero);

            float Px = x1 + (t * (x2 - x1));
            float Py = y1 + (t * (y2- y1));

            return (true, new Vector2(Px, Py));
        }

        public (bool result, GameObject? resultObject, TileData? tile) CastRay(Vector2 sourcePosition, float angle, GameObject source, TileMap collisionMap, List<GameObject> gameObjects, bool draw, params Type[] TargetTypes)
        {
            Vector2 angleVector = Extensions.FromAngle(angle, 32);
            Vector2 destinationPosition = new Vector2(sourcePosition.X + angleVector.X, sourcePosition.Y + angleVector.Y);
            Rectangle srcR = new Rectangle(sourcePosition, angleVector.X, angleVector.Y);


            float closest = float.MaxValue;
            TileData? closestTile = null;

            foreach (TileData tile in collisionMap.Tiles)
            {
                Rectangle colR = tile.GetRectangle();
                var result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X, colR.Y, colR.Y2);
                if (result.intersects)
                {
                    if (result.point.Distance(sourcePosition) < closest)
                    {
                        closest = result.point.Distance(sourcePosition);
                        destinationPosition = result.point;
                        closestTile = tile;
                    }
                }
                result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X2, colR.X2, colR.Y, colR.Y2);
                if (result.intersects)
                {
                    if (result.point.Distance(sourcePosition) < closest)
                    {
                        closest = result.point.Distance(sourcePosition);
                        destinationPosition = result.point;
                        closestTile = tile;
                    }
                }
                result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X2, colR.Y, colR.Y);
                if (result.intersects)
                {
                    if (result.point.Distance(sourcePosition) < closest)
                    {
                        closest = result.point.Distance(sourcePosition);
                        destinationPosition = result.point;
                        closestTile = tile;
                    }
                }
                result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X2, colR.Y2, colR.Y2);
                if (result.intersects)
                {
                    if (result.point.Distance(sourcePosition) < closest)
                    {
                        closest = result.point.Distance(sourcePosition);
                        destinationPosition = result.point;
                        closestTile = tile;
                    }
                }
            }

            GameObject? closestObject = null;

            if (TargetTypes is not null && (TargetTypes.Length > 0))
            {
                foreach (GameObject gameObject in gameObjects)
                {
                    if (gameObject == source)
                        continue;

                    if (!TargetTypes.Contains(gameObject.GetType()))
                        continue;

                    Rectangle colR = new Rectangle(gameObject.Location, new Vector2(4,4));
                    var result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X, colR.Y, colR.Y2);
                    if (result.intersects)
                    {
                        if (result.point.Distance(sourcePosition) < closest)
                        {
                            closest = result.point.Distance(sourcePosition);
                            destinationPosition = result.point;
                            closestObject = gameObject;
                        }
                    }
                    result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X2, colR.X2, colR.Y, colR.Y2);
                    if (result.intersects)
                    {
                        if (result.point.Distance(sourcePosition) < closest)
                        {
                            closest = result.point.Distance(sourcePosition);
                            destinationPosition = result.point;
                            closestObject = gameObject;
                        }
                    }
                    result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X2, colR.Y, colR.Y);
                    if (result.intersects)
                    {
                        if (result.point.Distance(sourcePosition) < closest)
                        {
                            closest = result.point.Distance(sourcePosition);
                            destinationPosition = result.point;
                            closestObject = gameObject;
                        }
                    }
                    result = LinesCollisions(srcR.X, srcR.X2, srcR.Y, srcR.Y2, colR.X, colR.X2, colR.Y2, colR.Y2);
                    if (result.intersects)
                    {
                        if (result.point.Distance(sourcePosition) < closest)
                        {
                            closest = result.point.Distance(sourcePosition);
                            destinationPosition = result.point;
                            closestObject = gameObject;
                        }
                    }
                }
            }

            if (draw) Line.Draw(new(sourcePosition.X - CameraX, sourcePosition.Y - CameraY), new(destinationPosition.X - CameraX, destinationPosition.Y - CameraY), 1f);

            if (closestObject is null)
            {
                if (closestTile is null)
                {
                    return (false, null, null);
                }
                return (true, null, closestTile);
            }
            return (true, closestObject, null);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {

            if (IsExiting)
            {
                return;
            }

            if (player is null)
                return;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            CameraX = player.X - 32;
            CameraY = player.Y - 32;

            if (CameraX < 0) CameraX = 0;
            if (CameraY < 0) CameraY = 0;

            if (CameraX + ViewportWidth > level.LevelWidth * 4) CameraX = level.LevelWidth * 4 - ViewportWidth;
            if (CameraY + ViewportWidth > level.LevelHeight * 4) CameraY = level.LevelHeight * 4 - ViewportHeight;

            Title = $"{args.Time:0.##} / {(1 / args.Time):0.##} ... {CameraX:0.##}-{CameraY:0.##}";

            gameRenderTexture.Begin();
            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            
            foreach (TileData tile in Background.Tiles)
            {
                Tile.Draw(tile.TileID, new Vector2i(tile.TileX * 4 - CameraX, tile.TileY * 4 - CameraY), tile.GetTileRotationRad());
            }
            
            foreach (TileData tile in Foreground.Tiles)
            {
                Tile.Draw(tile.TileID, new Vector2i(tile.TileX * 4 - CameraX, tile.TileY * 4 - CameraY), tile.GetTileRotationRad());
            }
            
            foreach (GameObject gameObject in this.gameObjects)
            {
                gameObject.Draw(args);
            }

            gameRenderTexture.End();

            rayCastRenderTexture.Begin();

            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            Vector2 playerDirection = ((Objects.Player)player).LookingAt;
            playerDirection = new Vector2(playerDirection.X - player.X, playerDirection.Y - player.Y);
            float originalAngle = playerDirection.GetAngle();

            TileMap tiles = new();
            foreach (TileData tile in Foreground.Tiles)
            {
                if (new Vector2(tile.TileX * 4, tile.TileY * 4).Distance(player.Location) < 36)
                {
                    tiles.Tiles.Add(tile);
                }
            }
            List<GameObject> gameObjects = new();
            foreach (GameObject go in this.gameObjects)
            {
                if (go.Location.Distance(player.Location) < 36)
                {
                    gameObjects.Add(go);
                }
            }

            List<TileData> collidedTiles = new List<TileData>();

            for (float i = -32; i <= 32; i++)
            {
                float angle = originalAngle + MathHelper.DegreesToRadians(((50 * i) / 32f));
                var result = CastRay(new Vector2(player.X + 2, player.Y + 2), angle, player, tiles, gameObjects, true);
                if (result.result && result.tile is not null)
                {
                    if (!collidedTiles.Contains(result.tile.Value))
                    {
                        collidedTiles.Add(result.tile.Value);
                    }
                }

            }
            rayCastRenderTexture.End();

            tempRenderTexture.Begin();

            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (!rayCastRenderTexture.Sprite.returnResult || rayCastRenderTexture.Sprite.spriteResult is null)
                return;
            if (!gameRenderTexture.Sprite.returnResult || gameRenderTexture.Sprite.spriteResult is null)
                return;

            TwoTextureSprite.DrawTexture(gameRenderTexture.Sprite.spriteResult.TextureID, rayCastRenderTexture.Sprite.spriteResult.TextureID, "Mask", new Rectanglei(0, 0, 64, 64), new Rectanglei(0, 0, 64, 64), 0, false, true);

            foreach (TileData tile in collidedTiles)
            {
                Tile.Draw(tile.TileID, new Vector2i(tile.TileX * 4 - CameraX, tile.TileY * 4 - CameraY), tile.GetTileRotationRad());
            }

            tempRenderTexture.End();

            memoryRenderTexture.Begin();

            if (!tempRenderTexture.Sprite.returnResult || tempRenderTexture.Sprite.spriteResult is null)
                return;

            Sprite.DrawTexture(tempRenderTexture.Sprite.spriteResult.TextureID, "BaseShader", new Rectanglei(0, 0, 64, 64), new Rectanglei(CameraX, CameraY - 48, 64, 64), 0, false, true);

            memoryRenderTexture.End();

            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (!memoryRenderTexture.Sprite.returnResult || memoryRenderTexture.Sprite.spriteResult is null)
                return;

            Sprite.DrawTexture(memoryRenderTexture.Sprite.spriteResult.TextureID, "Desaturated", new Rectanglei(CameraX, -(CameraY- 48), 64, 64), new Rectanglei(0, 0, 64, 64), 0, false, true);
            Sprite.DrawTexture(tempRenderTexture.Sprite.spriteResult.TextureID, "BaseShader", new Rectanglei(0, 0, 64, 64), new Rectanglei(0, 0, 64, 64), 0, false, true);
            player.Draw(args);

            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUnload()
        {
            ContentManager.RemoveAll();
            base.OnUnload();
        }

        public void ShowInventory(bool score = false, bool key = false, bool enemies = false)
        {
            //TODO
        }
    }
}
