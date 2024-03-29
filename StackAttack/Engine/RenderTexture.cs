﻿using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack.Engine
{
    public class RenderTexture
    {
        public string Name { get; set; }
        public string Shader 
        {
            get
            {
                if (Sprite.returnResult == false || Sprite.spriteResult is null)
                {
                    return "";
                }
                return Sprite.spriteResult.ShaderID;
            } 
            set 
            {
                if (Sprite.returnResult == false || Sprite.spriteResult is null)
                {
                    return;
                }
                Sprite.spriteResult.ShaderID = value;
            }
        }
        public int Width { get; set; }
        public int Height { get; set; }
        private readonly int frameBuffer;
        public (bool returnResult, Texture? textureResult) Texture => ContentManager.Get<Texture>(Name);
        public (bool returnResult, Sprite? spriteResult) Sprite => ContentManager.Get<Sprite>(Name);
        private readonly int renderHandle;
        private readonly string shader  = "";

        public RenderTexture()
        {
            Name = "";
        }

        public RenderTexture(int width, int height, string shader, string name = "")
        {
            Width = width;
            Height = height;
            this.shader = shader;
            Name = "renderTexture_" + name;
            if (string.IsNullOrWhiteSpace(name))
            {
                do
                {
                    Name = "renderTexture_" + new Random().Next(0, 99999);
                } while (ContentManager.ContainsKey<Texture>(Name));
            }

            frameBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
            renderHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, renderHandle);
            byte[] pixels = Array.Empty<byte>();
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            int depthrenderbuffer = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthrenderbuffer);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthrenderbuffer);
            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, renderHandle, 0);
            DrawBuffersEnum[] DrawBuffers = { DrawBuffersEnum.ColorAttachment0 };
            GL.DrawBuffers(1, DrawBuffers);
            ContentManager.Add(Name, new Texture(renderHandle, width, height));
            ContentManager.Add(Name, new Sprite(Name, shader, new Vector2i(0, 0), new Vector2i(width, height)));
        }

        public void Reregister()
        {
            ContentManager.Add(Name, new Texture(renderHandle, Width, Height));
            ContentManager.Add(Name, new Sprite(Name, shader, new Vector2i(0, 0), new Vector2i(Width, Height)));
        }

        public void Begin()
        {
            Game.SetViewport(0, 0, Width, Height, Width, Height);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer);
        }

        public static void End()
        {
            Game.SetViewport(0, 0, Game.WindowWidth, Game.WindowHeight, 64, 64);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
}
