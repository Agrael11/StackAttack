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

        public bool playing = false;
        public SoundPlay? play;
        public float Volume { get; set; } = 0.5f;
        public bool Looping { get; set; }

        public Sound()
        {

        }

        public void Update()
        {
            if (play is null)
                return;

            if (Looping)
            {
                if (playing && !play.Playing())
                {
                    play.Play();
                }
            }
            else
            {
                if (!play.Playing())
                {
                    playing = false;
                }
            }
        }

        public Sound? Load(string path)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                play = new SoundPlayLinux(path, Volume, Looping);
            }
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    play = new SoundPlayVista(path, Volume, Looping);
                }
                else
                {
                    play = new SoundPlayPreVista(path, Volume, Looping);
                }
            }
            else
            {
                play = new SoundPlayNone(path, Volume, Looping);
            }
            play.Init();
            return this;
        }

        public void UseSound()
        {
            if (play is null)
                return;

            playing = true;
            play.Play();
        }


        public void StopUseSound()
        {
            if (play is null)
                return;

            playing = false;
            play.Stop();
        }

        public void Dispose()
        {
            if (play is null)
                return;

            play.Dispose();
        }
    }
}
