using Android.App;
using Android.Widget;
using Android.OS;
using System;
using Java.Util;
using Com.Yalantis.Ucrop;
using Android.Support.V7.App;
using Android.Content;
using Android.Runtime;
using Android.Content.PM;
using Android.Text;
using Java.Lang;
using Android;
using Android.Support.V4.App;
using Android.Graphics;
using Android.Support.V4.Content;
using Com.Yalantis.Ucrop.Model;
using Com.Yalantis.Ucrop.View;

namespace uCropQs
{
    [Activity(Label = "uCropQs", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/AppTheme")]
    public class MainActivity : BaseActivity
    {

        private const string TAG = "SampleActivity";

        private const int REQUEST_SELECT_PICTURE = 0x01;
        private const string SAMPLE_CROPPED_IMAGE_NAME = "SampleCropImage";

        private RadioGroup mRadioGroupAspectRatio, mRadioGroupCompressionSettings;
        private EditText mEditTextMaxWidth, mEditTextMaxHeight;
        private EditText mEditTextRatioX, mEditTextRatioY;
        private CheckBox mCheckBoxMaxSize;
        private SeekBar mSeekBarQuality;
        private TextView mTextViewQuality;
        private CheckBox mCheckBoxHideBottomControls;
        private CheckBox mCheckBoxFreeStyleCrop;
        private ITextWatcher mAspectRatioTextWatcher;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            SetupUI();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (resultCode == Result.Ok)
            {
                if (requestCode == REQUEST_SELECT_PICTURE)
                {
                    Android.Net.Uri selectedUri = data.Data;
                    if (selectedUri != null)
                    {
                        StartCropActivity(data.Data);
                    }
                    else
                    {
                        Toast.MakeText(this, Resource.String.toast_cannot_retrieve_selected_image, ToastLength.Short).Show();
                    }
                }
                else if (requestCode == UCrop.RequestCrop)
                {
                    HandleCropResult(data);
                }
            }
            if (resultCode.ToString().Equals(UCrop.ResultError.ToString()))
            {
                HandleCropError(data);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch (requestCode)
            {
                case REQUEST_STORAGE_READ_ACCESS_PERMISSION:
                    if (grantResults[0] == Permission.Granted)
                    {
                        PickFromGallery();
                    }

                    break;

                default:
                    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                    break;
            }
        }

        private void SetupUI()
        {
            FindViewById<Button>(Resource.Id.button_crop).Click += (s, e) =>
            {
                PickFromGallery();
            };

            FindViewById<Button>(Resource.Id.button_random_image).Click += (s, e) =>
            {
                System.Random random = new System.Random();
                int minSizePixels = 800;
                int maxSizePixels = 2400;
                StartCropActivity(Android.Net.Uri.Parse(string.Format("https://unsplash.it/{0}/{1}/?random",
                        minSizePixels + random.Next(maxSizePixels - minSizePixels),
                        minSizePixels + random.Next(maxSizePixels - minSizePixels))));
            };

            mRadioGroupAspectRatio = FindViewById<RadioGroup>(Resource.Id.radio_group_aspect_ratio);
            mRadioGroupCompressionSettings = FindViewById<RadioGroup>(Resource.Id.radio_group_compression_settings);
            mCheckBoxMaxSize = FindViewById<CheckBox>(Resource.Id.checkbox_max_size);
            mEditTextRatioX = FindViewById<EditText>(Resource.Id.edit_text_ratio_x);
            mEditTextRatioY = FindViewById<EditText>(Resource.Id.edit_text_ratio_y);
            mEditTextMaxWidth = FindViewById<EditText>(Resource.Id.edit_text_max_width);
            mEditTextMaxHeight = FindViewById<EditText>(Resource.Id.edit_text_max_height);
            mSeekBarQuality = FindViewById<SeekBar>(Resource.Id.seekbar_quality);
            mTextViewQuality = FindViewById<TextView>(Resource.Id.text_view_quality);
            mCheckBoxHideBottomControls = FindViewById<CheckBox>(Resource.Id.checkbox_hide_bottom_controls);
            mCheckBoxFreeStyleCrop = FindViewById<CheckBox>(Resource.Id.checkbox_freestyle_crop);

            mRadioGroupAspectRatio.Check(Resource.Id.radio_dynamic);
            mEditTextRatioX.AddTextChangedListener(mAspectRatioTextWatcher);
            mEditTextRatioY.AddTextChangedListener(mAspectRatioTextWatcher);
            mRadioGroupCompressionSettings.CheckedChange += (s, e) =>
            {
                mSeekBarQuality.Enabled = e.CheckedId == Resource.Id.radio_jpeg;
            };
            mRadioGroupCompressionSettings.Check(Resource.Id.radio_jpeg);
            mSeekBarQuality.Progress = UCropActivity.DefaultCompressQuality;
            mTextViewQuality.Text = string.Format(GetString(Resource.String.format_quality_d), mSeekBarQuality.Progress);
            mSeekBarQuality.ProgressChanged += (s, e) =>
            {
                mTextViewQuality.Text = string.Format(GetString(Resource.String.format_quality_d), e.Progress);
            };

            mAspectRatioTextWatcher = new UcropTextWatcher(mRadioGroupAspectRatio);
        }

        private void PickFromGallery()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M
                && ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage)
                    != Permission.Granted)
            {
                RequestPermission(Manifest.Permission.ReadExternalStorage,
                        GetString(Resource.String.permission_read_storage_rationale),
                        REQUEST_STORAGE_READ_ACCESS_PERMISSION);
            }
            else
            {
                Intent intent = new Intent();
                intent.SetType("image/*");
                intent.SetAction(Intent.ActionGetContent);
                intent.AddCategory(Intent.CategoryOpenable);
                StartActivityForResult(Intent.CreateChooser(intent, GetString(Resource.String.label_select_picture)), REQUEST_SELECT_PICTURE);
            }
        }

        private void StartCropActivity(Android.Net.Uri uri)
        {
            string destinationFileName = SAMPLE_CROPPED_IMAGE_NAME;
            switch (mRadioGroupCompressionSettings.CheckedRadioButtonId)
            {
                case Resource.Id.radio_png:
                    destinationFileName += ".png";
                    break;
                case Resource.Id.radio_jpeg:
                    destinationFileName += ".jpg";
                    break;
            }

            UCrop uCrop = UCrop.Of(uri, Android.Net.Uri.FromFile(new Java.IO.File(CacheDir, destinationFileName)));

            uCrop = BasisConfig(uCrop);
            uCrop = AdvancedConfig(uCrop);

            uCrop.Start(this);
        }

        private UCrop BasisConfig(UCrop uCrop)
        {
            switch (mRadioGroupAspectRatio.CheckedRadioButtonId)
            {
                case Resource.Id.radio_origin:
                    uCrop = uCrop.UseSourceImageAspectRatio();

                    break;

                case Resource.Id.radio_square:
                    uCrop = uCrop.WithAspectRatio(1, 1);

                    break;

                case Resource.Id.radio_dynamic:
                    // do nothing
                    break;

                default:
                    try
                    {
                        float ratioX = float.Parse(mEditTextRatioX.Text.Trim());
                        float ratioY = float.Parse(mEditTextRatioY.Text.Trim());
                        if (ratioX > 0 && ratioY > 0)
                        {
                            uCrop = uCrop.WithAspectRatio(ratioX, ratioY);
                        }
                    }
                    catch (NumberFormatException e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.Message);
                    }

                    break;
            }

            if (mCheckBoxMaxSize.Checked)
            {
                try
                {
                    int maxWidth = int.Parse(mEditTextMaxWidth.Text.Trim());
                    int maxHeight = int.Parse(mEditTextMaxHeight.Text.Trim());
                    if (maxWidth > 0 && maxHeight > 0)
                    {
                        uCrop = uCrop.WithMaxResultSize(maxWidth, maxHeight);
                    }
                }
                catch (NumberFormatException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                }
            }

            return uCrop;
        }

        private UCrop AdvancedConfig(UCrop uCrop)
        {
            UCrop.Options options = new UCrop.Options();

            switch (mRadioGroupCompressionSettings.CheckedRadioButtonId)
            {
                case Resource.Id.radio_png:
                    options.SetCompressionFormat(Bitmap.CompressFormat.Png);

                    break;

                case Resource.Id.radio_jpeg:

                default:
                    options.SetCompressionFormat(Bitmap.CompressFormat.Jpeg);

                    break;
            }
            options.SetCompressionQuality(mSeekBarQuality.Progress);

            options.SetHideBottomControls(mCheckBoxHideBottomControls.Checked);
            options.SetFreeStyleCropEnabled(mCheckBoxFreeStyleCrop.Checked);

            /*
            If you want to configure how gestures work for all UCropActivity tabs

            options.SetAllowedGestures(UCropActivity.Scale, UCropActivity.Rotate, UCropActivity.All);
            
            * */

            /*
            This sets max size for bitmap that will be decoded from source Uri.
            More size - more memory allocation, default implementation uses screen diagonal.

            options.SetMaxBitmapSize(640);

            * */


            /*

             Tune everything

             options.SetMaxScaleMultiplier(5);
             options.SetImageToCropBoundsAnimDuration(666);
             options.SetDimmedLayerColor(Color.Cyan);
             options.SetCircleDimmedLayer(true);
             options.SetShowCropFrame(false);
             options.SetCropGridStrokeWidth(20);
             options.SetCropGridColor(Color.Green);
             options.SetCropGridColumnCount(2);
             options.SetCropGridRowCount(1);

             // not found two below methods
             //options.SetToolbarCropDrawable(Resource.Drawable.your_crop_icon);
             //options.SetToolbarCancelDrawable(Resource.Drawable.your_cancel_icon);

             // Color palette
             //options.SetToolbarColor(ContextCompat.GetColor(this, Resource.Color.your_color_res));
             //options.SetStatusBarColor(ContextCompat.GetColor(this, Resource.Color.your_color_res));
             //options.SetActiveWidgetColor(ContextCompat.GetColor(this, Resource.Color.your_color_res));
             //options.SetToolbarWidgetColor(ContextCompat.GetColor(this, Resource.Color.your_color_res));

            // not found the below method
             //options.SetRootViewBackgroundColor(ContextCompat.getColor(this, R.color.your_color_res));

             // Aspect ratio options
             options.SetAspectRatioOptions(1,
                 new AspectRatio("WOW", 1, 2),
                 new AspectRatio("MUCH", 3, 4),
                 new AspectRatio("RATIO", CropImageView.DefaultAspectRatio, CropImageView.DefaultAspectRatio),
                 new AspectRatio("SO", 16, 9),
                 new AspectRatio("ASPECT", 1, 1));

            */

            return uCrop.WithOptions(options);
        }

        private void HandleCropResult(Intent result)
        {
            Android.Net.Uri resultUri = UCrop.GetOutput(result);
            if (resultUri != null)
            {
                ResultActivity.StartWithUri(this, resultUri);
            }
            else
            {
                Toast.MakeText(this, Resource.String.toast_cannot_retrieve_cropped_image, ToastLength.Short).Show();
            }
        }

        private void HandleCropError(Intent result)
        {
            Throwable cropError = UCrop.GetError(result);
            if (cropError != null)
            {
                System.Diagnostics.Debug.WriteLine(cropError.Message);
                Toast.MakeText(this, cropError.Message, ToastLength.Long).Show();
            }
            else
            {
                Toast.MakeText(this, Resource.String.toast_unexpected_error, ToastLength.Short).Show();
            }
        }

        class UcropTextWatcher : Java.Lang.Object, ITextWatcher
        {

            private RadioGroup mRadioGroupAspectRatio;

            public UcropTextWatcher(RadioGroup mRadioGroupAspectRatio)
            {
                this.mRadioGroupAspectRatio = mRadioGroupAspectRatio;
            }

            public void AfterTextChanged(IEditable s)
            {

            }

            public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
            {
                mRadioGroupAspectRatio.ClearCheck();
            }

            public void OnTextChanged(ICharSequence s, int start, int before, int count)
            {

            }

        }
    }
}

