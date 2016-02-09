using System;
using System.Collections.Generic;
using System.IO;
using Android.Widget;
using BitMobile.Application.Exceptions;
using BitMobile.Application.IO;
using BitMobile.Application.Translator;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Droid.UI;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "MediaPlayer")]
    public class MediaPlayer : Control<VideoView>
    {
        private MediaController _mediaController;
        private string _path;
        private bool _contentSet;
        private bool _playbackAllowed;

        public MediaPlayer(BaseScreen activity)
            : base(activity)
        {
        }

        public string Path
        {
            get { return _path; }
            set
            {
                if (value != _path)
                {
                    _path = value;
                    SetVideoPath();
                }
            }
        }

        public bool AutoPlay { get; set; }

        public override void CreateView()
        {
            _view = new VideoView(Activity);
            _mediaController = new MediaController(Activity);
            _view.SetMediaController(_mediaController);

            SetVideoPath();

            if (AutoPlay)
                Play();

            _view.RequestFocus();
        }

        public bool Play()
        {
            if (_contentSet)
            {
                if (!_playbackAllowed)
                    SetVideoPath();

                _view.Start();
                _playbackAllowed = true;

            }
            return _contentSet;
        }

        public void Pause()
        {
            _view.Pause();
        }

        public void Stop()
        {
            _view.StopPlayback();
            _playbackAllowed = false;
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            return styleBound;
        }

        protected override void Dismiss()
        {
            _mediaController.Dispose();
            base.Dismiss();
        }

        private void SetVideoPath()
        {
            _contentSet = false;
            if (_view != null && Path != null)
            {
                try
                {
                    string path = IOContext.Current.TranslateLocalPath(Path);
                    if (File.Exists(path))
                    {
                        _view.SetVideoPath(path);
                        _contentSet = true;
                    }
                    else
                        throw new NonFatalException(D.FILE_NOT_EXISTS);
                }
                catch (Exception e)
                {
                    CurrentContext.HandleException(e);
                }
            }
        }
    }
}