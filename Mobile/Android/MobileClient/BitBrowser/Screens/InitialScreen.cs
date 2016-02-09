using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Widget;
using BitMobile.Application;
using BitMobile.Utilities.Translator;

namespace BitMobile.Droid.Screens
{
    abstract class InitialScreen
    {
        protected readonly BaseScreen Activity;
        protected readonly Settings Settings;
        private List<Bitmap> _bitmaps = new List<Bitmap>();

        protected InitialScreen(BaseScreen activity, Settings settings)
        {
            Activity = activity;
            Settings = settings;
        }

        public void Recycle()
        {
            foreach (Bitmap bitmap in _bitmaps)
            {
                bitmap.Recycle();
                bitmap.Dispose();
            }
            _bitmaps.Clear();
        }

        protected void SetBackground(ImageView topImg, ImageView bottomImg, TextView caption1
            , TextView caption2, ImageView logoImg)
        {
            switch (Settings.CurrentSolutionType)
            {
                case SolutionType.SuperAgent:
                    using (Bitmap topImgBitmap = DecodeBitmap(Resource.Drawable.SuperAgentTop))
                        topImg.SetImageBitmap(topImgBitmap);
                    using (Bitmap bottomImgBitmap = DecodeBitmap(Resource.Drawable.SuperAgentBottom))
                        bottomImg.SetImageBitmap(bottomImgBitmap);
                    caption1.Text = D.BIT_CATCH1;
                    caption2.Text = D.BIT_CATCH2;
                    using (Bitmap logoImgBitmap = DecodeBitmap(Resource.Drawable.SuperAgentLogo))
                        logoImg.SetImageBitmap(logoImgBitmap);
                    break;

                case SolutionType.SuperService:
                    using (Bitmap topImgBitmap = DecodeBitmap(Resource.Drawable.SuperServiceTop))
                        topImg.SetImageBitmap(topImgBitmap);
                    using (Bitmap bottomImgBitmap = DecodeBitmap(Resource.Drawable.SuperServiceBottom))
                        bottomImg.SetImageBitmap(bottomImgBitmap);
                    caption1.Text = D.SUPER_SERVICE1;
                    caption2.Text = D.SUPER_SERVICE2;
                    using (Bitmap logoImgBitmap = DecodeBitmap(Resource.Drawable.SuperServiceLogo))
                        logoImg.SetImageBitmap(logoImgBitmap);
                    break;

                case SolutionType.LandSuperService:
                    using (Bitmap topImgBitmap = DecodeBitmap(Resource.Drawable.SuperServiceTop))
                        topImg.SetImageBitmap(topImgBitmap);
                    using (Bitmap bottomImgBitmap = DecodeBitmap(Resource.Drawable.SuperServiceBottom))
                        bottomImg.SetImageBitmap(bottomImgBitmap);
                    using (Bitmap logoImgBitmap = DecodeBitmap(Resource.Drawable.LandSuperServiceLogo))
                        logoImg.SetImageBitmap(logoImgBitmap);
                    ((LinearLayout.LayoutParams)logoImg.LayoutParameters).Weight = 1;
                    using (var specialfor = Activity.FindViewById<ImageView>(Resource.Id.specialfor))
                    using (Bitmap specialforBitmap = DecodeBitmap(Resource.Drawable.Specialfor))
                        specialfor.SetImageBitmap(specialforBitmap);
                    using (var clientLogo = Activity.FindViewById<ImageView>(Resource.Id.clientLogo))
                    using (Bitmap clientLogoBitmap = DecodeBitmap(Resource.Drawable.LandLogo))
                        clientLogo.SetImageBitmap(clientLogoBitmap);
                    break;

                default:
                    throw new NotImplementedException("Current solution type is not supported");
            }
        }

        protected Color GetBaseColor()
        {
            Color color;
            switch (Settings.CurrentSolutionType)
            {
                case SolutionType.BitMobile:
                    color = new Color(210, 0, 126);
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

        private Bitmap DecodeBitmap(int resId)
        {
            Bitmap bitmap = Activity.DecodeBitmap(resId);
            if (!_bitmaps.Contains(bitmap))
                _bitmaps.Add(bitmap);
            return bitmap;
        }
    }
}