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
    public class TwoTextureSprite : IDisposable
    {
        public string Texture1ID { get; set; }
        public string Texture2ID { get; set; }
        public string ShaderID { get; set; }
        public Vector2i Location { get; set; }
        public Vector2i Size { get; set; }
        public Rectanglei Rectangle { get { return new Rectanglei(Location, Size); }}

        private int VertexBufferObject;
        private int ElementBufferObject;
        private int VertexArrayObject;
        private int indiciesLength = -1;


        public TwoTextureSprite(string texture1ID, string texture2ID, string shaderID, Vector2i location, Vector2i size)
        {
            Texture1ID = texture1ID;
            Texture2ID = texture2ID;
            ShaderID = shaderID;
            Location = location;
            Size = size;

            (bool returnResult, Shader? shaderResult) = ContentManager.Get<Shader>(shaderID);
            if (!returnResult || shaderResult is null)
                return;

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GenerateVertexElementBuffers();

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            int texCoordLocation = shaderResult.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false,
                5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(0);
        }

        protected void GenerateVertexElementBuffers()
        {
            (bool returnResult, Texture? textureResult) = ContentManager.Get<Texture>(Texture1ID);
            if (!returnResult || textureResult is null)
                return;
            float myX1 = Location.X / (float)textureResult.Width;
            float myY1 = Location.Y / (float)textureResult.Height;
            float myX2 = myX1 + Size.X / (float)textureResult.Width;
            float myY2 = myY1 + Size.Y / (float)textureResult.Height;

            float[] vertices =
           {
                1f, 1f, 0.0f, myX2, myY1, //top right
                1f, 0f, 0.0f, myX2, myY2, //bottom right
                0f, 0f, 0.0f, myX1, myY2, //bottom left
                0f, 1f, 0.0f, myX1, myY1 //top left
            };

            uint[] indicies =
            {
                0, 1, 3,
                1, 2, 3
            };
            indiciesLength = indicies.Length;

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float),
                vertices, BufferUsageHint.StaticDraw);

            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indicies.Length * sizeof(uint),
                indicies, BufferUsageHint.StaticDraw);
        }

        public void Draw(Vector2i position, Vector2i size, float rotation = 0, bool horizontalFlip = false, bool verticalFlip = false)
        {
            (bool returnResult, Texture? texture1Result) = ContentManager.Get<Texture>(Texture1ID);
            if (!returnResult || texture1Result is null)
                return;
            (returnResult, Texture? texture2Result) = ContentManager.Get<Texture>(Texture2ID);
            if (!returnResult || texture2Result is null)
                return;
            (returnResult, Shader? shaderResult) = ContentManager.Get<Shader>(ShaderID);
            if (!returnResult || shaderResult is null)
                return;

            shaderResult.UseShader();

            float realX = position.X / (Game.ViewportWidth / 2f);
            realX -= 1;

            float realY = position.Y / (Game.ViewportHeight / 2f);
            realY = 2 - realY;
            realY -= 2;

            float realWidth = size.X / (Game.ViewportWidth / 2f);
            float realHeight = size.Y / (Game.ViewportHeight / 2f);

            Matrix4 translate = Matrix4.CreateTranslation(realX, realY, 0);
            Matrix4 rotationM = Matrix4.CreateTranslation(-0.5f, -0.5f, 0) * Matrix4.CreateRotationZ((float)Math.Tau - rotation) * Matrix4.CreateTranslation(0.5f, 0.5f, 0);
            Matrix4 scale = Matrix4.CreateTranslation(0, -1, 0) * Matrix4.CreateScale(realWidth, realHeight, 1f);
            if (horizontalFlip)
            {
                scale *= Matrix4.CreateScale(-1f, 1f, 1f) * Matrix4.CreateTranslation(2f, 0, 0);
            }
            if (verticalFlip)
            {
                scale *= Matrix4.CreateScale(1, -1f, 1f) * Matrix4.CreateTranslation(0, -2f, 0);
            }
            scale *= Matrix4.CreateTranslation(0, 1, 0);
            Matrix4 transform = rotationM * scale * translate;

            shaderResult.SetMatrix4("transform", ref transform);
            shaderResult.SetInt("texture1", 0);
            shaderResult.SetInt("texture2", 1);

            texture1Result.UseTexture(TextureUnit.Texture0);
            texture2Result.UseTexture(TextureUnit.Texture1);
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, indiciesLength, DrawElementsType.UnsignedInt, 0);

        }

        public static void DrawTexture(string texture1ID, string texture2ID, string shaderID, Rectanglei sourceRectangle, Rectanglei destinationRectangle, float rotation = 0, bool horizontalFlip = false, bool verticalFlip = false)
        {
            TwoTextureSprite tempSprite = new TwoTextureSprite(texture1ID, texture2ID, shaderID, sourceRectangle.Location, sourceRectangle.Size);
            tempSprite.Draw(destinationRectangle.Location, destinationRectangle.Size, rotation, horizontalFlip, verticalFlip);
            tempSprite.Dispose();
        }

        public static void DrawTexture(string texture1ID, string texture2ID, string shaderID, Vector2i sourcePosition, Vector2i sourceSize, Vector2i position, Vector2i size, float rotation = 0, bool horizontalFlip = false, bool verticalFlip = false)
        {
            TwoTextureSprite tempSprite = new TwoTextureSprite(texture1ID, texture2ID, shaderID, sourcePosition, sourceSize);
            tempSprite.Draw(position, size, rotation, horizontalFlip, verticalFlip);
            tempSprite.Dispose();
        }

        public void Dispose()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.DeleteBuffer(VertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.DeleteBuffer(ElementBufferObject);
            GL.BindVertexArray(VertexArrayObject);
            GL.DeleteVertexArray(VertexArrayObject);
        }
    }
}
