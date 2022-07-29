using OpenTK.Windowing.Common;
using StackAttack.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Objects
{
    internal class Key : GameObject
    {
        public Key() : base(0,0,0,Headings.North, null, "")
        {
        }

        public Key(int x, int y, float rotation, Headings heading, Game? game, string spriteID) : base(x, y, rotation, heading, game,spriteID)
        {
        }

        public override GameObject CreateNew(int x, int y, float rotation, Headings heading, Game? game, string spriteID)
        {
            return new Key(x, y, rotation, heading, game, spriteID);
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
            if (scene.player is null)
                return;

            (bool returnState, Sprite? returnSprite) = ContentManager.Get<Sprite>(SpriteID);
            if (returnState == true && returnSprite is not null)
            {
                returnSprite.Draw(new OpenTK.Mathematics.Vector2i(X - scene.CameraX, Y - scene.CameraY), (float)((int)Heading * (Math.PI / 2)));
            }
        }

        public override void Update(FrameEventArgs args)
        {
            //throw new NotImplementedException();
        }
    }
}
