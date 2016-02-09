using System;
using System.Collections.Generic;
using BitMobile.Application.IO;
using BitMobile.Application.StyleSheet;
using BitMobile.Common.Controls;
using BitMobile.Common.StyleSheet;
using BitMobile.IOS;
using BitMobile.UI;
using MonoTouch.UIKit;

namespace BitMobile.Controls
{
    [Synonym("img")]
    [MarkupElement(MarkupElementAttribute.ControlsNamespace, "Image")]
    public class Image : Control<UIImageView>
    {
        private UIImage _backgroundImage;
        private UIImage _selectedImage;

        public string Source { get; set; }

        public override void CreateView()
        {
            _view = new UIImageView();
        }

        public override void AnimateTouch(TouchEventType touch)
        {
            if (_selectedImage != null)
            {
                switch (touch)
                {
                    case TouchEventType.Begin:
                        _view.Image = _selectedImage;
                        break;
                    case TouchEventType.Cancel:
                    case TouchEventType.End:
                        UIView.BeginAnimations(null);
                        UIView.SetAnimationDuration(0.1);
                        _view.Image = _backgroundImage;
                        UIView.CommitAnimations();
                        break;
                }
            }
        }

        protected override IBound Apply(IStyleSheet stylesheet, IBound styleBound, IBound maxBound)
        {
            base.Apply(stylesheet, styleBound, maxBound);

            IStyleSheetHelper style = stylesheet.Helper;

            // background image
            _backgroundImage = FromSource() ?? stylesheet.GetBackgroundImage(this);

            if (_backgroundImage != null)
            {
                _view.Image = _backgroundImage;

                // selected-color
                UIColor selectedColor = style.SelectedColor(this).ToNullableColor();
                if (selectedColor != null)
                    _selectedImage = GetFilteredImage(_backgroundImage, selectedColor);
            }

            // size to content by background
            return GetBoundByImage(styleBound, maxBound, _backgroundImage);
        }

        protected override IBound ReApply(IDictionary<Type, IStyle> styles, IBound styleBound, IBound maxBound)
        {
            IStyleHelper helper = StyleSheetContext.Current.CreateHelper(styles, CurrentStyleSheet, this);

            // background image
            _backgroundImage = FromSource() ?? helper.Get<IBackgroundImage>().GetImage();

            if (_backgroundImage != null)
            {
                _view.Image = _backgroundImage;

                // selected-color
                ISelectedColor selectedColor;
                if (helper.TryGet(out selectedColor))
                {
                    UIColor color = selectedColor.ToNullableColor();
                    if (color != null)
                        _selectedImage = GetFilteredImage(_backgroundImage, selectedColor.ToNullableColor());
                }
            }

            // size to content by background
            return GetBoundByImage(styleBound, maxBound, _backgroundImage);
        }

        protected override void Dismiss()
        {
            DisposeField(ref _backgroundImage);
            DisposeField(ref _selectedImage);
        }

        private UIImage FromSource()
        {
            if (Source != null)
            {
                try
                {
                    return UIImage.FromFile(IOContext.Current.TranslateLocalPath(Source));
                }
                catch (Exception e)
                {
                    CurrentContext.HandleException(e);
                }
            }
            return null;
        }
    }
}