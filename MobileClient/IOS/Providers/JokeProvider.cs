using System;
using System.Collections.Generic;
using MonoTouch.AVFoundation;
using MonoTouch.Foundation;

namespace BitMobile.IOS
{
    public class JokeProvider
    {
        private readonly string _syncSound;
        private readonly List<string> _tapSounds;
        private AVAudioPlayer _player;

        public JokeProvider()
        {
            _tapSounds = new List<string>
            {
                "Cartoon_sound_effects_boing.wav",
                "Cartoon_sound_effects_bong_4.wav",
                "Cartoon_sound_effects_bongs_1.wav",
                "Cartoon_sound_effects_zip_4.wav",
                "Comedy_effect_kiss.wav",
                "Comedy_effect_spring_3.wav",
                "Comedy_effect_squeak_and_pop.wav",
                "Comical_sound_series_boing_spring_2.wav",
                "Comical_sounds_1.wav",
                "Comical_sounds_2.wav"
            };
            _syncSound = "Comical_sound_series_bongs_1.wav";
        }

        public bool IsJokeDay
        {
            get { return DateTime.Now.Month == 4 && DateTime.Now.Day == 1; }
        }

        public void OnTap()
        {
            string fileName = _tapSounds[new Random().Next(_tapSounds.Count - 1)];
            Play(fileName);
        }

        public void OnSync()
        {
            Play(_syncSound);
        }

        private void Play(string fileName)
        {
            if (IsJokeDay)
                try
                {
                    if (_player != null)
                    {
                        _player.Stop();
                        _player.Dispose();
                    }
                    NSUrl ulr = NSUrl.FromFilename(fileName);
                    _player = AVAudioPlayer.FromUrl(ulr);
                    _player.Play();
                }
                catch
                {
                }
        }
    }
}