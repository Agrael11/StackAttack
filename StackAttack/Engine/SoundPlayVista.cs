using NAudio.Wave;
using StackAttack.Engine.Helpers;

namespace StackAttack.Engine
{
    public class SoundPlayVista : SoundPlay, IDisposable
    {
        private WaveFileReader reader;
        private WasapiOut wasapiout;

        public SoundPlayVista(string path, float volume, bool looping) : base(path, volume, looping)
        {
            reader = new(path);
            wasapiout = new WasapiOut();
        }

        public override void Init()
        {
        }

        public override void Play()
        {
            if (wasapiout != null)
            {
                reader.Dispose();
                wasapiout.Stop();
                wasapiout.Dispose();
            }
            reader = new(path);
            wasapiout = new WasapiOut();
            wasapiout.Init(reader);
            wasapiout.Volume = Volume;
            wasapiout.PlaybackStopped += Waveout_PlaybackStopped;
            wasapiout.Play();
        }

        private void Waveout_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (Looping)
            {
                Play();
            }
        }

        public override void Dispose()
        {
            wasapiout.Dispose();
            reader.Dispose();
        }

        public override void Stop()
        {
            wasapiout.Stop();
        }

        public override bool Playing()
        {
            return wasapiout.PlaybackState == PlaybackState.Playing;
        }
    }
}
