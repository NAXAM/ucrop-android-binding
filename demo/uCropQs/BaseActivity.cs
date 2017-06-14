using Android.Content;
using Android.Support.V4.App;
using Android.Support.V7.App;
using System;
using static Android.Views.View;

namespace uCropQs
{
    public class BaseActivity : AppCompatActivity
    {

        protected const int REQUEST_STORAGE_READ_ACCESS_PERMISSION = 101;
        protected const int REQUEST_STORAGE_WRITE_ACCESS_PERMISSION = 102;

        private AlertDialog mAlertDialog;


        protected override void OnStop()
        {
            base.OnStop();
            if (mAlertDialog != null && mAlertDialog.IsShowing)
            {
                mAlertDialog.Dismiss();
            }
        }

        protected void RequestPermission(string permission, string rationale, int requestCode)
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, permission))
            {
                ShowAlertDialog(GetString(Resource.String.permission_title_rationale),
                            rationale,
                            (sender, args) =>
                            {
                                ActivityCompat.RequestPermissions(this, new String[] { permission }, requestCode);
                            },
                            GetString(Resource.String.label_ok),
                            null,
                            GetString(Resource.String.label_cancel));
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new String[] { permission }, requestCode);
            }
        }

        protected void ShowAlertDialog(string title, string message, EventHandler<DialogClickEventArgs> onPositiveButtionClicked, string positiveButtonText, EventHandler<DialogClickEventArgs> onNegativeButtionClicked, string negativeButtonText)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(title);
            builder.SetMessage(message);
            builder.SetPositiveButton(positiveButtonText, onPositiveButtionClicked);
            builder.SetNegativeButton(negativeButtonText, onNegativeButtionClicked);
            mAlertDialog = builder.Show();
        }

    }
}