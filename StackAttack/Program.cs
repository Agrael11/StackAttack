using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;
using StackAttack.Engine.Helpers;

namespace StackAttack
{
    public class Program
    {
        private static DebugProc _debugProcCallback = DebugCallback;
        private static GCHandle _debugProcCallbackHandle;
        private static Game? game;

        public static void Main()
        {
            Logger.Log(Logger.Levels.Info, "Loading Window");
            GameWindowSettings gameWindowSettings = new();
            NativeWindowSettings nativeWindowSettings = new();
            nativeWindowSettings.Size = new OpenTK.Mathematics.Vector2i(512, 512);
            nativeWindowSettings.WindowBorder = OpenTK.Windowing.Common.WindowBorder.Fixed;
            nativeWindowSettings.Title = "Stack Attack";
            using (game = new(gameWindowSettings, nativeWindowSettings))
            {
                try
                {
                    _debugProcCallbackHandle = GCHandle.Alloc(_debugProcCallback);
                    GL.DebugMessageCallback(_debugProcCallback, IntPtr.Zero);
                    GL.Enable(EnableCap.DebugOutput);
                    GL.Enable(EnableCap.DebugOutputSynchronous);
                    game.Run();
                }
                catch (Exception ex)
                {
                    Logger.Log(Logger.Levels.Fatal, ex.Message);
                    Environment.Exit(-1);
                }
            }
        }

        private static void DebugCallback(DebugSource source, DebugType type, int id,
    DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            try
            {
                if (severity == DebugSeverity.DontCare || severity == DebugSeverity.DebugSeverityNotification)
                    return;
                string messageString = Marshal.PtrToStringAnsi(message, length);
                Logger.Levels errorLevel = Logger.Levels.Info;
                switch (severity)
                {
                    case DebugSeverity.DebugSeverityHigh:
                        errorLevel = Logger.Levels.Fatal;
                        break;
                    case DebugSeverity.DebugSeverityMedium:
                        errorLevel = Logger.Levels.Error;
                        break;
                    case DebugSeverity.DebugSeverityLow:
                        errorLevel = Logger.Levels.Warn;
                        break;
                    case DebugSeverity.DebugSeverityNotification:
                        errorLevel = Logger.Levels.Info;
                        break;
                    case DebugSeverity.DontCare:
                        errorLevel = Logger.Levels.Info;
                        break;
                    default:
                        errorLevel = Logger.Levels.Info;
                        break;
                }
                Logger.Log(errorLevel, messageString);
            }
            catch
            {
                Environment.Exit(-1);
            }
        }
    }
}