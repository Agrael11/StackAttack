﻿using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using StackAttack.Engine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine
{
    public class Tile : IDisposable
    {
        public struct TileDefinition
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string TextureID { get; set; }
            public string ShaderID { get; set; }
            public string TileID { get; set; }

            public TileDefinition(string tileID, string textureID, string shaderID, int x, int y, int width, int height)
            {
                TextureID = textureID;
                ShaderID = shaderID;
                TileID = tileID;
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }
        }

        private Sprite sprite;
        public string TextureID { get { return sprite.TextureID; } set { sprite.TextureID = value; } }
        public string ShaderID { get { return sprite.ShaderID; } set { sprite.ShaderID = value; } }
        public Vector2i Location { get { return sprite.Location; } set { sprite.Location = value; } }
        public Vector2i Size { get { return sprite.Size; } set { sprite.Size = value; } }
        public Rectanglei Rectangle { get { return new Rectanglei(Location.X*4,Location.Y*4, Size); } set { Location = new(value.X / 4, value.Y / 4); Size = value.Size; } }

        public Tile(string textureID, string shaderID, Vector2i location, Vector2i size)
        {
            sprite = new Sprite(textureID, shaderID, location, size);
            TextureID = textureID;
            Location = location;
            Size = size;
        }

        public void Dispose()
        {
            sprite.Dispose();
        }

        public void Draw(Vector2i position, float rotation, bool horizontalFlip = false, bool verticalFlip = false)
        {
            sprite.Draw(position, Size, rotation, horizontalFlip, verticalFlip);
        }

        public void Draw(Rectanglei rectangle, float rotation, bool horizontalFlip = false, bool verticalFlip = false)
        {
            sprite.Draw(rectangle.Location, rectangle.Size, rotation, horizontalFlip, verticalFlip);
        }

        public void Draw(Vector2i position, Vector2i size, float rotation, bool horizontalFlip = false, bool verticalFlip = false)
        {
            sprite.Draw(position, size, rotation, horizontalFlip, verticalFlip);
        }

        public static void Draw(string tileID, Vector2i position, float rotation, bool horizontalFlip = false, bool verticalFlip = false)
        {
            (bool returnResult, Tile? tileResult) = ContentManager.Get<Tile>(tileID);
            if (!returnResult || tileResult is null)
                return;

            tileResult.sprite.Draw(position, tileResult.Size, rotation, horizontalFlip, verticalFlip);
        }

        public static void Draw(string tileID, Rectanglei dstRectangle, float rotation, bool horizontalFlip = false, bool verticalFlip = false)
        {
            (bool returnResult, Tile? tileResult) = ContentManager.Get<Tile>(tileID);
            if (!returnResult || tileResult is null)
                return;

            tileResult.sprite.Draw(dstRectangle.Location, dstRectangle.Size, rotation, horizontalFlip, verticalFlip);
        }

        public static void Draw(string tileID, Vector2i position, Vector2i size, float rotation, bool horizontalFlip = false, bool verticalFlip = false)
        {
            (bool returnResult, Tile? tileResult) = ContentManager.Get<Tile>(tileID);
            if (!returnResult || tileResult is null)
                return;

            tileResult.sprite.Draw(position, size, rotation, horizontalFlip, verticalFlip);
        }
    }
}
