using OpenTK.Windowing.Common;
using StackAttack.Engine;
using StackAttack.Engine.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Objects
{
    internal class GoldDoor : GameObject
    {
        private int DefaultTimer = 0;
        private int Timer = 100;
        public bool IsOpen = false;
        private TileData tile;

        public GoldDoor() : base(0, 0, 0, Headings.North, null, "")
        {
            Init();
        }

        public GoldDoor(int x, int y, float rotation, Headings heading, Game? game, string spriteID) : base(x, y, rotation, heading, game, spriteID)
        {
            Init();
        }

        private void Init()
        {
            Close();
            Timer = Random.Shared.Next(50, 200);
            DefaultTimer = Timer;
        }

        public override GameObject CreateNew(int x, int y, float rotation, Headings heading, Game? game, string spriteID)
        {
            return new BlueDoor(x, y, rotation, heading, game, spriteID);
        }

        public override void Draw(FrameEventArgs args)
        {
            if (IsOpen)
            {
                (bool returnState, Sprite? returnSprite) = ContentManager.Get<Sprite>(SpriteID);
                if (returnState == true && returnSprite is not null)
                {
                    returnSprite.Draw(new OpenTK.Mathematics.Vector2i(X, Y), (float)((int)Heading * (Math.PI / 2)));
                }
            }
        }

        public override void Update(FrameEventArgs args)
        {
            Timer--;
            if (Timer == 0)
            {
                Timer = DefaultTimer;
                if (IsOpen)
                {
                    Close();
                }
                else
                {
                    Open();
                }
            }
        }

        public void Open()
        {
            if (Parent is null)
                return;

            Parent.Foreground.Tiles.Remove(tile);
            IsOpen = true;
        }

        public void Close()
        {
            if (Parent is null)
                return;

            tile = new Engine.Map.TileData(SpriteID, X / 4, Y / 4, (float)((int)Heading * 90));
            Parent.Foreground.Tiles.Add(tile);
            IsOpen = false;
        }
    }
}
