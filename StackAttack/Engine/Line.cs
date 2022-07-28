using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using StackAttack.Engine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine
{
    internal class Line : IDisposable
    {
        private static Vector4 _color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        private static string _shaderID = "Line";

        public static Vector4 Color { 
            get { return _color; }
            set { _color = value; _generated = false; } }

        private static int _VertexBufferObject;
        private static int _ElementBufferObject;
        private static int _VertexArrayObject;
        private static int _indiciesLength = -1;

        private static bool _generated = false;


        protected static void GenerateVertexElementBuffers()
        {
            float[] vertices =
           {
                1f, 1f, 0.0f, Color.X, Color.Y, Color.W, Color.W, //top right
                1f, 0f, 0.0f, Color.X, Color.Y, Color.W, Color.W,//bottom right
                0f, 0f, 0.0f, Color.X, Color.Y, Color.W, Color.W,//bottom left
                0f, 1f, 0.0f, Color.X, Color.Y, Color.W, Color.W //top left
            };

            uint[] indicies =
            {
                0, 1, 3,
                1, 2, 3
            };
            _indiciesLength = indicies.Length;

            _VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float),
                vertices, BufferUsageHint.StaticDraw);

            _ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indicies.Length * sizeof(uint),
                indicies, BufferUsageHint.StaticDraw);
        }
        
        private static void Generate()
        {
            _generated = true;
            (bool returnResult, Shader? shaderResult) = ContentManager.Get<Shader>(_shaderID);
            if (!returnResult || shaderResult is null)
                return;

            _VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_VertexArrayObject);

            GenerateVertexElementBuffers();

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
            int colorLocation = shaderResult.GetAttribLocation("aColor");
            GL.EnableVertexAttribArray(colorLocation);
            GL.VertexAttribPointer(colorLocation, 4, VertexAttribPointerType.Float, false,
                7 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(0);
        }

        public static void Draw(Vector2 location1, Vector2 location2, float strokeWidth)
        {
            if (!_generated)
            {
                Generate();
            }

            float myX1 = location1.X;
            float myY1 = location1.Y;
            float myX2 = location2.X;
            float myY2 = location2.Y;
            float rotation = new Vector2(myX2-myX1, myY2-myY1).GetAngle();
            float size = new Vector2(myX1, myY1).Distance(new Vector2(myX2, myY2));

            (bool returnResult, Shader? shaderResult) = ContentManager.Get<Shader>(_shaderID);
            if (!returnResult || shaderResult is null)
                return;

            shaderResult.UseShader();

            float realX = myX1 / (Game.ViewportWidth / 2f);
            realX -= 1;

            float realY = myY1 / (Game.ViewportHeight / 2f);
            realY = 2 - realY;
            realY -= 2;

            float realWidth = size / (Game.ViewportWidth / 2f);
            float realHeight = strokeWidth / (Game.ViewportHeight / 2f);

            Matrix4 translate = Matrix4.CreateTranslation(realX, realY, 0);
            Matrix4 rotationM = Matrix4.CreateTranslation(0f, -1f, 0)
                * Matrix4.CreateRotationZ((float)Math.Tau - rotation) 
                * Matrix4.CreateTranslation(0f, 1f, 0);
            Matrix4 scale = Matrix4.CreateTranslation(0, -1, 0) * Matrix4.CreateScale(realWidth, realHeight, 1f);
            scale *= Matrix4.CreateTranslation(0, 1, 0);
            Matrix4 transform = scale * rotationM * translate;

            shaderResult.SetMatrix4("transform", ref transform);
            
            GL.BindVertexArray(_VertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, _indiciesLength, DrawElementsType.UnsignedInt, 0);

        }

        public void Dispose()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VertexBufferObject);
            GL.DeleteBuffer(_VertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ElementBufferObject);
            GL.DeleteBuffer(_ElementBufferObject);
            GL.BindVertexArray(_VertexArrayObject);
            GL.DeleteVertexArray(_VertexArrayObject);
        }
    }
}
