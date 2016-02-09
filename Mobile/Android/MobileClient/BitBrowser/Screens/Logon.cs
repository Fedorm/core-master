using System;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using BitMobile.Application;
using BitMobile.Utilities.Translator;

namespace BitMobile.Droid.Screens
{
    class LogonScreen : InitialScreen
    {
        readonly Action _resultCallback;

        public LogonScreen(BaseScreen activity, Settings settings, Action resultCallback)
            : base(activity, settings)
        {
            _resultCallback = resultCallback;
        }

        public void Start(string message, string exceptionMessage = null)
        {
            Activity.FlipScreen(Resource.Layout.Logon);
            
            using (var userName = Activity.FindViewById<EditText>(Resource.Id.userName))
            using (var password = Activity.FindViewById<EditText>(Resource.Id.password))
            using (var loginButton = Activity.FindViewById<TextView>(Resource.Id.login))
            using (var demoButton = Activity.FindViewById<TextView>(Resource.Id.demo))
            using (var caption = Activity.FindViewById<TextView>(Resource.Id.caption))
            using (var caption1 = Activity.FindViewById<TextView>(Resource.Id.caption1))
            using (var caption2 = Activity.FindViewById<TextView>(Resource.Id.caption2))
            using (var bottomText1 = Activity.FindViewById<TextView>(Resource.Id.bottomText1))
            using (var bottomText2 = Activity.FindViewById<TextView>(Resource.Id.bottomText2))
            using (var messageText = Activity.FindViewById<TextView>(Resource.Id.loginMessage))
            using (var topImg = Activity.FindViewById<ImageView>(Resource.Id.imageViewTop))
            using (var bottomImg = Activity.FindViewById<ImageView>(Resource.Id.imageViewBottom))
            using (var lockImg = Activity.FindViewById<ImageView>(Resource.Id.lockImage))
            using (var logoImg = Activity.FindViewById<ImageView>(Resource.Id.logoImage))
            using (var topBitmap = (BitmapDrawable)topImg.Drawable)
            using (var botBitmap = (BitmapDrawable)bottomImg.Drawable)
            {
                userName.Hint = D.USER_NAME;
                password.Hint = D.PASSWORD;
                loginButton.Text = D.LOGON;
                demoButton.Text = D.DEMO;
                caption.Text = D.BIT_MOBILE;

                int width = Activity.Resources.DisplayMetrics.WidthPixels;

                int topHeight = topBitmap.Bitmap.Height * width / topBitmap.Bitmap.Width;
                topImg.LayoutParameters.Width = width;
                topImg.LayoutParameters.Height = topHeight;

                int botHeight = botBitmap.Bitmap.Height * width / botBitmap.Bitmap.Width;
                bottomImg.LayoutParameters.Width = width;
                bottomImg.LayoutParameters.Height = botHeight;

                const int radius = 10;

                Color baseColor = GetBaseColor();

                using (var shape = new GradientDrawable())
                {
                    shape.SetShape(ShapeType.Rectangle);
                    shape.SetColor(Color.Transparent);
                    shape.SetCornerRadius(radius);
                    shape.SetStroke(2, baseColor);
                    userName.SetBackgroundDrawable(shape);
                    password.SetBackgroundDrawable(shape);
                }

                using (var shape = new GradientDrawable())
                {
                    shape.SetShape(ShapeType.Rectangle);
                    shape.SetCornerRadius(radius);
                    shape.SetStroke(0, Color.Transparent);
                    Color buttonColor = GetLoginButtonColor();
                    shape.SetColor(buttonColor);
                    loginButton.SetBackgroundDrawable(shape);
                }

                using (var shape = new GradientDrawable())
                {
                    shape.SetShape(ShapeType.Rectangle);
                    shape.SetCornerRadius(radius);
                    shape.SetStroke(0, Color.Transparent);
                    Color buttonColor = GetDemoButtonColor();
                    shape.SetColor(buttonColor);
                    demoButton.SetBackgroundDrawable(shape);
                }

                messageText.Text = message;
                if (exceptionMessage != null)
                    messageText.SetTextColor(new Color(247, 71, 71));

                userName.Text = Settings.UserName;
                password.Text = Settings.DevelopModeEnabled || exceptionMessage != null ? Settings.Password : "";

                loginButton.Click += LoginButtonClick;
                demoButton.Click += DemoButtonOnClick;
                demoButton.Visibility = Settings.DemoEnabled ? ViewStates.Visible : ViewStates.Gone;

                if (Settings.CurrentSolutionType != SolutionType.BitMobile)
                {
                    caption.Text = "";

                    using (Drawable d = userName.GetCompoundDrawables()[0])
                        d.SetColorFilter(baseColor, PorterDuff.Mode.SrcIn);
                    using (Drawable d = password.GetCompoundDrawables()[0])
                        d.SetColorFilter(baseColor, PorterDuff.Mode.SrcIn);
                    lockImg.Drawable.SetColorFilter(baseColor, PorterDuff.Mode.SrcIn);
                    caption.Visibility = ViewStates.Invisible;
                    bottomText1.Text = D.EFFECTIVE_SOLUTIONS_BASED_ON_1C_FOR_BUSINESS;
                    bottomText2.Text = D.FIRST_BIT_COPYRIGHT;

                    SetBackground(topImg, bottomImg, caption1, caption2, logoImg);
                }
            }

            if (exceptionMessage != null && Settings.DevelopModeEnabled)
                using (var builder = new AlertDialog.Builder(Activity))
                {
                    builder.SetTitle(message);
                    builder.SetMessage(exceptionMessage);
                    builder.SetPositiveButton(D.OK, (sender, e) => { });
                    builder.Show();
                }
        }

        private void LoginButtonClick(object sender, EventArgs e)
        {
            using (var userName = Activity.FindViewById<EditText>(Resource.Id.userName))
            using (var password = Activity.FindViewById<EditText>(Resource.Id.password))
            {
                if (userName != null && password != null)
                    if (!(String.IsNullOrEmpty(userName.Text) || String.IsNullOrEmpty(password.Text)))
                    {
                        Settings.UserName = userName.Text;
                        Settings.Password = password.Text;
                        Settings.WriteSettings();

                        _resultCallback();
                    }
                    else
                        Toast.MakeText(Activity, D.AUTORIZATION_DATA_CANNOT_BE_EMPTY, ToastLength.Long).Show();
            }
        }

        private void DemoButtonOnClick(object sender, EventArgs eventArgs)
        {
            using (var userName = Activity.FindViewById<EditText>(Resource.Id.userName))
            using (var password = Activity.FindViewById<EditText>(Resource.Id.password))
            {
                if (userName != null && password != null)
                {
                    userName.Text = Settings.DemoUserName;
                    password.Text = Settings.DemoPassword;

                    Settings.UserName = Settings.DemoUserName;
                    Settings.Password = Settings.DemoPassword;
                    Settings.WriteSettings();
                    _resultCallback();
                }
            }
        }

        private Color GetLoginButtonColor()
        {
            Color color;
            switch (Settings.CurrentSolutionType)
            {
                case SolutionType.BitMobile:
                    color = new Color(42, 45, 135);
                    break;
                case SolutionType.SuperAgent:
                    color = new Color(67, 172, 253);
                    break;
                case SolutionType.SuperService:
                case SolutionType.LandSuperService:
                    color = new Color(230, 137, 27);
                    break;
                default:
                    throw new NotImplementedException("Current solution type is not supported");
            }
            return color;
        }

        private Color GetDemoButtonColor()
        {
            return new Color(183, 183, 183);
        }
    }
}