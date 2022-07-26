using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace StackAttack
{
    internal class Texture : IDisposable, ILoadable<Texture>
    {
        public struct TextureDefinition 
        {
            public string TextureID { get; set; }
            public string FileName { get; set; }

            public TextureDefinition(string textureID, string fileName)
            {
                TextureID = textureID;
                FileName = fileName;
            }
        }

        //public string ID = "NullTexture";
        private int _handle;
        public int Handle { get { return _handle; } }
        private int _width = 0;
        public int Width { get { return _width; } }
        private int _height = 0;
        public int Height { get { return _height; } }
        private byte[] _pixels = Array.Empty<byte>();
        public TextureWrapMode TextureWrapS { get; set; } = TextureWrapMode.Repeat;
        public TextureWrapMode TextureWrapT { get; set; } = TextureWrapMode.Repeat;
        public TextureMinFilter TextureMinFilter { get; set; } = TextureMinFilter.Nearest;
        public TextureMagFilter TextureMagFilter { get; set; } = TextureMagFilter.Nearest;

        public Texture()
        {

        }
        public Texture(int handle, int width, int height)
        {
            _handle = handle;
            _width = width;
            _height = height;
        }

        public Texture? Load(string path) 
        {
            Image<Rgba32> image;
            try
            {
                image = Image.Load<Rgba32>(path);
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Levels.Warn, ex.Message);
                return null;
            }
            //senk you, actually let me change the code a litle
            _width = image.Width;
            _height = image.Height;
            _pixels = new byte[4 * image.Width * image.Height];
        
            image.CopyPixelDataTo(_pixels);
            
            //GL.ActiveTexture(TextureUnit.Texture0);
            _handle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _handle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapT);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _width, _height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, _pixels);
            image.Dispose();
            return this; //načo vytvárať iný? :D tiez som si vravel :D, to je ešte ako som to predtým mal static, keď som nemal ten interface aha
        }

        public void Reload()
        {

        }

        public byte[] GetPixels()
        {
            return _pixels;
        }

        public void SetPixels(byte[] data)
        {
            if (data.Length != 4 * _width * _height)
            {
                throw new Exception("Unexpected data size");
            }
            _pixels = data;
        }

        public void SetPixels(byte[] data, int width, int height)
        {
            if (data.Length != 4 * width * height)
            {
                throw new Exception("Unexpected data size");
            }
            _width = width;
            _height = height;
            _pixels = data;
        }

        public void UseTexture(TextureUnit unit = TextureUnit.Texture0)
        {

            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, _handle); //len som chcel pozrieť, či to tam má dáta :D
        }

        public void Dispose()
        {
        }
    }
}
