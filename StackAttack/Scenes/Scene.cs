using OpenTK.Windowing.Common;

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

        public abstract void Draw(FrameEventArgs args);

        public abstract void Dispose();
    }
}
