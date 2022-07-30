using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StackAttack.Engine;
using StackAttack.Engine.Helpers;

namespace StackAttack.Scenes
{
    public class CreditsScene : Scene
    {
        public CreditsScene(Game parent) : base(parent)
        {
        }

        readonly List<string> credits = ("@CREATED BY\n" +
            "$===========\n" +
            "#TACHI\n" +
            "\n" +
            "\n" +
            "\n" +
            "@PROGRAMMING\n" +
            "$===========\n" +
            "#TACHI\n" +
            "\n" +
            "\n" +
            "\n" +
            "@SOUNDS\n" +
            "$===========\n" +
            "#TACHI\n" +
            "\n" +
            "\n" +
            "\n" +
            "@SOUNDS MADE\n" +
            "@WITH\n" +
            "$===========\n" +
            "JSFXR\n" +
            "\n" +
            "\n" +
            "\n" +
            "@MUSIC FROM\n" +
            "$===========\n" +
            "FESLIYAN\n" +
            "STUDIOS.COM\n" +
            "\n" +
            "\n" +
            "\n" +
            "@DAVID RANDA\n" +
            "$===========\n" +
            "8 BIT MENU\n" +
            "\n" +
            "8 BIT\n" +
            "NOSTALGIA\n" +
            "\n" +
            "8 BIT RETRO\n" +
            "FUNK\n" +
            "\n" +
            "\n" +
            "\n" +
            "@DAVID\n" +
            "@FESLIYAN\n" +
            "$===========\n" +
            "8 BIT\n" +
            "SMOOTH\n" +
            "PRESENTATI-\n" +
            "ON\n" +
            "\n" +
            "A BIT OF\n" +
            "HOPE\n" +
            "\n" +
            "RETRO\n" +
            "FOREST\n" +
            "\n" +
            "\n" +
            "\n" +
            "@USING\n" +
            "@LIBRARIES\n" +
            "$=========\n" +
            "OPENTK\n" +
            "\n" +
            "NAUDIO\n" +
            "\n" +
            "MANAGEDBASS\n" +
            "\n" +
            "UN4SEEN\n" +
            "BASS\n" +
            "\n" +
            "SIXLABORS\n" +
            "IMAGESHARP\n" +
            "\n" +
            "\n" +
            "\n" +
            "€THANK YOU\n" +
            "€FOR PLAYING\n" +
            "€MY GAME").Split('\n').ToList();

        float yScroll = 0;

        public override void Dispose()
        {
            _shaderDefinitions.Clear();
            _textureDefinitions.Clear();
            _spriteDefinitions.Clear();
            _soundDefinitions.Clear();
            ContentManager.RemoveAll();
        }

        private List<Shader.ShaderDefinition> _shaderDefinitions = new();
        private List<Texture.TextureDefinition> _textureDefinitions = new();
        private List<Sprite.SpriteDefinition> _spriteDefinitions = new();
        private List<Sound.SoundDefinition> _soundDefinitions = new();

        private void LoadDefinitions()
        {
            foreach (var soundDefinition in _soundDefinitions)
            {
                var (returnState, returnObject) = ContentManager.Load<Sound>(soundDefinition.SoundID, soundDefinition.FileName);
                if (returnState && returnObject is not null)
                {
                    returnObject.Volume = soundDefinition.Volume;
                    returnObject.Looping = soundDefinition.Looping;
                }
            }

            foreach (var shaderDefinition in _shaderDefinitions)
            {
                ContentManager.Load<Shader>(shaderDefinition.ShaderID, shaderDefinition.FileName);
            }

            foreach (var textureDefinition in _textureDefinitions)
            {
                ContentManager.Load<Texture>(textureDefinition.TextureID, textureDefinition.FileName);
            }

            foreach (var spriteDefinition in _spriteDefinitions)
            {
                ContentManager.Add(spriteDefinition.SpriteID, new Sprite(spriteDefinition.TextureID, spriteDefinition.ShaderID, new Vector2i(spriteDefinition.X, spriteDefinition.Y), new Vector2i(spriteDefinition.Width, spriteDefinition.Height)));
            }
        }



        public override void Draw(FrameEventArgs args, ref RenderTexture texture)
        {
            var (returnStatus, returnObject) = ContentManager.Get<Shader>("BaseShader");
            if (!returnStatus || returnObject is null)
            {
                Logger.Log(Logger.Levels.Error, "Could not load shader");
                return;
            }
            texture.Begin();
            
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            for (int i = 0; i < credits.Count; i++)
            {
                string text = credits[i];
                
                if (text.Length > 0 && text[0] == '@')
                {
                    text = text[1..];
                    SetColor(0.475f, 0.859f, 0.506f, 1);
                }
                else if (text.Length > 0 && text[0] == '#')
                {
                    text = text[1..];
                    SetColor(0.494f, 0.569f, 1, 1);
                }
                else if (text.Length > 0 && text[0] == '€')
                {
                    text = text[1..];
                    SetColor(1, 0.765f, 0, 1);
                }
                else if (text.Length > 0 && text[0] == '$')
                {
                    text = text[1..];
                    SetColor(0.5f, 0.5f, 0.5f, 1);
                }
                else
                {
                    SetColor(1, 1, 1, 1);
                }

                Scenes.GameScene.DrawText(text, (Game.ViewportWidth - text.Length*5)/2, i*7+Game.ViewportHeight-(int)(yScroll/10));
                //Scenes.GameScene.DrawText(text, (Game.ViewportWidth - text.Length * 5) / 2, i * 7 + Game.ViewportHeight - (int)(yScroll));
                SetColor(1, 1, 1, 1);
            }
            RenderTexture.End();
            yScroll++;
        }

        public override void Init()
        {
            if (Game.BackgroundMusic is not null)
            {
                Game.BackgroundMusic.StopUseSound();
                Game.BackgroundMusic = null;
            }

            Game.LoadDefinitionData("SFX/soundsDefinitions.json", ref _soundDefinitions);
            Game.LoadDefinitionData("Shaders/shaderDefinitions.json", ref _shaderDefinitions);
            Game.LoadDefinitionData("Textures/textureDefinitions.json", ref _textureDefinitions);
            Game.LoadDefinitionData("Textures/spriteDefinitions.json", ref _spriteDefinitions); 
            LoadDefinitions();


            var (returnStatus, returnObject) = ContentManager.Get<Sound>("8BitSmoothPresentation");

            if (returnStatus && returnObject is not null)
            {
                Game.BackgroundMusic = returnObject;
                Game.BackgroundMusic.UseSound();
            }

            SetColor(1, 1, 1, 1);
        }

        private static void SetColor(float R, float G, float B, float A)
        {
            var (returnStatus, returnObject) = ContentManager.Get<Shader>("BaseShader");
            if (!returnStatus || returnObject is null)
            {
                Logger.Log(Logger.Levels.Error, "Could not load shader");
                return;
            }

            Vector4 alphaColor = new(R, G, B, A);

            returnObject.SetVector4("color", ref alphaColor);
        }

        public override void Update(FrameEventArgs args)
        {
            if (Game.BackgroundMusic is not null && !Game.BackgroundMusic.playing &&yScroll > 7400)
            {
                Parent.SwitchScene(new Scenes.StartScene(Parent));
            }
        }
    }
}
