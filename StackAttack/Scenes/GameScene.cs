using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using StackAttack.Engine;
using StackAttack.Engine.Helpers;
using StackAttack.Engine.Map;
using StackAttack.Objects;

namespace StackAttack.Scenes
{
    internal class GameScene : Scene
    {
        public int CameraX { get; set; } = 0;
        public int CameraY { get; set; } = 0;
        public LevelData Level = new();
        public List<GameObject> GameObjects { get; set; } = new();
        public GameObject? Player { get; set; }
        public TileMap Background { get; set; } = new();
        public TileMap Foreground { get; set; } = new();
        private int _hp = 100;
        public int HP { get { return _hp; } set { _hp = value; ShowHealthBar(); } }
        public int EnemiesLeft { get; set; } = 0;
        public int Ammo { get; set; } = 0;
        public int Score { get; set; } = 0;
        (int state, int timer) UIKey;
        (int state, int timer) UIScore;
        (int state, int timer) UIEnemy;
        (int state, int timer) UIHealth;
        (int state, int timer) UIAmmo;
        (int state, int timer) UIText;
        const int UIVisibleTimerDefault = 300;
        int UIVisibleTimer = 0;

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

            ShowHealthBar();
            ShowInventory(true, true, true, true);
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

            if (Parent.KeyboardState.IsKeyPressed(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Tab) || Parent.MouseState.IsButtonPressed(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Button2))
            {
                ShowHealthBar();
                ShowInventory(true, true, true, true);
            }

#if DEBUG
            if (Parent.KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadAdd))
            {
                HP++;
                if (HP > 100) HP = 100;
            }
            if (Parent.KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.KeyPadSubtract))
            {
                HP--;
                if (HP < 0) HP = 0;
            }
#endif

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

            //Blood

            //Enemies Do Anim

            if (UIVisibleTimer > 0)
            {
                UIVisibleTimer--;
            }

            string ToDraw = "$:" + Ammo.ToString();
            int x = Game.ViewportHeight - 2 - ToDraw.Length * 5;
            int y = Game.ViewportHeight - 14;
            if (UIAmmo.state > 0)
            {
                if (UIAmmo.state == 1)
                {
                    UIAmmo.timer++;
                    x = Game.ViewportHeight - 2 - ToDraw.Length * 5 + (int)(ToDraw.Length * 7 * ((20 - UIAmmo.timer) / 20f));
                    if (UIAmmo.timer >= 20)
                    {
                        UIEnemy.timer = 20;
                        UIAmmo.state = 2;
                        UIVisibleTimer = UIVisibleTimerDefault;
                    }
                }
                if (UIAmmo.state == 2)
                {
                    x = Game.ViewportHeight - 2 - ToDraw.Length * 5;
                    if (UIVisibleTimer <= 0)
                    {
                        UIAmmo.state = 3;
                    }
                }
                if (UIAmmo.state == 3)
                {
                    UIAmmo.timer--;
                    x = Game.ViewportHeight - 2 - ToDraw.Length * 5 + (int)(ToDraw.Length * 7 * ((20 - UIAmmo.timer) / 20f));
                    if (UIAmmo.timer <= 0)
                    {
                        UIEnemy.timer = 0;
                        UIAmmo.state = 0;
                    }
                }

                DrawText(ToDraw, x, y);

            }


            ToDraw = "@:" + EnemiesLeft.ToString();
            x = Game.ViewportHeight - 2 - ToDraw.Length * 5;
            y = Game.ViewportHeight - 7;
            if (UIEnemy.state > 0)
            {
                if (UIEnemy.state == 1)
                {
                    UIEnemy.timer++;
                    y = Game.ViewportHeight - 7 + (int)(7 * ((20-UIEnemy.timer) / 20f));
                    if (UIEnemy.timer >= 20)
                    {
                        UIEnemy.timer = 20;
                        UIEnemy.state = 2;
                        UIVisibleTimer = UIVisibleTimerDefault;
                    }
                }
                if (UIEnemy.state == 2)
                {
                    y = Game.ViewportHeight - 7;
                    if (UIVisibleTimer == 0)
                    {
                        UIEnemy.timer = 20;
                        UIEnemy.state = 3;
                    }
                }
                if (UIEnemy.state == 3)
                {
                    UIEnemy.timer--;
                    y = Game.ViewportHeight - 7 + (int)(7 * ((20 - UIEnemy.timer) / 20f));
                    if (UIEnemy.timer <= 0)
                    {
                        UIEnemy.timer = 0;
                        UIEnemy.state = 0;
                    }
                }

                DrawText(ToDraw, x, y);

            }

            if (UIKey.state > 0)
            {
                if (((Player)Player).HasKey() || true)
                {
                    x = 2;
                    y = Game.ViewportHeight - 14;
                    if (UIKey.state == 1)
                    {
                        UIKey.timer++;
                        x = -5 + (int)(7 * (UIKey.timer / 20f));
                        if (UIKey.timer >= 20)
                        {
                            UIKey.timer = 20;
                            UIKey.state = 2;
                            UIVisibleTimer = UIVisibleTimerDefault;
                        }
                    }
                    if (UIKey.state == 2)
                    {
                        x = 2;
                        if (UIVisibleTimer == 0)
                        {
                            UIKey.timer = 20;
                            UIKey.state = 3;
                        }
                    }
                    if (UIKey.state == 3)
                    {
                        UIKey.timer--;
                        x = -5 + (int)(7 * (UIKey.timer / 20f));
                        if (UIKey.timer <= 0)
                        {
                            UIKey.timer = 0;
                            UIKey.state = 0;
                        }
                    }
                    Sprite.Draw("FontKey", new Rectanglei(0, 0, 5, 5), new Rectanglei(x, y, 5, 5));
                }
            }

            //Score Do Anim
            if (UIScore.state > 0)
            {
                x = 2;
                y = Game.ViewportHeight - 7;
                ToDraw = "&:" + Score.ToString();

                if (UIScore.state == 1)
                {
                    UIScore.timer++;
                    y = Game.ViewportHeight - 7 + (int)(7 * ((20 - UIScore.timer) / 20f));
                    if (UIScore.timer >= 20)
                    {
                        UIScore.timer = 20;
                        UIScore.state = 2;
                        UIVisibleTimer = UIVisibleTimerDefault;
                    }
                }
                if (UIScore.state == 2)
                {
                    y = Game.ViewportHeight - 7;
                    if (UIVisibleTimer == 0)
                    {
                        UIScore.timer = 20;
                        UIScore.state = 3;
                    }
                }
                if (UIScore.state == 3)
                {
                    UIScore.timer--;
                    y = Game.ViewportHeight - 7 + (int)(7 * ((20 - UIScore.timer) / 20f));
                    if (UIScore.timer <= 0)
                    {
                        UIScore.timer = 0;
                        UIScore.state = 0;
                    }
                }
                DrawText(ToDraw, x, y);
            }


            if (UIHealth.state != 0)
            {
                x = Game.ViewportWidth / 2 - 10;
                y = 2;
                if (UIHealth.state == 1)
                {
                    UIHealth.timer++;
                    y = -5 + (int)(7 * (UIHealth.timer / 20f));
                    if (UIHealth.timer >= 20)
                    {
                        UIHealth.timer = 20;
                        UIHealth.state = 2;
                        UIHealth.timer = UIVisibleTimerDefault;
                    }
                }
                if (UIHealth.state == 2)
                {
                    y = 2;
                    UIHealth.timer--;
                    if (UIHealth.timer <= 0)
                    {
                        UIHealth.timer = 0;
                        HideHealthBar();
                    }
                }
                if (UIHealth.state == 3)
                {
                    UIHealth.timer--;
                    y = -5 + (int)(7 * (UIHealth.timer / 20f));
                    if (UIHealth.timer <= 0)
                    {
                        UIHealth.timer = 0;
                        UIHealth.state = 0;
                    }
                }

                Sprite.Draw("Health1Off", new Rectanglei(0, 0, 5, 5), new Rectanglei(x, y, 5, 5));
                Sprite.Draw("Health2Off", new Rectanglei(0, 0, 5, 5), new Rectanglei(x + 5, y, 5, 5));
                Sprite.Draw("Health2Off", new Rectanglei(0, 0, 5, 5), new Rectanglei(x + 10, y, 5, 5));
                Sprite.Draw("Health3Off", new Rectanglei(0, 0, 5, 5), new Rectanglei(x + 15, y, 5, 5));

                if (HP > 75)
                {
                    float HPP = (HP - 75) / 25f;
                    Sprite.Draw("Health3On", new Rectanglei(0, 0, (int)(5 * HPP), 5), new Rectanglei(x + 15, y, (int)(5 * HPP), 5));
                    Sprite.Draw("Health2On", new Rectanglei(0, 0, 5, 5), new Rectanglei(x + 10, y, 5, 5));
                    Sprite.Draw("Health2On", new Rectanglei(0, 0, 5, 5), new Rectanglei(x + 5, y, 5, 5));
                    Sprite.Draw("Health1On", new Rectanglei(0, 0, 5, 5), new Rectanglei(x, y, 5, 5));
                }
                else if (HP > 50)
                {
                    float HPP = (HP - 50) / 25f;
                    Sprite.Draw("Health2On", new Rectanglei(0, 0, (int)(5 * HPP), 5), new Rectanglei(x + 10, y, (int)(5 * HPP), 5));
                    Sprite.Draw("Health2On", new Rectanglei(0, 0, 5, 5), new Rectanglei(x + 5, y, 5, 5));
                    Sprite.Draw("Health1On", new Rectanglei(0, 0, 5, 5), new Rectanglei(x, y, 5, 5));
                }
                else if (HP > 25)
                {
                    float HPP = (HP - 25) / 25f;
                    Sprite.Draw("Health2On", new Rectanglei(0, 0, (int)(5 * HPP), 5), new Rectanglei(x + 5, y, (int)(5 * HPP), 5));
                    Sprite.Draw("Health1On", new Rectanglei(0, 0, 5, 5), new Rectanglei(x, y, 5, 5));
                }
                else if (HP > 0)
                {
                    float HPP = HP / 25f;
                    Sprite.Draw("Health1On", new Rectanglei(0, 0, (int)(5 * HPP), 5), new Rectanglei(x, y, (int)(5 * HPP), 5));
                }
            }
        }

        private void DrawText(string text, int x, int y)
        {
            text = text.ToUpper();
            int rx = 0;
            int ry = 0;
            for (int i = 0; i < text.Length; i++)
            {
                string fontID = "";
                if (text[i] == '@')
                {
                    fontID = "FontEnemy";
                }
                else if (text[i] == '$')
                {
                    fontID = "FontAmmo";
                }
                else if (text[i] == '#')
                {
                    fontID = "FontKey";
                }
                else if (text[i] == '&')
                {
                    fontID = "FontChest";
                }
                else if ((text[i] >= 'A' && text[i] <= 'Z') || (text[i] >= '0' && text[i] <= '9') || (text[i] == ':') ||
                    (text[i] == '.') || (text[i] == ',') || (text[i] == '!') || (text[i] == '?') || (text[i] == '-') ||
                    (text[i] == '='))
                {
                    fontID = "Font" + text[i];
                }
                if (fontID != "")
                {
                    Sprite.Draw(fontID, new Rectanglei(0, 0, 5, 5), new Rectanglei(x + rx * 5, y + ry * 7, 5, 5));
                }
                rx++;
                if ((rx > 11) || (text[i] == '\n'))
                {
                    rx = 0;
                    ry++;
                }
            }
        }

        public void ShowInventory(bool score, bool key, bool enemies, bool ammo)
        {
            if (score)
            {
                SetUIStateTimer(ref UIScore);
            }
            if (enemies)
            {
                EnemiesLeft = GameObjects.Where(t => t.GetType() == typeof(Enemy)).Count();
                SetUIStateTimer(ref UIEnemy);
            }
            if (key)
            {
                SetUIStateTimer(ref UIKey);
            }
            if (ammo)
            {
                SetUIStateTimer(ref UIAmmo);
            }
        }

        public void HideInventory()
        {
            if (UIKey.state != 0) UIKey.state = 3;
            if (UIScore.state != 0) UIScore.state = 3;
            if (UIEnemy.state != 0) UIEnemy.state = 3;
            if (UIAmmo.state != 0) UIAmmo.state = 3;
        }

        private void SetUIStateTimer(ref (int state, int timer) statetimer)
        {
            if (statetimer.state != 2 && statetimer.state != 1)
            {
                statetimer.state = 1;
                UIVisibleTimer = UIVisibleTimerDefault;
                statetimer.timer = 0;
            }
            else
            {
                UIVisibleTimer = UIVisibleTimerDefault;
            }
        }

        public void ShowHealthBar()
        {
            if (UIHealth.state != 2 && UIHealth.state != 1)
            {
                UIHealth.state = 1;
                UIHealth.timer = 0;
            }
            else if (UIHealth.state == 2)
            {
                UIHealth.timer = UIVisibleTimerDefault;
            }
        }

        public void HideHealthBar(bool overrider = false)
        {
            if (HP > 25 || overrider)
            {
                UIHealth.state = 3;
                UIHealth.timer = 20;
            }
            else
            {
                UIHealth.state = 2;
                UIHealth.timer = UIVisibleTimerDefault;
            }
        }

        public void ShowHitAnimation()
        {

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
