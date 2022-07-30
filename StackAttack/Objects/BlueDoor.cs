using OpenTK.Windowing.Common;
using StackAttack.Engine;
using StackAttack.Engine.Helpers;
using StackAttack.Engine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Objects
{
    internal class BlueDoor : GameObject
    {
        private int DefaultTimer = 100;
        private int Timer = 0;
        public bool IsOpen = true;
        private TileData tile;

        public BlueDoor() : base(0,0,0,Headings.North, null, "")
        {
            Init();
        }

        public BlueDoor(int x, int y, float rotation, Headings heading, Game? game, string spriteID) : base(x, y, rotation, heading, game, spriteID)
        {
            Init();
        }

        internal void Init()
        {
            TryClose();
        }

        public override GameObject CreateNew(int x, int y, float rotation, Headings heading, Game? game, string spriteID)
        {
            return new BlueDoor(x, y, rotation, heading, game, spriteID);
        }

        public override void Draw(FrameEventArgs args)
        {
            if (Parent is null)
                return;
            if (Parent.CurrentScene is null)
                return;
            if (Parent.CurrentScene.GetType() != typeof(Scenes.GameScene))
                return;
            Scenes.GameScene? scene = (Scenes.GameScene)Parent.CurrentScene;
            if (scene is null)
                return;
            if (scene.Player is null)
                return;

            if (IsOpen)
            {
                (bool returnState, Sprite? returnSprite) = ContentManager.Get<Sprite>(SpriteID);
                if (returnState == true && returnSprite is not null)
                {
                    returnSprite.Draw(new OpenTK.Mathematics.Vector2i(X - scene.CameraX, Y - scene.CameraY), (float)((int)Heading * (Math.PI / 2)));
                }
            }
        }

        public override void Update(FrameEventArgs args)
        {
            if (Timer == 0)
            {
                if (IsOpen)
                {
                    if (TryClose())
                    {
                        Timer--;
                    }
                    else
                    {
                        Timer = DefaultTimer/10;
                    }
                }
            }
            else if (Timer > 0)
            {
                Timer--;
            }
        }

        public void Open()
        {
            if (Parent is null)
                return;
            if (Parent.CurrentScene is null)
                return;
            if (Parent.CurrentScene.GetType() != typeof(Scenes.GameScene))
                return;
            Scenes.GameScene? scene = (Scenes.GameScene)Parent.CurrentScene;
            if (scene is null)
                return;
            if (scene.Player is null)
                return;

            if (!IsOpen)
            {
                var result = ContentManager.Get<Sound>("Door");

                if (result.returnStatus && result.returnObject is not null)
                {
                    result.returnObject.UseSound();
                }
            }

            scene.Foreground.Tiles.Remove(tile);
            IsOpen = true;
            Timer = DefaultTimer;

        }

        public bool TryClose()
        {
            if (Parent is null)
                return false;
            if (Parent.CurrentScene is null)
                return false;
            if (Parent.CurrentScene.GetType() != typeof(Scenes.GameScene))
                return false;
            Scenes.GameScene? scene = (Scenes.GameScene)Parent.CurrentScene;
            if (scene is null)
                return false;
            if (scene.Player is null)
                return false;
            if (scene.Player is null)
                return false;

            (bool result, Sprite? meSprite) = ContentManager.Get<Sprite>(scene.Player.SpriteID);
            if (!result || meSprite is null)
                return false;

            (result, Sprite? playerSprite) = ContentManager.Get<Sprite>(scene.Player.SpriteID);
            if (!result || playerSprite is null)
                return false;

            if (new Rectanglei(scene.Player.Location, playerSprite.Size).Intersects(new Rectanglei(Location, meSprite.Size)))
            {
                return false;
            }

            foreach (GameObject gameObject in scene.GameObjects)
            {
                if (gameObject == this)
                    continue;

                (result, Sprite? objectSprite) = ContentManager.Get<Sprite>(scene.Player.SpriteID);
                if (!result || objectSprite is null)
                    return false;

                if (new Rectanglei(gameObject.Location, objectSprite.Size).Intersects(new Rectanglei(Location, meSprite.Size)))
                {
                    return false;
                }
            }

            tile = new Engine.Map.TileData(SpriteID, X/4, Y/4, (float)((int)Heading * 90));
            scene.Foreground.Tiles.Add(tile);
            IsOpen = false;
            return true;
        }
    }
}
