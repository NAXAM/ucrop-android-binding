using System;
using Android.App;
using Android.Content;
using Android.OS;
using Com.Yalantis.Ucrop.View;
using System.IO;
using Android.Util;
using Android.Widget;
using Android.Views;
using Android.Content.PM;
using Android.Runtime;
using Android.Support.V4.App;
using Android;
using Android.Icu.Util;
using Java.IO;
using Java.Nio.Channels;
using Android.Support.V4.Content;
using System.Collections.Generic;

namespace uCropQs
{
    [Activity(Label = "ResultActivity")]
    public class ResultActivity : BaseActivity
    {

        private static readonly string TAG = "ResultActivity";
        private static readonly int DOWNLOAD_NOTIFICATION_ID_DONE = 911;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Result);

            try
            {
                UCropView uCropView = FindViewById<UCropView>(Resource.Id.ucrop);
                uCropView.CropImageView.SetImageUri(Intent.Data, null);
                uCropView.OverlayView.SetShowCropFrame(false);
                uCropView.OverlayView.SetShowCropGrid(false);
                uCropView.OverlayView.SetDimmedColor(Android.Graphics.Color.Transparent);
            }
            catch (Exception e)
            {
                Log.Error(TAG, "setImageUri", e);
                Toast.MakeText(this, e.Message, ToastLength.Long).Show();
            }

            Android.Graphics.BitmapFactory.Options options = new Android.Graphics.BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            Android.Graphics.BitmapFactory.DecodeFile(new Java.IO.File(Intent.Data.Path).AbsolutePath, options);

            //SetSupportActionBar(FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar));
            //if (SupportActionBar != null)
            //{
            //    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            //    SupportActionBar.Title = GetString(Resource.String.format_crop_result_d_d, options.OutWidth, options.OutHeight);
            //}
        }

        public static void StartWithUri(Context context, Android.Net.Uri uri)
        {
            Intent intent = new Intent(context, typeof(ResultActivity));
            intent.SetData(uri);
            context.StartActivity(intent);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_result, menu);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_download)
            {
                SaveCroppedImage();
            }
            else if (item.ItemId == Android.Resource.Id.Home)
            {
                OnBackPressed();
            }
            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch (requestCode)
            {
                case REQUEST_STORAGE_WRITE_ACCESS_PERMISSION:
                    if (grantResults[0] == Permission.Granted)
                    {
                        SaveCroppedImage();
                    }

                    break;

                default: base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                    break;
            }
        }

        private void SaveCroppedImage()
        {
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage)
                    != Permission.Granted)
            {
                RequestPermission(Manifest.Permission.WriteExternalStorage,
                        GetString(Resource.String.permission_write_storage_rationale),
                        REQUEST_STORAGE_WRITE_ACCESS_PERMISSION);
            }
            else
            {
                Android.Net.Uri imageUri = Intent.Data;
                if (imageUri != null && imageUri.Scheme.Equals("file"))
                {
                    try
                    {
                        CopyFileToDownloads(Intent.Data);
                    }
                    catch (Exception e)
                    {
                        Toast.MakeText(this, e.Message, ToastLength.Short).Show();
                    }
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.toast_unexpected_error), ToastLength.Short).Show();
                }
            }
        }

        private void CopyFileToDownloads(Android.Net.Uri croppedFileUri)
        {
            try
            {
                String downloadsDirectoryPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
                String filename = String.Format("%d_%s", Calendar.Instance.TimeInMillis, croppedFileUri.LastPathSegment);

                Java.IO.File saveFile = new Java.IO.File(downloadsDirectoryPath, filename);

                FileInputStream inStream = new FileInputStream(new Java.IO.File(croppedFileUri.Path));
                FileOutputStream outStream = new FileOutputStream(saveFile);
                FileChannel inChannel = inStream.Channel;
                FileChannel outChannel = outStream.Channel;
                inChannel.TransferTo(0, inChannel.Size(), outChannel);
                inStream.Close();
                outStream.Close();

                ShowNotification(saveFile);
            }
            catch(Exception e)
            {
                Toast.MakeText(this, e.Message, ToastLength.Short).Show();
            }
        }

        private void ShowNotification(Java.IO.File file)
        {
            Intent intent = new Intent(Intent.ActionView);
            intent.AddFlags(ActivityFlags.NewTask);
            Android.Net.Uri fileUri = FileProvider.GetUriForFile(
                    this,
                    GetString(Resource.String.file_provider_authorities),
                    file);

            intent.SetDataAndType(fileUri, "image/*");

            IList<ResolveInfo> resInfoList = PackageManager.QueryIntentActivities(
                    intent,
                    PackageInfoFlags.MatchDefaultOnly);

            foreach (ResolveInfo info in resInfoList)
            {
                GrantUriPermission(
                        info.ActivityInfo.PackageName,
                        fileUri, ActivityFlags.GrantWriteUriPermission | ActivityFlags.GrantReadUriPermission);
            }

            NotificationCompat.Builder mNotification = new NotificationCompat.Builder(this);

            mNotification
                    .SetContentTitle(GetString(Resource.String.app_name))
                    .SetContentText(GetString(Resource.String.notification_image_saved_click_to_preview))
                    .SetTicker(GetString(Resource.String.notification_image_saved))
                    .SetSmallIcon(Resource.Drawable.ic_done)
                    .SetOngoing(false)
                    .SetContentIntent(PendingIntent.GetActivity(this, 0, intent, 0))
                    .SetAutoCancel(true);
            ((NotificationManager)GetSystemService(NotificationService)).Notify(DOWNLOAD_NOTIFICATION_ID_DONE, mNotification.Build());
        }

    }
}