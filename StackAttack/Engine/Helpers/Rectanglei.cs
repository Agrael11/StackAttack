using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine.Helpers
{
    public class Rectanglei
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int X2 { get { return X + Width; } }
        public int Y2 { get { return Y + Height; } }
        public Vector2i Location { get { return new Vector2i(X, Y); } set { X = value.X; Y = value.Y; } }
        public Vector2i Size { get { return new Vector2i(Width, Height); } set { Width = value.X; Height = value.Y; } }

        public Rectanglei(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rectanglei(Vector2i location, int width, int height)
        {
            Width = width;
            Height = height;
            Location = location;
        }

        public Rectanglei(int x, int y, Vector2i size)
        {
            X = x;
            Y = y;
            Size = size;
        }

        public Rectanglei(Vector2i location, Vector2i size)
        {
            Location = location;
            Size = size;
        }

        public static bool Intersects(Rectanglei rectangle1, Rectanglei rectangle2)
        {
            return (rectangle1.X < rectangle2.X2 && rectangle1.X2 > rectangle2.X && rectangle1.Y < rectangle2.Y2 && rectangle1.Y2 > rectangle2.Y);
        }

        public  bool Intersects(Rectanglei rectangle)
        {
            return (X < rectangle.X2 && X2 > rectangle.X && Y < rectangle.Y2 && Y2 > rectangle.Y);
        }

        public override string ToString()
        {
            return $"{X}x{Y} {Width}x{Height}";
        }
    }
}
