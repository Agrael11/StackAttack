using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StackAttack
{
    public class Shader :IDisposable, ILoadable<Shader>
    {
        public int Handle { get; private set; }

        public Shader? Load(string Path)
        {
            int VertexShader;
            int FragmentShader;

            string VertexShaderSource = "";
            string FragmentShaderSource = "";

            try
            {
                VertexShaderSource = File.ReadAllText(Path + ".vert");
                FragmentShaderSource = File.ReadAllText(Path + ".frag");
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.Levels.Error, ex.Message);
                return null;
            }

            int success = -1;

            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);
            GL.CompileShader(VertexShader);
            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(VertexShader);
                Logger.Log(Logger.Levels.Error, infoLog);
                return null;
            }

            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);
            GL.CompileShader(FragmentShader);
            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(FragmentShader);
                Logger.Log(Logger.Levels.Error, infoLog);
                return null;
            }

            int handle = GL.CreateProgram();
            GL.AttachShader(handle, VertexShader);
            GL.AttachShader(handle, FragmentShader);

            GL.LinkProgram(handle);
            GL.GetProgram(handle, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(handle);
                Logger.Log(Logger.Levels.Error, infoLog);
                return null;
            }

            GL.DetachShader(handle, VertexShader);
            GL.DetachShader(handle, FragmentShader);
            GL.DeleteBuffer(VertexShader);
            GL.DeleteBuffer(FragmentShader);

            Shader shader = new Shader() { Handle = handle };
            return shader;
        }

        public int GetAttribLocation(string attrib)
        {
            return GL.GetAttribLocation(Handle, attrib);
        }

        public void UseShader()
        {
            GL.UseProgram(Handle);
        }

        public void SetInt(string name, int value)
        {
            int location = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(location, value);
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }
    }
}
