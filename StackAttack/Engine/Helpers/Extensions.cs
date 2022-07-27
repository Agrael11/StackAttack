using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine.Helpers
{
    internal static class Extensions
    {
        public static float Dist(this Vector2i me,  Vector2i vector)
        {
            float xDist = vector.X - me.X;
            float yDist = vector.Y - me.Y;
            return (float)Math.Sqrt(xDist * xDist + yDist * yDist);
        }
    }
}
