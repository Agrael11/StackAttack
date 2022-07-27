using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine
{
    internal class Tile : IDisposable
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

        public void Draw(Vector2i position, Vector2i size, float rotation, bool horizontalFlip = false, bool verticalFlip = false)
        {
            sprite.Draw(position, size, rotation, horizontalFlip, verticalFlip);
        }

        public static void Draw(string tileID, Vector2i position, float rotation, bool horizontalFlip = false, bool verticalFlip = false)
        {
            Tile tile = ContentManager.Get<Tile>(tileID);
            tile.sprite.Draw(position, tile.Size, rotation, horizontalFlip, verticalFlip);
        }

        public static void Draw(string tileID, Vector2i position, Vector2i size, float rotation, bool horizontalFlip = false, bool verticalFlip = false)
        {
            Tile tile = ContentManager.Get<Tile>(tileID);
            tile.sprite.Draw(position, size, rotation, horizontalFlip, verticalFlip);
        }
    }
}
