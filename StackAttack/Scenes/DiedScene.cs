using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using StackAttack.Engine;
using StackAttack.Engine.Helpers;

namespace StackAttack.Scenes
{
    public class DiedScene : Scene
    {
        public DiedScene(Game parent) : base(parent)
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

        int timer = 0;
        private List<Shader.ShaderDefinition> _shaderDefinitions = new();
        private List<Texture.TextureDefinition> _textureDefinitions = new();
        private List<Sprite.SpriteDefinition> _spriteDefinitions = new();
        private List<Sound.SoundDefinition> _soundDefinitions = new();

        private RenderTexture? oldTexture = null;

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
            timer++;
            var (returnStatus, returnObject) = ContentManager.Get<Shader>("BaseShader");
            if (!returnStatus || returnObject is null)
            {
                Logger.Log(Logger.Levels.Error, "Could not load shader");
                return;
            }

            if (oldTexture is null)
            {
                oldTexture = new RenderTexture(64, 64, "BaseShader");
                oldTexture.Begin();
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.ClearColor(0f, 0f, 0f, 1f);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                if (texture.Sprite.returnResult && texture.Sprite.spriteResult is not null)
                {
                    texture.Sprite.spriteResult.Draw(new Rectanglei(0,0,64,64));
                }
                RenderTexture.End();
            }

            texture.Begin();
            
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            GL.ClearColor(0f, 0f, 0f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            float time = 1 - (timer / 20f);
            if (time > 1) time = 1;
            if (time < 0) time = 0;
            float time2 = 0.1f + time * 0.9f;
            Vector4 alphaColor = new Vector4(1, 1, 1, time2);

            returnObject.SetVector4("color", ref alphaColor);

            if (oldTexture is not null && oldTexture.Sprite.returnResult && oldTexture.Sprite.spriteResult is not null)
            {
                oldTexture.Sprite.spriteResult.Draw(new Rectanglei(0, 0, 64, 64));
            }

            time = (timer / 20f);
            if (time > 1) time = 1;
            if (time < 0) time = 0;

            alphaColor = new Vector4(1, 1, 1, time);

            returnObject.SetVector4("color", ref alphaColor);

            Sprite.Draw("YouDied", new Rectangle(0,0,64,64));
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

            var (returnStatus, returnObject) = ContentManager.Get<Shader>("BaseShader");
            if (!returnStatus || returnObject is null)
            {
                Logger.Log(Logger.Levels.Error, "Could not load shader");
                return;
            }

            Vector4 alphaColor = new Vector4(1,1,1,1);

            returnObject.SetVector4("color", ref alphaColor);

        }

        public override void Update(FrameEventArgs args)
        {
            if (timer == 120)
            {
                GameScene gameScene = new(Parent)
                {
                    LoadLevel = Parent.currentLevel
                };
                gameScene.HP = 100;
                Parent.SwitchScene(gameScene);
            }
        }
    }
}
