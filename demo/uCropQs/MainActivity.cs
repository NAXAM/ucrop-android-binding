using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Java.Util;
using Com.Yalantis.Ucrop;
using Android.Support.V7.App;

namespace uCropQs
{
    [Activity(Label = "uCropQs", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private Button btnCrop;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            btnCrop = FindViewById<Button>(Resource.Id.btnCrop);

            btnCrop.Click += BtnCrop_Click;
        }

        private void BtnCrop_Click(object sender, System.EventArgs e)
        {
            string path = Android.OS.Environment.ExternalStorageDirectory.Path;
            var destFile = Android.Net.Uri.FromFile(new Java.IO.File(path, "SAMPLE_CROPPED_IMAGE_NAME.png"));
            var srcFile = Android.Net.Uri.Parse("https://unsplash.it/800/800/?random");

            UCrop uCrop = UCrop.Of(srcFile, destFile);

            uCrop.UseSourceImageAspectRatio();
            uCrop.WithAspectRatio(1, 1);

            uCrop.Start(this);
        }
    }
}

