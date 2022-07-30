using NAudio.Wave;
using StackAttack.Engine.Helpers;

namespace StackAttack.Engine
{
    public class Sound : IDisposable, ILoadable<Sound>
    {
        public struct SoundDefinition
        {
            public string SoundID { get; set; }
            public string FileName { get; set; }
            public bool Looping { get; set; } = false;
            public float Volume { get; set; } = 0.25f;

            public SoundDefinition(string soundID, string fileName, bool looping, float volume)
            {
                SoundID = soundID;
                FileName = fileName;
                Looping = looping;
                Volume = volume;
            }
        }

        //public string ID = "NullTexture";
        private string path = "";
        private WaveOutEvent waveout;
        private WaveFileReader reader;
        public float Volume { get; set; } = 0.5f;
        public bool Looping { get; set; }

        public Sound()
        {

        }

        public Sound? Load(string path)
        {
            this.path = path;
            return this;
        }

        public void Reload()
        {

        }

        public void UseSound()
        {
            reader = new(path);
            if (reader is null)
                return;
            if (waveout is not null)
            {
                waveout.Stop();
                waveout.Dispose();
            }
            waveout = new WaveOutEvent();
            waveout.Init(reader);
            waveout.Volume = Volume;
            waveout.PlaybackStopped += Waveout_PlaybackStopped;
            waveout.Play();
        }

        private void Waveout_PlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (Looping)
            {
                UseSound();
            }
        }

        public void Dispose()
        {
        }
    }
}
