using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine.Helpers
{
    public class Rectangle
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float X2 { get { return X + Width; } }
        public float Y2 { get { return Y + Height; } }
        public Vector2 Location { get { return new Vector2(X, Y); } set { X = value.X; Y = value.Y; } }
        public Vector2 Size { get { return new Vector2(Width, Height); } set { Width = value.X; Height = value.Y; } }

        public Rectangle(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectangle(Vector2 location, float width, float height)
        {
            Width = width;
            Height = height;
            Location = location;
        }

        public Rectangle(float x, float y, Vector2 size)
        {
            X = x;
            Y = y;
            Size = size;
        }

        public Rectangle(Vector2 location, Vector2 size)
        {
            Location = location;
            Size = size;
        }

        public static bool Intersects(Rectangle rectangle1, Rectangle rectangle2)
        {
            return (rectangle1.X < rectangle2.X2 && rectangle1.X2 > rectangle2.X && rectangle1.Y < rectangle2.Y2 && rectangle1.Y2 > rectangle2.Y);
        }

        public  bool Intersects(Rectangle rectangle)
        {
            return (X < rectangle.X2 && X2 > rectangle.X && Y < rectangle.Y2 && Y2 > rectangle.Y);
        }

        public override string ToString()
        {
            return $"{X}x{Y} {Width}x{Height}";
        }
    }
}
