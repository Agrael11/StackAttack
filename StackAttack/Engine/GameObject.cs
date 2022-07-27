using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine
{
    internal abstract class GameObject
    {
        public struct GameObjectDefinition
        {
            public string ObjectID { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public float Rotation { get; set; }
            public Headings Heading { get; set; }
            public string SpriteID { get; set; }

            public GameObjectDefinition(string objectID, int x, int y, float rotation, Headings heading, string spriteID)
            {
                ObjectID = objectID;
                X = x;
                Y = y;
                Rotation = rotation;
                Heading = heading;
                SpriteID = spriteID;
            }
        }

        public int X { get; set; }
        public int Y { get; set; }
        public float Rotation { get; set; }
        public Headings Heading { get; set; }
        public string SpriteID { get; set; } = "";

        public GameObject()
        {

        }

        public GameObject(int x, int y, float rotation, Headings heading)
        {
            X = x;
            Y = y;
            Rotation = rotation;
            Heading = heading;
        }

        public abstract void Update(FrameEventArgs args);

        public abstract void Draw(FrameEventArgs args);

        public abstract GameObject CreateNew(int x, int y, float rotation, Headings heading);
    }
}
