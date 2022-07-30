using NAudio.Wave;
using StackAttack.Engine.Helpers;

namespace StackAttack.Engine
{
    public class SoundPlayNone : SoundPlay, IDisposable
    {
        private bool playing = false;

        public SoundPlayNone(string path, float volume, bool looping) : base(path, volume, looping)
        {
        }

        public override void Init()
        {
        }

        public override void Play()
        {
            playing = Looping;
        }

        public override void Dispose()
        {
        }

        public override void Stop()
        {
            playing = false;
        }

        public override bool Playing()
        {
            return playing;
        }
    }
}
