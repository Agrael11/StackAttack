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
        public static float Distance(this Vector2i me, Vector2i vector)
        {
            return Vector2.Distance(new Vector2(me.X, me.Y), new Vector2(vector.X, vector.Y));
        }

        public static float Distance(this Vector2 me, Vector2 vector)
        {
            return (Vector2.Distance(me, vector));
        }

        public static float GetAngle(this Vector2i me)
        {
            return new Vector2(me.X, me.Y).GetAngle();
        }

        public static float GetAngle(this Vector2 me)
        {
            float angle = (float)(2 * Math.Atan(me.Y / (me.X + Math.Sqrt(me.X * me.X + me.Y * me.Y))));
            if (float.IsNaN(angle)) angle = (float)(Math.PI);
            if (angle < 0) angle += (float)Math.Tau;
            if (angle > Math.Tau) angle -= (float)Math.Tau;
            return angle;
        }

        public static Vector2 SetMagnitude(this Vector2 me, float magnitude)
        {
            me.Normalize();
            me.X *= magnitude;
            me.Y *= magnitude;
            return me;
        }

        public static Vector2i FromAnglei(float angle, float magnitude = 1)
        {
            Vector2 v = FromAngle(angle, magnitude);
            return new((int)v.X, (int)v.Y);
        }

        public static Vector2 FromAngle(float angle, float magnitude = 1)
        {
            //if (angle > Math.PI) angle = (float)(Math.Tau-angle);
            Vector2 v = new((float)Math.Cos(angle), (float)Math.Sin(angle));
            v = v.SetMagnitude(magnitude);
            return v;
        }
    }
}
