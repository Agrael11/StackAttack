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
        private enum Actions { MoveToPosition, FollowMemory, LookAround, Engage, Strafe}

        public Vector2i LookingAt = new(0, 0);
        public Vector2i Target = new(0, 0);
        public Vector2i Memory = new(0, 0);
        List<Dijkstra.Node>? Path = null;
        float speed = 0.25f;
        float tempX;
        float tempY;
        int timer = 20;
        int stater = 0;
        int steps = 10;
        Actions action = Actions.LookAround;
        string AngledSpriteID = "";
        int MemoryState = 0;
        public int Health { get; set; } = 100;

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
            if (Parent.CurrentScene is null)
                return;
            if (Parent.CurrentScene.GetType() != typeof(Scenes.GameScene))
                return;
            Scenes.GameScene? scene = (Scenes.GameScene)Parent.CurrentScene;
            if (scene is null)
                return;
            if (scene.Player is null)
                return;

            int renderX = X - scene.CameraX;
            int renderY = Y - scene.CameraY;

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

        public bool CanSeeObject(GameObject gameobject)
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

            if (scene.Player.Location.Distance(Location) < 32)
            {
                float lookingAngle = new Vector2(LookingAt.X - X, LookingAt.Y - Y).GetAngle();
                float playerAngle = new Vector2(gameobject.X - X, gameobject.Y - Y).GetAngle();
                
                //if (Math.Abs(playerAngle - lookingAngle) > MathHelper.DegreesToRadians(90) && Math.Abs(playerAngle - lookingAngle) < MathHelper.DegreesToRadians(270)) return false;

                var result = RayCasting.CastRay(new(Location.X+2,Location.Y+2), new Vector2i(scene.Player.Location.X+2, scene.Player.Location.Y+2), this, scene.Foreground, new() { gameobject }, false, 0, 0, gameobject.GetType());
                return (result.result && result.resultObject is not null && result.resultObject.GetType() == gameobject.GetType());
            }
            return false;
        }

        public override void Update(FrameEventArgs args)
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

            bool canSeePlayer = CanSeeObject(scene.Player);
            float playerDistance = new Vector2(Location.X + 2, Location.Y + 2).Distance(scene.Player.Location);

            timer--;
            if (timer <= 0)
            {
                switch (action)
                {
                    case Actions.LookAround:
                        //TOOD: FOV
                        if (canSeePlayer)
                        {
                            //If sees player
                            Target.X = scene.Player.X;
                            Target.Y = scene.Player.Y; //LookAtPlayer
                            //Remember player
                            LookingAt.X = Target.X;
                            LookingAt.Y = Target.Y;
                            Memory.X = Target.X;
                            Memory.Y = Target.Y;
                            MemoryState = 1;
                            //Engage
                            if (playerDistance < 16)
                                action = Actions.Engage;
                            else
                                action = Actions.MoveToPosition;
                        }
                        else if (MemoryState > 0)
                        {
                            Target.X = scene.Player.X;
                            Target.Y = scene.Player.Y; 
                            LookingAt.X = scene.Player.X;
                            LookingAt.Y = scene.Player.Y;
                            MemoryState--;
                            action = Actions.MoveToPosition;
                        }
                        else
                        {
                            LookingAt.X = Random.Shared.Next(0, scene.Level.LevelWidth);
                            LookingAt.Y = Random.Shared.Next(0, scene.Level.LevelHeight);
                            var castresult = RayCasting.CastRay(new(Location.X+2, Location.Y+2), LookingAt, this, scene.Foreground, new(), false, 0, 0);
                            if (castresult.result == true && castresult.tile is not null)
                            {
                                LookingAt = new Vector2i(castresult.tile.Value.TileX*4 - X, castresult.tile.Value.TileY*4 - Y);
                                LookingAt = LookingAt.SetMagnitude(LookingAt.GetMagnitude() * 0.5f);
                                LookingAt = new Vector2i(X + LookingAt.X, Y + LookingAt.Y);
                            }
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
                        }
                        break;
                    case Actions.MoveToPosition:
                        if (canSeePlayer && playerDistance <16)
                        {
                            //If sees player
                            Target.X = scene.Player.X;
                            Target.Y = scene.Player.Y; //LookAtPlayer
                            //Remember player
                            LookingAt.X = Target.X;
                            LookingAt.Y = Target.Y;
                            Memory.X = Target.X;
                            Memory.Y = Target.Y;
                            Path = null;
                            action = Actions.Engage;
                            break;
                        }
                        else if (canSeePlayer)
                        {
                            if (steps <= 0)
                            {
                                Path = null;
                                //If sees player
                                Target.X = scene.Player.X;
                                Target.Y = scene.Player.Y; //LookAtPlayer
                                                            //Remember player
                                LookingAt.X = Target.X;
                                LookingAt.Y = Target.Y;
                                Memory.X = Target.X;
                                Memory.Y = Target.Y;
                                steps = 10;
                            }
                            steps--;
                        }
                        else
                        {
                            steps = 10;
                        }
                        if (Path is null)
                        {
                            Path = Dijkstra.DoDijkstra(new(Location.X / 4, Location.Y / 4), new(Target.X / 4, Target.Y / 4), Parent);
                        }    
                        if (Path is null || Path.Count == 0)
                        {
                            Path = null;
                            action = Actions.LookAround;
                            timer = 20;
                            break;
                        }
                        Dijkstra.Node targetNode = Path[0];
                        Vector2i target = new Vector2i(targetNode.X * 4, targetNode.Y * 4);
                        while (Location == target)
                        {
                            Path.RemoveAt(0);
                            if (Path.Count == 0) break;
                            targetNode = Path[0];
                            target = new Vector2i(targetNode.X * 4, targetNode.Y * 4);
                        }
                        timer--;
                        if (timer > 0) 
                            break; 
                        if (X > target.X)
                        {
                            X--;
                            timer = 10;
                            break;
                        }
                        if (X < target.X)
                        {
                            X++;
                            timer = 10;
                            break;
                        }
                        if (Y > target.Y)
                        {
                            Y--;
                            timer = 10;
                            break;
                        }
                        if (Y < target.Y)
                        {
                            Y++;
                            timer = 10;
                            break;
                        }
                        break;
                    case Actions.Engage:
                        if (!canSeePlayer)
                        {
                            action = Actions.LookAround;
                        }
                        if (playerDistance > 16)
                        {
                            action = Actions.MoveToPosition;
                        }
                        break;
                }
            }

            foreach (GameObject gameObject in scene.GameObjects)
            {
                if (gameObject.GetType() == typeof(BlueDoor))
                {
                    if (gameObject.Location.Distance(Location) < 5)
                    {
                        ((BlueDoor)gameObject).Open();
                    }
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
