using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Widget;
using BitMobile.Application;
using BitMobile.Utilities.Translator;

namespace BitMobile.Droid.Screens
{
    public delegate void Progress(int total, int processed, string message);

    class ProgressScreen : InitialScreen
    {
        public ProgressScreen(BaseScreen activity, Settings settings)
            : base(activity, settings)
        {
        }

        public void Start()
        {
            Activity.FlipScreen(Resource.Layout.Progress);
            
            using (var progressTitle = Activity.FindViewById<TextView>(Resource.Id.progressTitleLoading))
            using (var caption = Activity.FindViewById<TextView>(Resource.Id.caption))
            using (var caption1 = Activity.FindViewById<TextView>(Resource.Id.caption1))
            using (var caption2 = Activity.FindViewById<TextView>(Resource.Id.caption2))
            using (var bottomText1 = Activity.FindViewById<TextView>(Resource.Id.bottomText1))
            using (var bottomText2 = Activity.FindViewById<TextView>(Resource.Id.bottomText2))
            using (var topImg = Activity.FindViewById<ImageView>(Resource.Id.imageViewTop))
            using (var bottomImg = Activity.FindViewById<ImageView>(Resource.Id.imageViewBottom))
            using (var lockImg = Activity.FindViewById<ImageView>(Resource.Id.lockImage))
            using (var logoImg = Activity.FindViewById<ImageView>(Resource.Id.logoImage))
            using (var topBitmap = (BitmapDrawable)topImg.Drawable)
            using (var botBitmap = (BitmapDrawable)bottomImg.Drawable)
            {
                progressTitle.Text = D.PLAESE_WAIT_DATA_IS_LOADED;
                caption.Text = D.BIT_MOBILE;

                int width = Activity.Resources.DisplayMetrics.WidthPixels;

                int topHeight = topBitmap.Bitmap.Height * width / topBitmap.Bitmap.Width;
                topImg.LayoutParameters.Width = width;
                topImg.LayoutParameters.Height = topHeight;

                int botHeight = botBitmap.Bitmap.Height * width / botBitmap.Bitmap.Width;
                bottomImg.LayoutParameters.Width = width;
                bottomImg.LayoutParameters.Height = botHeight;

                if (Settings.CurrentSolutionType != SolutionType.BitMobile)
                {
                    caption.Text = "";

                    Color baseColor = GetBaseColor();

                    lockImg.Drawable.SetColorFilter(baseColor, PorterDuff.Mode.SrcIn);
                    caption.Visibility = Android.Views.ViewStates.Invisible;
                    bottomText1.Text = D.EFFECTIVE_SOLUTIONS_BASED_ON_1C_FOR_BUSINESS;
                    bottomText2.Text = D.FIRST_BIT_COPYRIGHT;

                    SetBackground(topImg, bottomImg, caption1, caption2, logoImg);
                }
            }
        }

        public void Progress(int total, int processed, string message)
        {
            Activity.RunOnUiThread(() =>
                    {
                        using (var progressText = Activity.FindViewById<TextView>(Resource.Id.progressTextLoading))
                        using (var progressBar = Activity.FindViewById<ProgressBar>(Resource.Id.progressBarLoading))
                        {
                            progressText.Text = message;

                            if (total > 0)
                            {
                                progressBar.Indeterminate = false;

                                decimal proc = processed;
                                decimal tot = total;
                                decimal div = proc / tot;

                                var percent = (int)(div * 100);
                                progressBar.Progress = percent;

                                string text = BitBrowserApp.Current.Settings.DevelopModeEnabled
                                    ? string.Format(D.LOADING + "... {0} kb", processed / 1024)
                                    : string.Format(D.LOADING + "... {0} %", percent);

                                if (processed > 0)
                                    progressText.Text = text;
                            }
                            else
                                progressBar.Indeterminate = true;

                        }
                    });
        }

    }
}