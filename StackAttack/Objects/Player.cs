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
        float speed = 0.25f;
        float tempX;
        float tempY;
        bool hasKey = false;

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
        }

        public override GameObject CreateNew(int x, int y, float rotation, Headings heading, Game? game, string spriteID)
        {
            return new Player(x, y, rotation, heading, game, spriteID);
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

            for (int i = Parent.gameObjects.Count - 1; i >= 0; i--)
            {
                GameObject gameObject = Parent.gameObjects[i];
                if (input.IsKeyPressed(Keys.Space))
                {
                    if (gameObject.Location.Dist(Location) < 8)
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
                if (gameObject.Location.Dist(Location) < 2)
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

            foreach (GameObject gameObject in Parent.gameObjects)
            {

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
    }
}
