using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StackAttack.Engine;
using StackAttack.Engine.Helpers;

namespace StackAttack.Scenes
{
    public class StartScene : Scene
    {
        public StartScene(Game parent) : base(parent)
        {
        }

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

            Sprite.Draw("NewGame", new Rectangle(0,0,64,64));
            RenderTexture.End();
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


            var (returnStatus, returnObject) = ContentManager.Get<Sound>("8BitMenu");

            if (returnStatus && returnObject is not null)
            {
                Game.BackgroundMusic = returnObject;
                Game.BackgroundMusic.UseSound();
            }

            var baseShader = ContentManager.Get<Shader>("BaseShader");
            if (!baseShader.returnStatus || baseShader.returnObject is null)
            {
                Logger.Log(Logger.Levels.Error, "Could not load shader");
                return;
            }

            Vector4 alphaColor = new(1,1,1,1);

            baseShader.returnObject.SetVector4("color", ref alphaColor);

        }

        public override void Update(FrameEventArgs args)
        {
            KeyboardState state = Parent.KeyboardState.GetSnapshot();
            if (state.IsAnyKeyDown)
            {
                GameScene gameScene = new(Parent)
                {
                    LoadLevel = Parent.currentLevel
                };
                Parent.SwitchScene(gameScene);
            }
        }
    }
}
