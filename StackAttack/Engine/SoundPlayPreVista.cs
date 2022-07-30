using NAudio.Wave;
using StackAttack.Engine.Helpers;

namespace StackAttack.Engine
{
    public class SoundPlayPreVista : SoundPlay, IDisposable
    {
        private WaveFileReader reader;
        private WaveOutEvent waveoutevent;

        public SoundPlayPreVista(string path, float volume, bool looping) : base(path, volume, looping)
        {
            reader = new(path);
            waveoutevent = new WaveOutEvent();
        }

        public override void Init()
        {
        }

        public override void Play()
        {
            if (waveoutevent != null)
            {
                reader.Dispose();
                waveoutevent.Stop();
                waveoutevent.Dispose();
            }
            reader = new(path);
            waveoutevent = new WaveOutEvent();
            waveoutevent.Init(reader);
            waveoutevent.Volume = Volume;
            waveoutevent.PlaybackStopped += Waveout_PlaybackStopped;
            waveoutevent.Play();
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
            waveoutevent.Dispose();
            reader.Dispose();
        }

        public override void Stop()
        {
            waveoutevent.Stop();
        }

        public override bool Playing()
        {
            return waveoutevent.PlaybackState == PlaybackState.Playing;
        }
    }
}
