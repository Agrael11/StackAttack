using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
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
    internal class Player : GameObject
    {
        public Vector2i LookingAt = new(0,0);
        float speed = 0.25f;
        float tempX;
        float tempY;
        bool hasKey = false;
        string AngledSpriteID = "";
        string TargetSpriteID = "";

        public Player() : base(0,0,0,Headings.North, null, "")
        {
            Init();
        }

        public Player(int x, int y, float rotation, Headings heading, Game? game, string spriteID) : base(x, y, rotation, heading, game, spriteID)
        {
            Init();
        }

        public void Init()
        {
            tempX = X;
            tempY = Y;
            AngledSpriteID = SpriteID + "Angle";
            TargetSpriteID = "Target";
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
            return new Player(x, y, rotation, heading, game, spriteID);
        }

        public override void Draw(FrameEventArgs args)
        {
            (bool returnState, Sprite? playerSprite) = ContentManager.Get<Sprite>(SpriteID);
            if (returnState == false || playerSprite is null)
                return;

            (returnState, Sprite? playerSpriteAngle) = ContentManager.Get<Sprite>(AngledSpriteID);
            if (returnState == false || playerSpriteAngle is null)
                return;

            (returnState, Sprite? targetSprite) = ContentManager.Get<Sprite>(TargetSpriteID);
            if (returnState == false || targetSprite is null)
                return;

            float angle = new Vector2i(LookingAt.X - X, LookingAt.Y - Y).GetAngle() + (float)(Math.PI/2);
            angle %= (float)Math.Tau;

            if (Parent is null)
                return;

            if (angle > Math.PI * 0.125f && angle < Math.PI * 0.375f)
            {
                playerSpriteAngle.Draw(new OpenTK.Mathematics.Vector2i(X, Y), 0);
            }
            else if (angle > Math.PI * 0.625f && angle < Math.PI * 0.875f)
            {
                playerSpriteAngle.Draw(new OpenTK.Mathematics.Vector2i(X, Y), (float)(Math.PI / 2));
            }
            else if (angle > Math.PI * 1.125f && angle < Math.PI * 1.375f)
            {
                playerSpriteAngle.Draw(new OpenTK.Mathematics.Vector2i(X, Y), (float)Math.PI);
            }
            else if (angle > Math.PI * 1.625f && angle < Math.PI * 1.875f)
            {
                playerSpriteAngle.Draw(new OpenTK.Mathematics.Vector2i(X, Y), (float)(1.5f * Math.PI));
            }
            else
            {
                playerSprite.Draw(new OpenTK.Mathematics.Vector2i(X, Y), angle);
            }

            targetSprite.Draw(new Vector2i(LookingAt.X-2, LookingAt.Y-2));

        }

        public override void Update(FrameEventArgs args)
        {
            if (Parent is null)
                return;

            KeyboardState input = Parent.KeyboardState.GetSnapshot();
            
            if (input.IsKeyPressed(Keys.Escape))
            {
                Parent.Close();
                return;
            }

            float oldTempY = tempY;
            float oldTempX = tempX;

            if (input.IsKeyDown(Keys.W))
            {
                tempY -= speed;
            }

            if (input.IsKeyDown(Keys.S))
            {
                tempY += speed;
            }

            if (input.IsKeyDown(Keys.A))
            {
                tempX -= speed;
            }

            if (input.IsKeyDown(Keys.D))
            {
                tempX += speed;
            }

            int XScale = Game.WindowWidth / Game.ViewportWidth;
            int YScale = Game.WindowWidth / Game.ViewportHeight;
            int Scale = (XScale > YScale) ? YScale : XScale;

            MouseState mouse = Parent.MouseState.GetSnapshot();
            LookingAt = new((int)(mouse.X / Scale), (int)(mouse.Y / Scale));

            for (int i = Parent.gameObjects.Count - 1; i >= 0; i--)
            {
                GameObject gameObject = Parent.gameObjects[i];
                if (input.IsKeyPressed(Keys.Space))
                {
                    if (gameObject.Location.Distance(Location) < 8)
                    {
                        if (gameObject.GetType() == typeof(BlueDoor))
                        {
                            BlueDoor blueDoor = (BlueDoor)gameObject;
                            blueDoor.Open();
                            continue;
                        }
                        if (hasKey && gameObject.GetType() == typeof(GoldDoor))
                        {
                            GoldDoor goldDoor = (GoldDoor)gameObject;
                            goldDoor.Open();
                            continue;
                        }
                    }
                }
                if (gameObject.Location.Distance(Location) < 2)
                {
                    if (gameObject.GetType() == typeof(Key))
                    {
                        hasKey = true;
                        Parent.gameObjects.Remove(gameObject);
                        Parent.ShowInventory(false,true);
                        continue;
                    }
                    if (gameObject.GetType() == typeof(Chest))
                    {
                        Parent.Score += 100;
                        Parent.gameObjects.Remove(gameObject);
                        Parent.ShowInventory(true);
                        continue;
                    }
                }
            }


            if (tempX != oldTempX || tempY != oldTempY)
            {
                X = (int)tempX;

                foreach (TileData tileData in Parent.Foreground.Tiles)
                {
                    (bool returnResult, Sprite? returnSprite) = ContentManager.Get<Sprite>(SpriteID);
                    if (returnResult == false || returnSprite is null)
                    {
                        continue;
                    }

                    if (Rectanglei.Intersects(tileData.Rectangle, new Rectanglei(X,Y,returnSprite.Size)))
                    {
                        tempX = oldTempX;
                        break;
                    }
                }

                X = (int)tempX;
                Y = (int)tempY;

                foreach (TileData tileData in Parent.Foreground.Tiles)
                {
                    (bool returnResult, Sprite? returnSprite) = ContentManager.Get<Sprite>(SpriteID);
                    if (returnResult == false || returnSprite is null)
                    {
                        continue;
                    }

                    if (Rectanglei.Intersects(tileData.Rectangle, new Rectanglei(X, Y, returnSprite.Size)))
                    {
                        tempY = oldTempY;
                        break;
                    }
                }

                Y = (int)tempY;

                if (tempX > oldTempX)
                {
                    Heading = Headings.East;
                }
                else if (tempX < oldTempX)
                {
                    Heading = Headings.West;
                }
                else if (tempY > oldTempY && tempX == oldTempX)
                {
                    Heading = Headings.South;
                }
                else if (tempY < oldTempY && tempX == oldTempX)
                {
                    Heading = Headings.North;
                }
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is Player player &&
                   base.Equals(obj) &&
                   X == player.X &&
                   Y == player.Y &&
                   Rotation == player.Rotation &&
                   Heading == player.Heading &&
                   SpriteID == player.SpriteID &&
                   LookingAt.Equals(player.LookingAt) &&
                   speed == player.speed &&
                   tempX == player.tempX &&
                   tempY == player.tempY &&
                   hasKey == player.hasKey &&
                   AngledSpriteID == player.AngledSpriteID &&
                   TargetSpriteID == player.TargetSpriteID;
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
            hash.Add(speed);
            hash.Add(tempX);
            hash.Add(tempY);
            hash.Add(hasKey);
            hash.Add(AngledSpriteID);
            hash.Add(TargetSpriteID);
            return hash.ToHashCode();
        }
    }
}
