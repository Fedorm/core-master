using System;
using System.Drawing;
using MonoTouch.UIKit;

namespace BitMobile.IOS
{
    public class DatePicker
    {
        private static DatePicker _current;

        private readonly UIButton _cancelButton = UIButton.FromType(UIButtonType.RoundedRect);
        private readonly UIButton _doneButton = UIButton.FromType(UIButtonType.RoundedRect);
        private readonly UIView _layout = new UIView();
        private readonly UILabel _titleLabel = new UILabel();
        private UIDatePicker _datePicker = new UIDatePicker(RectangleF.Empty);

        public DatePicker()
        {
            _current = this;

            _titleLabel.BackgroundColor = UIColor.GroupTableViewBackgroundColor;
            _titleLabel.TextColor = UIColor.DarkTextColor;
            _titleLabel.Font = UIFont.BoldSystemFontOfSize(18);
            _titleLabel.LineBreakMode = UILineBreakMode.TailTruncation;

            _datePicker.BackgroundColor = UIColor.GroupTableViewBackgroundColor;

            _layout.BackgroundColor = UIColor.GroupTableViewBackgroundColor;

            _layout.AddSubview(_datePicker);
            _layout.AddSubview(_titleLabel);
            _layout.AddSubview(_doneButton);
            _layout.AddSubview(_cancelButton);
        }

        public UIDatePicker NativeDatePicker
        {
            get { return _datePicker; }
            set { _datePicker = value; }
        }

        public string DoneTitle { get; set; }

        public string CancelTitle { get; set; }

        public string Title
        {
            get { return _titleLabel.Text; }
            set { _titleLabel.Text = value; }
        }

        public static void CancelCurrent()
        {
            if (_current != null)
            {
                if (_current.Click != null)
                    _current.Click(_current, new UIButtonEventArgs(1));
                _current.Close();
                _current = null;
            }
        }

        public event EventHandler<UIButtonEventArgs> Click;

        public void Show(UIView owner, ScreenController controller)
        {
            _doneButton.SetTitle(DoneTitle, UIControlState.Normal);
            _doneButton.SetTitleColor(UIColor.DarkTextColor, UIControlState.Normal);
            _cancelButton.SetTitle(CancelTitle, UIControlState.Normal);
            _cancelButton.SetTitleColor(UIColor.DarkTextColor, UIControlState.Normal);

            float titleBarHeight = 40;
            float margin = 10;
            var doneButtonSize = new SizeF(71, 30);
            var cancelButtonSize = new SizeF(71, 30);
            var layoutSize = new SizeF(owner.Frame.Width, _datePicker.Frame.Height + titleBarHeight);
            var actionSheetFrame = new RectangleF(0, owner.Frame.Height - layoutSize.Height
                , layoutSize.Width, layoutSize.Height);

            _layout.Frame = actionSheetFrame;
            _datePicker.Frame = new RectangleF(_datePicker.Frame.X, titleBarHeight, layoutSize.Width,
                _datePicker.Frame.Height);
            _titleLabel.Frame = new RectangleF(margin, 4,
                owner.Frame.Width - doneButtonSize.Width - cancelButtonSize.Width - 5 * margin, 35);

            _cancelButton.SizeToFit();
            _doneButton.SizeToFit();

            _cancelButton.Frame =
                new RectangleF(layoutSize.Width - doneButtonSize.Width - cancelButtonSize.Width - margin, 7,
                    cancelButtonSize.Width, cancelButtonSize.Height);
            _cancelButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                if (Click != null)
                    Click(this, new UIButtonEventArgs(1));
                Close();
            };

            _doneButton.Frame = new RectangleF(layoutSize.Width - doneButtonSize.Width - margin, 7, doneButtonSize.Width,
                doneButtonSize.Height);
            _doneButton.TouchUpInside += (object sender, EventArgs e) =>
            {
                if (Click != null)
                    Click(this, new UIButtonEventArgs(0));
                Close();
            };

            ScreenController.HackFieldForDateTimePicker.InputView = _layout;
            owner.AddSubview(ScreenController.HackFieldForDateTimePicker);
            ScreenController.HackFieldForDateTimePicker.BecomeFirstResponder();
        }

        private void Close()
        {
            if (_current != null)
            {
                ScreenController.HackFieldForDateTimePicker.EndEditing(true);

                ScreenController.HackFieldForDateTimePicker.InputView = null;

                _current._layout.Dispose();
                _current._doneButton.Dispose();
                _current._cancelButton.Dispose();
                _current._titleLabel.Dispose();
                _current._datePicker.Dispose();
            }
        }
    }
}