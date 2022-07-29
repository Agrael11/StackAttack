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
        public LevelData Level = new();
        public List<GameObject> GameObjects { get; set; } = new();
        public GameObject? Player { get; set; }
        public TileMap Background { get; set; } = new();
        public TileMap Foreground { get; set; } = new();

        private readonly List<Shader.ShaderDefinition> _shaderDefinitions = new();
        private readonly List<Texture.TextureDefinition> _textureDefinitions = new();
        private readonly List<Sprite.SpriteDefinition> _spriteDefinitions = new();
        private readonly List<Tile.TileDefinition> _tileDefinitions = new();
        private readonly RenderTexture _rayCastRenderTexture = new();
        private readonly RenderTexture _gameRenderTexture = new();
        private readonly RenderTexture _tempRenderTexture = new();
        private readonly RenderTexture _memoryRenderTexture = new();

        public GameScene(Game parent) : base(parent)
        {
            Game.LoadDefinitionData("Shaders/shaderDefinitions.json", ref _shaderDefinitions);
            Game.LoadDefinitionData("Textures/textureDefinitions.json", ref _textureDefinitions);
            Game.LoadDefinitionData("Textures/spriteDefinitions.json", ref _spriteDefinitions);
            Game.LoadDefinitionData("Textures/tileDefinitions.json", ref _tileDefinitions);
            Game.LoadDefinitionData("Levels/AlphaLevel.json", ref Level);
            LoadDefinitions();

            foreach (GameObjectStartData objectData in Level.GameObjectStartDatas)
            {
                (bool returnResult, GameObject? returnObject) = ContentManager.Get<GameObject>(objectData.GameObjectTypeID);
                if (returnResult == true && returnObject is not null)
                {
                    GameObjects.Add(returnObject.CreateNew(objectData.ObjectX, objectData.ObjectY, 0, objectData.Heading, Parent, objectData.SpriteID));
                }
            }
            Player = new Objects.Player(Level.PlayerStartData.PlayerX, Level.PlayerStartData.PlayerY, 0, Level.PlayerStartData.Heading, Parent, Level.PlayerStartData.SpriteID);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            _gameRenderTexture = new RenderTexture(64, 64, "BaseShader", "");
            _tempRenderTexture = new RenderTexture(64, 64, "BaseShader", "");
            _memoryRenderTexture = new RenderTexture(Level.LevelWidth * 4, Level.LevelHeight * 4, "Desaturated", "");
            _rayCastRenderTexture = new RenderTexture(64, 64, "BaseShader", "");

            parent.CursorState = CursorState.Hidden;
        }

        private void LoadDefinitions()
        {
            foreach (var shaderDefinition in _shaderDefinitions)
            {
                ContentManager.Load<Shader>(shaderDefinition.ShaderID, shaderDefinition.FileName);
            }

            foreach (var textureDefinition in _textureDefinitions)
            {
                ContentManager.Load<Texture>(textureDefinition.TextureID, textureDefinition.FileName);
            }

            foreach (var tileDefinition in _tileDefinitions)
            {
                ContentManager.Add(tileDefinition.TileID, new Tile(tileDefinition.TextureID, tileDefinition.ShaderID, new Vector2i(tileDefinition.X, tileDefinition.Y), new Vector2i(tileDefinition.Width, tileDefinition.Height)));
            }

            foreach (var spriteDefinition in _spriteDefinitions)
            {
                ContentManager.Add(spriteDefinition.SpriteID, new Sprite(spriteDefinition.TextureID, spriteDefinition.ShaderID, new Vector2i(spriteDefinition.X, spriteDefinition.Y), new Vector2i(spriteDefinition.Width, spriteDefinition.Height)));
            }

            IEnumerable<GameObject> gameObjectTypes = ExtensionsAndHelpers.GetAllInherited<GameObject>();
            foreach (GameObject gameObjectType in gameObjectTypes)
            {
                ContentManager.Add<GameObject>(gameObjectType.GetType().Name, gameObjectType);
            }
            Background = Level.Background.Clone();
            Foreground = Level.Foreground.Clone();
        }

        public override void Update(FrameEventArgs args)
        {
            foreach (GameObject gameObject in GameObjects)
            {
                gameObject.Update(args);
            }

            if (Player is null)
                return;

            Player.Update(args);
        }

        public override void Draw(FrameEventArgs args)
        {
            if (Player is null)
                return;

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            CameraX = Player.X - 32;
            CameraY = Player.Y - 32;

            if (CameraX < 0) CameraX = 0;
            if (CameraY < 0) CameraY = 0;

            if (CameraX + Game.ViewportWidth > Level.LevelWidth * 4) CameraX = Level.LevelWidth * 4 - Game.ViewportWidth;
            if (CameraY + Game.ViewportWidth > Level.LevelHeight * 4) CameraY = Level.LevelHeight * 4 - Game.ViewportHeight;

            _gameRenderTexture.Begin();
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

            foreach (GameObject gameObject in this.GameObjects)
            {
                gameObject.Draw(args);
            }

            _gameRenderTexture.End();

            _rayCastRenderTexture.Begin();

            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            Vector2 playerDirection = ((Objects.Player)Player).LookingAt;
            playerDirection = new Vector2(playerDirection.X - Player.X, playerDirection.Y - Player.Y);
            float originalAngle = playerDirection.GetAngle();

            TileMap tiles = new();
            foreach (TileData tile in Foreground.Tiles)
            {
                if (new Vector2(tile.TileX * 4, tile.TileY * 4).Distance(Player.Location) < 36)
                {
                    tiles.Tiles.Add(tile);
                }
            }

            List<GameObject> gameObjects = new();
            foreach (GameObject go in this.GameObjects)
            {
                if (go.Location.Distance(Player.Location) < 36)
                {
                    gameObjects.Add(go);
                }
            }

            List<TileData> collidedTiles = new();

            for (float i = -64; i <= 64; i++)
            {
                float angle = originalAngle + MathHelper.DegreesToRadians(((60 * i) / 64f));
                var result = RayCasting.CastRay(new Vector2(Player.X + 2, Player.Y + 2), angle, Player, tiles, gameObjects, true, CameraX, CameraY);
                if (result.result && result.tile is not null)
                {
                    if (!collidedTiles.Contains(result.tile.Value))
                    {
                        collidedTiles.Add(result.tile.Value);
                    }
                }

            }
            _rayCastRenderTexture.End();

            _tempRenderTexture.Begin();

            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (!_rayCastRenderTexture.Sprite.returnResult || _rayCastRenderTexture.Sprite.spriteResult is null)
                return;
            if (!_gameRenderTexture.Sprite.returnResult || _gameRenderTexture.Sprite.spriteResult is null)
                return;

            TwoTextureSprite.DrawTexture(_gameRenderTexture.Sprite.spriteResult.TextureID, _rayCastRenderTexture.Sprite.spriteResult.TextureID, "Mask", new Rectanglei(0, 0, 64, 64), new Rectanglei(0, 0, 64, 64), 0, false, true);

            foreach (TileData tile in collidedTiles)
            {
                Tile.Draw(tile.TileID, new Vector2i(tile.TileX * 4 - CameraX, tile.TileY * 4 - CameraY), tile.GetTileRotationRad());
            }

            _tempRenderTexture.End();

            _memoryRenderTexture.Begin();

            if (!_tempRenderTexture.Sprite.returnResult || _tempRenderTexture.Sprite.spriteResult is null)
                return;

            Sprite.DrawTexture(_tempRenderTexture.Sprite.spriteResult.TextureID, "BaseShader", new Rectanglei(0, 0, 64, 64), new Rectanglei(CameraX, CameraY - 48, 64, 64), 0, false, true);

            _memoryRenderTexture.End();

            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (!_memoryRenderTexture.Sprite.returnResult || _memoryRenderTexture.Sprite.spriteResult is null)
                return;

            Sprite.DrawTexture(_gameRenderTexture.Sprite.spriteResult.TextureID, "SuperDesaturated", new Rectanglei(0, 0, 64, 64), new Rectanglei(0, 0, 64, 64), 0, false, true);
            Sprite.DrawTexture(_memoryRenderTexture.Sprite.spriteResult.TextureID, "Desaturated", new Rectanglei(CameraX, -(CameraY - 48), 64, 64), new Rectanglei(0, 0, 64, 64), 0, false, true);
            Sprite.DrawTexture(_tempRenderTexture.Sprite.spriteResult.TextureID, "BaseShader", new Rectanglei(0, 0, 64, 64), new Rectanglei(0, 0, 64, 64), 0, false, true);
            Player.Draw(args);
        }

        public void ShowInventory(bool score = false, bool key = false, bool enemies = false)
        {
            //TODO
        }

        public void Unload()
        {
            _shaderDefinitions.Clear();
            _textureDefinitions.Clear();
            _spriteDefinitions.Clear();
            GameObjects.Clear();
            //level.Dispose();
            ContentManager.RemoveAll();
        }

        public override void Dispose()
        {
            Unload();
        }
    }
}
