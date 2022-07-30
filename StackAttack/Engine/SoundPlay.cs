using NAudio.Wave;
using StackAttack.Engine.Helpers;

namespace StackAttack.Engine
{
    public abstract class SoundPlay:IDisposable
    {
        internal string path = "";
        public float Volume { get; set; } = 0.5f;
        public bool Looping { get; set; }

        protected SoundPlay(string path, float volume, bool looping)
        {
            this.path = path;
            Volume = volume;
            Looping = looping;
        }

        public abstract void Init();

        public abstract void Play();

        public abstract void Stop();

        public abstract bool Playing();

        public abstract void Dispose();
    }
}
