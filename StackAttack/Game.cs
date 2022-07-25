using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace StackAttack
{
    class Game : GameWindow
    {
        int VertexBufferObject;
        int ElementBufferObject;
        int VertexArrayObject;
        int indiciesLength = 6;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1f);
            ContentManager.Load<Texture>("Sprites/Error.png", "Error");

            Shader s = ContentManager.Load<Shader>("Shaders/baseShader", "BaseShader");
            ContentManager.Load<Texture>("Sprites/IdeaV1.png", "IdeaV1");

            base.OnLoad();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            KeyboardState input = KeyboardState.GetSnapshot();
            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            };

            base.OnUpdateFrame(args);
        }

        protected void GenerateBufferObject()
        {
            float[] vertices =
            {
                +0.5f, +0.5f, 0.0f, 1.0f/8.0f, 0.0f/3.0f, //top right
                +0.5f, -0.5f, 0.0f, 1.0f/8.0f, 1.0f/3.0f, //bottom right
                -0.5f, -0.5f, 0.0f, 0.0f/8.0f, 1.0f/3.0f, //bottom left
                -0.5f, +0.5f, 0.0f, 0.0f/8.0f, 0.0f/3.0f //top left
            };

            uint[] indicies =
            {
                0, 1, 3,
                1, 2, 3
            };

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float),
                vertices, BufferUsageHint.StaticDraw);

            ElementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indicies.Length * sizeof(uint),
                indicies, BufferUsageHint.StaticDraw);
        }

        protected void StartGenerateVertexArrayObject()
        {
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);
        }

        protected void EndGenerateVertexArrayObject()
        {
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            int texCoordLocation = ContentManager.Get<Shader>("BaseShader").GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false,
                5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(0);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            StartGenerateVertexArrayObject();
            GenerateBufferObject();
            EndGenerateVertexArrayObject();

            Shader s = ContentManager.Get<Shader>("BaseShader");
            s.UseShader();
            s.SetInt("texture1", 0);
            ContentManager.Get<Texture>("IdeaV1").UseTexture();
            GL.BindVertexArray(VertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, indiciesLength, DrawElementsType.UnsignedInt, 0);


            Context.SwapBuffers();
            base.OnRenderFrame(args);
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(VertexBufferObject);
            base.OnUnload();
        }
    }
}
