using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using BitMobile.Application;
using BitMobile.Controls.StyleSheet;
using BitMobile.Droid;
using BitMobile.Droid.UI;
using BitMobile.Utilities.IO;

namespace BitMobile.Controls
{
    [Synonym("img")]
    // ReSharper disable UnusedMember.Global, MemberCanBePrivate.Global, UnusedAutoPropertyAccessor.Global
    class Image : Control<ImageView>, IApplicationContextAware, IImageContainer
    {
        IApplicationContext _applicationContext;
        Android.Graphics.Color? _selectedColor;

        public Image(BaseScreen activity)
            : base(activity)
        {
        }

        public string Source { get; set; }

        public override View CreateView()
        {
            _view = new ImageView(_activity);

            return _view;
        }

        public override Bound Apply(StyleSheet.StyleSheet stylesheet, Bound styleBound, Bound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            var style = stylesheet.GetHelper<StyleHelper>();

            // background image
            if (Source == null)
            {
                var background = style.Background(this, _applicationContext);
                _view.SetBackgroundDrawable(background);
            }

            //selected color
            _selectedColor = style.Color<SelectedColor>(this);

            return styleBound;
        }

        public override void AnimateTouch(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    if (_selectedColor != null && _view.Background != null)
                    {
                        _view.Background.SetColorFilter(_selectedColor.Value, PorterDuff.Mode.SrcIn);
                        _view.Invalidate();
                    }
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    if (_selectedColor != null && _view.Background != null)
                    {
                        _view.Background.ClearColorFilter();
                        _view.Invalidate();
                    }
                    break;
            }
        }

        #region IApplicationContextAware

        public void SetApplicationContext(object applicationContext)
        {
            _applicationContext = (IApplicationContext)applicationContext;
        }
        #endregion

        #region IImageContainer

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public bool InitializeImage(StyleSheet.StyleSheet stylesheet)
        {
            bool result = false;
            if (Source != null)
            {
                string path = FileSystemProvider.TranslatePath(_applicationContext.LocalStorage, Source);
                var img = (BitmapDrawable)Drawable.CreateFromPath(path);
                if (img != null)
                {
                    View.SetBackgroundDrawable(img);

                    ImageWidth = img.Bitmap.Width;
                    ImageHeight = img.Bitmap.Height;
                    result = true;
                }
            }
            else
                result = stylesheet.GetHelper<StyleHelper>().InitializeImageContainer(this, _applicationContext);

            return result;
        }
        #endregion
    }
}