using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using StackAttack.Engine;
using StackAttack.Engine.Helpers;
using StackAttack.Engine.Map;

namespace StackAttack.Scenes
{
    internal class GameScene : Scene
    {
        public int CameraX { get; set; } = 0;
        public int CameraY { get; set; } = 0;
        public int Score { get; set; } = 0;
        public LevelData level = new();
        public List<GameObject> gameObjects { get; set; } = new();
        public GameObject? player { get; set; }
        public TileMap Background { get; set; } = new();
        public TileMap Foreground { get; set; } = new();

        List<Shader.ShaderDefinition> shaderDefinitions = new();
        List<Texture.TextureDefinition> textureDefinitions = new();
        List<Sprite.SpriteDefinition> spriteDefinitions = new();
        List<Tile.TileDefinition> tileDefinitions = new();
        RenderTexture rayCastRenderTexture = new();
        RenderTexture gameRenderTexture = new();
        RenderTexture tempRenderTexture = new();
        RenderTexture memoryRenderTexture = new();

        public GameScene(Game parent) : base(parent)
        {
            Game.LoadDefinitionData("Shaders/shaderDefinitions.json", ref shaderDefinitions);
            Game.LoadDefinitionData("Textures/textureDefinitions.json", ref textureDefinitions);
            Game.LoadDefinitionData("Textures/spriteDefinitions.json", ref spriteDefinitions);
            Game.LoadDefinitionData("Textures/tileDefinitions.json", ref tileDefinitions);
            Game.LoadDefinitionData("Levels/AlphaLevel.json", ref level);
            LoadDefinitions();

            foreach (GameObjectStartData objectData in level.GameObjectStartDatas)
            {
                (bool returnResult, GameObject? returnObject) = ContentManager.Get<GameObject>(objectData.GameObjectTypeID);
                if (returnResult == true && returnObject is not null)
                {
                    gameObjects.Add(returnObject.CreateNew(objectData.ObjectX, objectData.ObjectY, 0, objectData.Heading, Parent, objectData.SpriteID));
                }
            }
            player = new Objects.Player(level.PlayerStartData.PlayerX, level.PlayerStartData.PlayerY, 0, level.PlayerStartData.Heading, Parent, level.PlayerStartData.SpriteID);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            gameRenderTexture = new RenderTexture(64, 64, "BaseShader", "");
            tempRenderTexture = new RenderTexture(64, 64, "BaseShader", "");
            memoryRenderTexture = new RenderTexture(level.LevelWidth * 4, level.LevelHeight * 4, "Desaturated", "");
            rayCastRenderTexture = new RenderTexture(64, 64, "BaseShader", "");

            parent.CursorState = CursorState.Hidden;
        }

        public void Unload()
        {

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

            IEnumerable<GameObject> gameObjectTypes = ExtensionsAndHelpers.GetAllInherited<GameObject>();
            foreach (GameObject gameObjectType in gameObjectTypes)
            {
                ContentManager.Add<GameObject>(gameObjectType.GetType().Name, gameObjectType);
            }
            Background = level.Background.Clone();
            Foreground = level.Foreground.Clone();
        }

        public override void Update(FrameEventArgs args)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Update(args);
            }

            if (player is null)
                return;

            player.Update(args);
        }

        public override void Draw(FrameEventArgs args)
        {
            if (player is null)
                return;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            CameraX = player.X - 32;
            CameraY = player.Y - 32;

            if (CameraX < 0) CameraX = 0;
            if (CameraY < 0) CameraY = 0;

            if (CameraX + Game.ViewportWidth > level.LevelWidth * 4) CameraX = level.LevelWidth * 4 - Game.ViewportWidth;
            if (CameraY + Game.ViewportWidth > level.LevelHeight * 4) CameraY = level.LevelHeight * 4 - Game.ViewportHeight;

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

            for (float i = -64; i <= 64; i++)
            {
                float angle = originalAngle + MathHelper.DegreesToRadians(((60 * i) / 64f));
                var result = Game.CastRay(new Vector2(player.X + 2, player.Y + 2), angle, player, tiles, gameObjects, true, CameraX, CameraY);
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

            Sprite.DrawTexture(gameRenderTexture.Sprite.spriteResult.TextureID, "SuperDesaturated", new Rectanglei(0, 0, 64, 64), new Rectanglei(0, 0, 64, 64), 0, false, true);
            Sprite.DrawTexture(memoryRenderTexture.Sprite.spriteResult.TextureID, "Desaturated", new Rectanglei(CameraX, -(CameraY - 48), 64, 64), new Rectanglei(0, 0, 64, 64), 0, false, true);
            Sprite.DrawTexture(tempRenderTexture.Sprite.spriteResult.TextureID, "BaseShader", new Rectanglei(0, 0, 64, 64), new Rectanglei(0, 0, 64, 64), 0, false, true);
            player.Draw(args);
        }

        public void ShowInventory(bool score = false, bool key = false, bool enemies = false)
        {
            //TODO
        }

        public override void Dispose()
        {
            shaderDefinitions.Clear();
            textureDefinitions.Clear();
            spriteDefinitions.Clear();
            gameObjects.Clear();
            //level.Dispose();
            ContentManager.RemoveAll();
        }
    }
}
