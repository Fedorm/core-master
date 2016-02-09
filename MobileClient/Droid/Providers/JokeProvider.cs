using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Media;

namespace BitMobile.Droid.Providers
{
    class JokeProvider
    {
        private readonly BaseScreen _baseActivity;
        private readonly SoundPool _soundPool;
        private readonly List<int> _tapSounds = new List<int>();
        private readonly bool _loaded;
        private int _syncSound;

        public JokeProvider(BaseScreen baseActivity)
        {
            _baseActivity = baseActivity;
            if (IsJokeDay)
                try
                {
                    _soundPool = new SoundPool(5, Stream.Alarm, 0);
                    InitSounds();
                    _loaded = true;
                }
                catch
                {
                }
        }

        public bool IsJokeDay
        {
            get { return DateTime.Now.Month == 4 && DateTime.Now.Day == 1; }
        }

        public void OnTap()
        {
            if (_loaded && IsJokeDay)
                try
                {
                    _soundPool.Play(new Random().Next(_tapSounds.Count - 1), 1, 1, 0, 0, 1);
                }
                catch { }
        }

        public void OnSync()
        {
            if (_loaded && IsJokeDay)
                try
                {
                    _soundPool.Play(_syncSound, 1, 1, 0, 0, 1);
                }
                catch { }
        }

        private void InitSounds()
        {
            _tapSounds.Add(_soundPool.Load(_baseActivity, Resource.Raw.Cartoon_sound_effects_boing, 1));
            _tapSounds.Add(_soundPool.Load(_baseActivity, Resource.Raw.Cartoon_sound_effects_bong_4, 1));
            _tapSounds.Add(_soundPool.Load(_baseActivity, Resource.Raw.Cartoon_sound_effects_bongs_1, 1));
            _tapSounds.Add(_soundPool.Load(_baseActivity, Resource.Raw.Cartoon_sound_effects_zip_4, 1));
            _tapSounds.Add(_soundPool.Load(_baseActivity, Resource.Raw.Comedy_effect_kiss, 1));
            _tapSounds.Add(_soundPool.Load(_baseActivity, Resource.Raw.Comedy_effect_spring_3, 1));
            _tapSounds.Add(_soundPool.Load(_baseActivity, Resource.Raw.Comedy_effect_squeak_and_pop, 1));
            _tapSounds.Add(_soundPool.Load(_baseActivity, Resource.Raw.Comical_sound_series_boing_spring_2, 1));
            _tapSounds.Add(_soundPool.Load(_baseActivity, Resource.Raw.Comical_sounds_1, 1));
            _tapSounds.Add(_soundPool.Load(_baseActivity, Resource.Raw.Comical_sounds_2, 1));
            _tapSounds.Add(_soundPool.Load(_baseActivity, Resource.Raw.Comical_sounds_2, 1));

            _syncSound = _soundPool.Load(_baseActivity, Resource.Raw.Comical_sound_series_bongs_1, 1);
        }

    }
}