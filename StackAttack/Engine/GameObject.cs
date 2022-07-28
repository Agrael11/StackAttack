using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine
{
    public abstract class GameObject
    {
        public struct GameObjectDefinition
        {
            public string ObjectID { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public float Rotation { get; set; }
            public Headings Heading { get; set; }
            public string SpriteID { get; set; }
            public Game? Parent { get; set; }

            public GameObjectDefinition(string objectID, Game? parent, int x, int y, float rotation, Headings heading, string spriteID)
            {
                ObjectID = objectID;
                X = x;
                Y = y;
                Rotation = rotation;
                Heading = heading;
                SpriteID = spriteID;
                Parent = parent;
            }
        }

        public int X { get; set; }
        public int Y { get; set; }
        public Vector2i Location
        {
            get
            {
                return new Vector2i(X, Y);
            }
        }
        public float Rotation { get; set; }
        public Headings Heading { get; set; }
        public string SpriteID { get; set; } = "";
        public Game? Parent { get; set; }

        public GameObject()
        {

        }

        public GameObject(int x, int y, float rotation, Headings heading, Game? parent, string spriteID)
        {
            X = x;
            Y = y;
            Rotation = rotation;
            Heading = heading;
            Parent = parent;
            SpriteID = spriteID;
        }

        public abstract void Update(FrameEventArgs args);

        public abstract void Draw(FrameEventArgs args);

        public abstract GameObject CreateNew(int x, int y, float rotation, Headings heading, Game parent, string spriteID);

        public override bool Equals(object? obj)
        {
            return obj is GameObject @object &&
                   X == @object.X &&
                   Y == @object.Y &&
                   Rotation == @object.Rotation &&
                   Heading == @object.Heading &&
                   SpriteID == @object.SpriteID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Rotation, Heading, SpriteID);
        }
    }
}
