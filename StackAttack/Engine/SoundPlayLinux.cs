using StackAttack.Engine.Helpers;
using System.Media;

namespace StackAttack.Engine
{
    public class SoundPlayLinux : SoundPlay, IDisposable
    {
        private static bool bassInit = false;
        private int Handle = -1;

        public SoundPlayLinux(string path, float volume, bool looping) : base(path, volume, looping)
        {
            if (!bassInit)
            {
                //ManagedBass.Bass.Init();
                //bassInit = true;
            }
        }

        public override void Init()
        {
            //Handle = ManagedBass.Bass.CreateStream(path);
        }

        public override void Play()
        {
            if (Looping)
            {
                //soundPlayer.PlayLooping();
            }
            else
            {
                // ManagedBass.Bass.Volume = Volume;
                //ManagedBass.Bass.ChannelPlay(Handle, true);
            }
        }

        public override void Dispose()
        {
        }

        public override bool Playing()
        {
            return false;//ManagedBass.Bass.ChannelIsActive(Handle) == ManagedBass.PlaybackState.Playing;
        }

        public override void Stop()
        {
            // ManagedBass.Bass.ChannelStop(Handle);
        }
    }
}
