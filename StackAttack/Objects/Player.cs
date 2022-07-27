using OpenTK.Windowing.Common;
using StackAttack.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Objects
{
    internal class Player : GameObject
    {
        public Player() : base(0,0,0,Headings.North)
        {
        }

        public Player(int x, int y, float rotation, Headings heading, string spriteID) : base(x, y, rotation, heading)
        {
            SpriteID = spriteID;
        }

        public override GameObject CreateNew(int x, int y, float rotation, Headings heading)
        {
            //throw new NotImplementedException();
            return new Player(x, y, rotation, heading, "");
        }

        public override void Draw(FrameEventArgs args)
        {
            (bool returnState, Sprite? returnSprite) = ContentManager.Get<Sprite>(SpriteID);
            if (returnState == true && returnSprite is not null)
            {
                returnSprite.Draw(new OpenTK.Mathematics.Vector2i(X, Y), (float)((int)Heading * (Math.PI / 2)));
            }
        }

        public override void Update(FrameEventArgs args)
        {
            //throw new NotImplementedException();
        }
    }
}
