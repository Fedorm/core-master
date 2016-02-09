using System;
using System.Collections.Generic;
using System.IO;
using BitMobile.Application.Exceptions;
using BitMobile.Application.IO;
using BitMobile.Application.Translator;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.UI;
using MonoTouch.Foundation;
using MonoTouch.MediaPlayer;
using MonoTouch.UIKit;

namespace BitMobile.IOS.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "MediaPlayer")]
    public class MediaPlayer : Control<UIView>
    {
        private MPMoviePlayerController _moviePlayer;
        private string _path;
        private bool _contentSet;

        public string Path
        {
            get { return _path; }
            set
            {
                if (value != _path)
                {
                    _path = value;
                    SetContentUrl();
                }
            }
        }

        public bool AutoPlay { get; set; }
        
        public bool Play()
        {
            if (_contentSet)
                _moviePlayer.Play();
            return _contentSet;
        }

        public void Pause()
        {
            _moviePlayer.Pause();
        }

        public void Stop()
        {
            _moviePlayer.Stop();
        }
        
        public override void CreateView()
        {
            _moviePlayer = new MPMoviePlayerController();
            _view = _moviePlayer.View;

            SetContentUrl();

            if (AutoPlay)
                Play();
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            return styleBound;
        }

        protected override void Dismiss()
        {
            if (_moviePlayer != null)
                _moviePlayer.Stop();
            DisposeField(ref _moviePlayer);
        }

        private void SetContentUrl()
        {
            _contentSet = false;
            if (_moviePlayer != null && Path != null)
            {
                try
                {
                    string path = IOContext.Current.TranslateLocalPath(Path);
                    if (File.Exists(path))
                    {
                        _moviePlayer.ContentUrl = NSUrl.FromFilename(path);
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
