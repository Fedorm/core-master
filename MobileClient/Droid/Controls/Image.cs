using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using BitMobile.Application.IO;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.Droid.StyleSheet;
using BitMobile.Droid.UI;

namespace BitMobile.Droid.Controls
{
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "Image")]
    [Synonym("img")]
    // ReSharper disable UnusedMember.Global, MemberCanBePrivate.Global, UnusedAutoPropertyAccessor.Global
    class Image : Control<ImageView>
    {
        Color? _selectedColor;

        public Image(BaseScreen activity)
            : base(activity)
        {
        }

        public string Source { get; set; }

        public override void CreateView()
        {
            _view = new ImageView(Activity);
        }

        public override void AnimateTouch(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    if (_selectedColor != null && _view.Background != null)
                        _view.Background.SetColorFilter(_selectedColor.Value, PorterDuff.Mode.SrcIn);

                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    if (_selectedColor != null && _view.Background != null)
                        _view.Background.ClearColorFilter();
                    break;
            }
        }

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            return ApplyInternal(CurrentStyleSheet, styleBound, maxBound);
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            return ApplyInternal(CurrentStyleSheet, styleBound, maxBound);
        }

        private bool SetFromSource(IBound styleBound)
        {
            if (Source != null)
            {
                try
                {
                    string path = IOContext.Current.TranslateLocalPath(Source);
                    using (var img = Helper.LoadBitmap(path, styleBound.Width, styleBound.Height, true))
                        if (img != null)
                        {
                            using (var background = new BitmapDrawable(img))
                                SetBackground(background);
                            return true;
                        }
                }
                catch (Exception e)
                {
                    CurrentContext.HandleException(e);
                }
            }
            return false;
        }

        private IBound ApplyInternal(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            var style = stylesheet.Helper;

            // background image
            if (!SetFromSource(styleBound))
                using (var background = stylesheet.Background(this, styleBound))
                    SetBackground(background);

            //selected color
            _selectedColor = style.SelectedColor(this).ToNullableColor();

            return GetBoundByBackgroud(styleBound, maxBound);
        }

    }
}