using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack
{
    internal class Sprite : IDisposable
    {
        public struct SpriteDefinition
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string TextureID { get; set; }
            public string ShaderID { get; set; }
            public string SpriteID { get; set; }

            public SpriteDefinition(string spriteID, string textureID, string shaderID, int x, int y, int width, int height)
            {
                TextureID = textureID;
                ShaderID = shaderID;
                SpriteID = spriteID;
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }
        }

        public string TextureID { get; set; }
        public string ShaderID { get; set; }
        public Vector2i Location { get; set; }
        public Vector2i Size { get; set; }

        private int VertexBufferObject;
        private int ElementBufferObject;
        private int VertexArrayObject;
        private int indiciesLength = -1;

        public Sprite(string textureID, string shaderID, Vector2i location, Vector2i size)
        {
            TextureID = textureID;
            ShaderID = shaderID;
            Location = location;
            Size = size;

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GenerateVertexElementBuffers();

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            int texCoordLocation = ContentManager.Get<Shader>(shaderID).GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false,
                5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(0);
        }

        public Sprite(string textureID, string shaderID, Vector2i location)
        {
            TextureID = textureID;
            ShaderID = shaderID;
            Location = location;
            Texture textureReference = ContentManager.Get<Texture>(textureID);
            Size = new Vector2i(textureReference.Width - location.X, textureReference.Height - location.Y);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GenerateVertexElementBuffers();

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            int texCoordLocation = ContentManager.Get<Shader>(shaderID).GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false,
                5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(0);
        }

        public Sprite(string textureID, string shaderID)
        {
            TextureID = textureID;
            ShaderID = shaderID;
            Location = new Vector2i(0, 0);
            Texture textureReference = ContentManager.Get<Texture>(textureID);
            Size = new Vector2i(textureReference.Width, textureReference.Height);

            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            GenerateVertexElementBuffers();

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            int texCoordLocation = ContentManager.Get<Shader>(shaderID).GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false,
                5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(0);
        }

        protected void GenerateVertexElementBuffers()
        {
            Texture textureReference = ContentManager.Get<Texture>(TextureID);
            float myX1 = Location.X / (float)textureReference.Width;
            float myY1 = Location.Y / (float)textureReference.Height;
            float myX2 = myX1 + (Size.X / (float)textureReference.Width);
            float myY2 = myY1 + (Size.Y / (float)textureReference.Height);

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

        public void Draw(Vector2i position, float rotation = 0, bool horizontalFlip = false, bool verticalFlip = false)
        {
            Draw(position, Size, rotation, horizontalFlip, verticalFlip);
        }

        public void Draw(Vector2i position, Vector2i size, float rotation = 0, bool horizontalFlip = false, bool verticalFlip = false)
        {
            Shader shaderReference = ContentManager.Get<Shader>(ShaderID);
            shaderReference.UseShader();

            float realX = position.X / (Game.ViewportWidth/2f);
            realX -= 1;

            float realY = position.Y / (Game.ViewportHeight/2f);
            realY = 2 - realY;
            realY -= 2;

            float realWidth = size.X / (Game.ViewportWidth/2f);
            float realHeight = size.Y / (Game.ViewportHeight/2f);

            Matrix4 translate = Matrix4.CreateTranslation(realX, realY, 0);
            Matrix4 rotationM = Matrix4.CreateTranslation(-0.5f, -0.5f, 0) * Matrix4.CreateRotationZ((float)Math.Tau-rotation) * Matrix4.CreateTranslation(0.5f, 0.5f, 0);
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

            shaderReference.SetMatrix4("transform", ref transform);

            shaderReference.SetInt("texture1", 0);
            ContentManager.Get<Texture>(TextureID).UseTexture();
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, indiciesLength, DrawElementsType.UnsignedInt, 0);

        }

        public static void Draw(string spriteID, Vector2i position, float rotation = 0, bool horizontalFlip = false, bool verticalFlip = false)
        {
            Sprite sprite = ContentManager.Get<Sprite>(spriteID);
            sprite.Draw(position, rotation, horizontalFlip, verticalFlip);
        }

        public static void Draw(string spriteID, Vector2i position, Vector2i size, float rotation = 0, bool horizontalFlip = false, bool verticalFlip = false)
        {
            Sprite sprite = ContentManager.Get<Sprite>(spriteID);
            sprite.Draw(position, size, rotation, horizontalFlip, verticalFlip);
        }

        public static void DrawTexture(string textureID, Vector2i sourcePosition, Vector2i sourceSize, string shaderID, Vector2i position, Vector2i size, float rotation = 0, bool horizontalFlip = false, bool verticalFlip = false)
        {
            Sprite tempSprite = new Sprite(textureID, shaderID, sourcePosition, sourceSize);
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
