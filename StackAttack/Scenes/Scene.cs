using OpenTK.Windowing.Common;
using StackAttack.Engine;

namespace StackAttack.Scenes
{
    public abstract class Scene : IDisposable
    {
        internal Game Parent;

        public Scene(Game parent)
        {
            Parent = parent;
        }

        public abstract void Init();

        public abstract void Update(FrameEventArgs args);

        public abstract void Draw(FrameEventArgs args, ref RenderTexture texture);

        public abstract void Dispose();
    }
}
