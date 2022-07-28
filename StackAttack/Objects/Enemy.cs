using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using StackAttack.Engine;
using StackAttack.Engine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Objects
{
    internal class Enemy : GameObject
    {
        private enum Actions { MoveToPosition, LookAround, Engage, Strafe}

        public Vector2i LookingAt = new(0, 0);
        public Vector2i Target = new(0, 0);
        float speed = 0.25f;
        float tempX;
        float tempY;
        int timer = 20;
        int stater = 0;
        Actions action = Actions.LookAround;
        string AngledSpriteID = "";

        public Enemy() : base(0,0,0,Headings.North, null, "")
        {
            Init();
        }

        public Enemy(int x, int y, float rotation, Headings heading, Game? game, string spriteID) : base(x, y, rotation, heading, game, spriteID)
        {
            Init();
        }

        public void Init()
        {
            tempX = X;
            tempY = Y;
            AngledSpriteID = SpriteID + "Angle";
            switch (Heading)
            {
                case Headings.North: LookingAt = new(X, 0); break;
                case Headings.South: LookingAt = new(X, Game.ViewportHeight); break;
                case Headings.East: LookingAt = new(Game.ViewportWidth, Y); break;
                case Headings.West: LookingAt = new(0, Y); break;
            }
        }

        public override GameObject CreateNew(int x, int y, float rotation, Headings heading, Game? game, string spriteID)
        {
           return new Enemy(x, y, rotation, heading, game, spriteID);
        }

        public override void Draw(FrameEventArgs args)
        {
            if (Parent is null)
                return;

            int renderX = X - Parent.CameraX;
            int renderY = Y - Parent.CameraY;

            (bool returnState, Sprite? enemySprite) = ContentManager.Get<Sprite>(SpriteID);
            if (returnState == false || enemySprite is null)
                return;

            (returnState, Sprite? enemySpriteAngle) = ContentManager.Get<Sprite>(AngledSpriteID);
            if (returnState == false || enemySpriteAngle is null)
                return;

            float angle = new Vector2i(LookingAt.X - X, LookingAt.Y - Y).GetAngle() + (float)(Math.PI / 2);
            angle %= (float)Math.Tau;

            if (Parent is null)
                return;

            if (angle > Math.PI * 0.125f && angle < Math.PI * 0.375f)
            {
                enemySpriteAngle.Draw(new OpenTK.Mathematics.Vector2i(renderX, renderY), 0);
            }
            else if (angle > Math.PI * 0.625f && angle < Math.PI * 0.875f)
            {
                enemySpriteAngle.Draw(new OpenTK.Mathematics.Vector2i(renderX, renderY), (float)(Math.PI / 2));
            }
            else if (angle > Math.PI * 1.125f && angle < Math.PI * 1.375f)
            {
                enemySpriteAngle.Draw(new OpenTK.Mathematics.Vector2i(renderX, renderY), (float)Math.PI);
            }
            else if (angle > Math.PI * 1.625f && angle < Math.PI * 1.875f)
            {
                enemySpriteAngle.Draw(new OpenTK.Mathematics.Vector2i(renderX, renderY), (float)(1.5f * Math.PI));
            }
            else
            {
                enemySprite.Draw(new OpenTK.Mathematics.Vector2i(renderX, renderY), angle);
            }

        }

        public override void Update(FrameEventArgs args)
        {
            timer--;
            if (timer <= 0)
            {
                switch (action)
                {
                    case Actions.LookAround:
                        //TOOD: FOV
                        if (false)
                        {
                            //If sees player
                            Target.X = Random.Shared.Next(0, 0);
                            Target.Y = Random.Shared.Next(0, 0); //LookAtPlayer
                            //Remember player
                            //Engage
                        }
                        else if (false)
                        {
                            //Memory
                            Target.X = Random.Shared.Next(0, 0);
                            Target.Y = Random.Shared.Next(0, 0); //LookAtMemory
                            action = Actions.MoveToPosition;
                        }
                        else
                        {
                            LookingAt.X = Random.Shared.Next(0, 64);
                            LookingAt.Y = Random.Shared.Next(0, 64);
                            stater++;
                            if (stater < 8)
                            {
                                timer = 20;
                            }
                            else
                            {
                                stater = 0;
                                action = Actions.MoveToPosition;
                                Target.X = LookingAt.X;
                                Target.Y = LookingAt.Y;
                            }
                            break;
                        }
                    case Actions.MoveToPosition:
                        //TODO A*
                        X = Target.X;
                        Y = Target.Y;
                        //Rotate in move direction
                        if (false)
                        {
                            //Check if sees player, and enter the engage mode

                        }
                        action = Actions.LookAround;
                        timer = 20;
                        break;
                }
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is Enemy enemy &&
                   base.Equals(obj) &&
                   X == enemy.X &&
                   Y == enemy.Y &&
                   Rotation == enemy.Rotation &&
                   Heading == enemy.Heading &&
                   SpriteID == enemy.SpriteID &&
                   LookingAt.Equals(enemy.LookingAt) &&
                   Target.Equals(enemy.Target) &&
                   speed == enemy.speed &&
                   tempX == enemy.tempX &&
                   tempY == enemy.tempY &&
                   timer == enemy.timer &&
                   stater == enemy.stater &&
                   action == enemy.action &&
                   AngledSpriteID == enemy.AngledSpriteID;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(base.GetHashCode());
            hash.Add(X);
            hash.Add(Y);
            hash.Add(Rotation);
            hash.Add(Heading);
            hash.Add(SpriteID);
            hash.Add(LookingAt);
            hash.Add(Target);
            hash.Add(speed);
            hash.Add(tempX);
            hash.Add(tempY);
            hash.Add(timer);
            hash.Add(stater);
            hash.Add(action);
            hash.Add(AngledSpriteID);
            return hash.ToHashCode();
        }
    }
}
